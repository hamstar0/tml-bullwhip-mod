using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Classes.Errors;
using ModLibsCore.Libraries.Debug;
using ModLibsCore.Services.ProjectileOwner;
using Bullwhip.Items;


namespace Bullwhip.Projectiles {
	public partial class BullwhipProjectile : ModProjectile {
		public override void AI() {
			Player ownerPlr = Main.player[this.projectile.owner];
			ownerPlr.heldProj = this.projectile.whoAmI;
			//ownerPlr.itemTime = ownerPlr.itemAnimation;

			if( Main.netMode != NetmodeID.Server ) {
				if( Main.myPlayer == this.projectile.owner ) {
					this.UpdatePosition();
				} else {
					this.projectile.spriteDirection = (int)this.projectile.ai[1] / 1000;
					this.projectile.rotation = this.projectile.ai[1] > 0f
						? this.projectile.ai[1] - 1000f
						: this.projectile.ai[1] + 1000f;
				}
			} else if( Main.netMode == NetmodeID.Server ) {
				this.projectile.spriteDirection = (int)this.projectile.ai[1] / 1000;
				this.projectile.rotation = this.projectile.ai[1] > 0f
					? this.projectile.ai[1] - 1000f
					: this.projectile.ai[1] + 1000f;
			}

			if( !ownerPlr.frozen ) {
				if( !this.IsBegun ) {
					this.IsBegun = true;
					this.projectile.netUpdate = true;
				}

				if( this.StepThroughFrames() ) {
					this.projectile.netUpdate = true;
				}
			}

			if( !this.IsLastFrame ) {
				if( this.projectile.timeLeft < 3 ) {
					this.projectile.timeLeft = 3;
				}
			}
		}


		////////////////

		private void UpdatePosition() {
			Player ownerPlr = Main.player[ this.projectile.owner ];
			Vector2 ownerMountedCenter = ownerPlr.RotatedRelativePoint( ownerPlr.MountedCenter, true );

			this.projectile.spriteDirection = ownerPlr.direction;
			//this.projectile.direction = ownerPlr.direction;
			//this.projectile.Center = ownerMountedCenter - (Vector2.Normalize(this.projectile.velocity) * 12f);

			//this.projectile.rotation = this.projectile.velocity.ToRotation();
			this.projectile.rotation = this.GetAimDirection( ownerPlr ).ToRotation();

			float swingPercent = (float)this.projectile.frame / (float)Main.projFrames[ this.projectile.type ];
			float swingDegreesArc = 180f * (ownerPlr.direction > 0 ? swingPercent : (1f - swingPercent));
			float swingDegrees = 180 + swingDegreesArc;
			float swingRadians = MathHelper.ToRadians( swingDegrees );
			var swingDir = new Vector2( (float)Math.Cos(swingRadians), (float)Math.Sin(swingRadians) );
			var swingPos = ownerMountedCenter + (swingDir * 10f);

			this.projectile.Center = swingPos;

			// Offset by 90 degrees here
			if( this.projectile.spriteDirection == -1 ) {
				this.projectile.rotation -= MathHelper.ToRadians( 180f );
			}

			this.projectile.ai[1] = this.projectile.rotation + (this.projectile.spriteDirection * 1000);
		}


		////////////////

		private bool StepThroughFrames() {
			var config = BullwhipConfig.Instance;
			Projectile proj = this.projectile;
			bool anim = false;

			proj.frameCounter++;

			switch( proj.frame ) {
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
				anim = proj.frameCounter >= 3;
				break;
			case 5:
				anim = proj.frameCounter >= 2;
				break;
			case 6:
			case 7:
				anim = proj.frameCounter >= 1;
				break;
			case 8:
				anim = proj.frameCounter >= 2;
				break;
			}

			if( anim ) {
				proj.frameCounter = 0;

				if( !this.IsLastFrame ) {
					proj.frame++;
				}

				if( proj.frame == 4 ) {
					var sndPos = proj.Center;
					sndPos += Vector2.Normalize(proj.velocity) * config.Get<int>( nameof(BullwhipConfig.MaximumWhipHitDist) );

					BullwhipItem.PlaySound( sndPos );
				}
			}

			return anim;
		}
	}
}
