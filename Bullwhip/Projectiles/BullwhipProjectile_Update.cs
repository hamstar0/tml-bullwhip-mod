﻿using System;
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
			Player ownerPlr = this.projectile.GetPlayerOwner();
			if( ownerPlr == null || ownerPlr.CCed || ownerPlr.noItems || ownerPlr.dead ) {
				this.projectile.Kill();

				return;
			}

			//
			
			bool isFirstFrame = !this.IsBegun;

			if( isFirstFrame ) {
				this.IsBegun = true;

				if( Main.myPlayer != ownerPlr.whoAmI ) {
					this.UpdatePosition_If();
				}

				this.projectile.netUpdate = true;
			}

			//

			ownerPlr.heldProj = this.projectile.whoAmI;
			//ownerPlr.itemTime = ownerPlr.itemAnimation;

			//

			if( Main.netMode != NetmodeID.Server ) {
				if( Main.myPlayer == ownerPlr.whoAmI ) {
					this.UpdatePosition_If();
				} else {
					if( !isFirstFrame ) {
						this.projectile.spriteDirection = this.LastKnownSpriteDirection;
						this.projectile.rotation = this.LastKnownRotation;
					}
				}
			} else if( Main.netMode == NetmodeID.Server ) {
				if( !isFirstFrame ) {
					this.projectile.spriteDirection = this.LastKnownSpriteDirection;
					this.projectile.rotation = this.LastKnownRotation;
				}
			}

			//
			
			if( this.StepThroughFrames() ) {
				this.projectile.netUpdate = true;
			}

			//

			if( !this.IsLastFrame ) {
				if( this.projectile.timeLeft < 3 ) {
					this.projectile.timeLeft = 3;
				}
			}
		}


		////////////////

		private void UpdatePosition_If() {
			Entity owner = this.projectile.GetOwner();
			Player ownerPlr = owner as Player;
			if( ownerPlr == null ) {
				return;
			}

			// Here be dragons!

			Vector2 ownerMountedCenter = ownerPlr.RotatedRelativePoint( ownerPlr.MountedCenter, true );

			this.projectile.spriteDirection = ownerPlr.direction;
			//this.projectile.direction = ownerPlr.direction;
			//this.projectile.Center = ownerMountedCenter - (Vector2.Normalize(this.projectile.velocity) * 12f);

			//this.projectile.rotation = this.projectile.velocity.ToRotation();
			this.projectile.rotation = this.GetAimDirection( ownerPlr ).ToRotation();

			int frameCount = Main.projFrames[this.projectile.type];

			float swingPercent = (float)this.projectile.frame / (float)frameCount;
			float swingPercentDir = ownerPlr.direction > 0
				? swingPercent
				: (1f - swingPercent);
			float swingDegreesArc = 180f * swingPercentDir;
			float swingDegrees = 180f + swingDegreesArc;
			float swingRadians = MathHelper.ToRadians( swingDegrees );
			var swingDir = new Vector2( (float)Math.Cos(swingRadians), (float)Math.Sin(swingRadians) );
			var swingPos = ownerMountedCenter + (swingDir * 10f);

			this.projectile.Center = swingPos;

			// Offset by 180 degrees here
			if( this.projectile.spriteDirection == -1 ) {
				this.projectile.rotation -= MathHelper.ToRadians( 180f );
			}

			//

			this.projectile.ai[1] = this.projectile.rotation
				+ (this.projectile.spriteDirection * 1000);
		}


		////////////////

		private bool StepThroughFrames() {
			var config = BullwhipConfig.Instance;
			Projectile proj = this.projectile;
			bool stepAnim = false;

			proj.frameCounter++;

			switch( proj.frame ) {
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
				stepAnim = proj.frameCounter >= 3;
				break;
			case 5:
				stepAnim = proj.frameCounter >= 2;
				break;
			case 6:
			case 7:
				stepAnim = proj.frameCounter >= 1;
				break;
			case 8:
				stepAnim = proj.frameCounter >= 2;
				break;
			}

			if( stepAnim ) {
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

			return stepAnim;
		}
	}
}
