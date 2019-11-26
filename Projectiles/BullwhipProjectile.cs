using Bullwhip.Items;
using HamstarHelpers.Helpers.DotNET.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;


namespace Bullwhip.Projectiles {
	public class BullwhipProjectile : ModProjectile {
		public override bool CloneNewInstances => false;

		////////////////

		public bool IsBegun {
			get => this.projectile.ai[0] != 0f;
			set => this.projectile.ai[0] = (value ? 1 : 0);
		}

		public bool IsLastFrame => this.projectile.frame >= (Main.projFrames[this.projectile.type] - 1);



		////////////////

		public override void SetStaticDefaults() {
			this.DisplayName.SetDefault( "Bullwhip" );
			Main.projFrames[ this.projectile.type ] = 9;
		}

		public override void SetDefaults() {
			this.projectile.width = 256;
			this.projectile.height = 72;
			this.projectile.aiStyle = -1;   // = 19;
			this.projectile.penetrate = -1;

			this.projectile.alpha = 0;

			this.projectile.melee = true;
			//this.projectile.friendly = true;
			//this.projectile.hide = true;
			this.projectile.ownerHitCheck = true;
			this.projectile.tileCollide = false;

			this.projectile.timeLeft = 3;
			this.drawHeldProjInFrontOfHeldItemAndArms = true;
		}


		////////////////

		public override void AI() {
			Player ownerPlr = Main.player[ this.projectile.owner ];
			ownerPlr.heldProj = this.projectile.whoAmI;
			//ownerPlr.itemTime = ownerPlr.itemAnimation;

			this.UpdatePosition();

			if( !ownerPlr.frozen ) {
				if( !this.IsBegun ) {
					this.IsBegun = true;
					this.projectile.netUpdate = true;
				}
				this.WalkFrames();
			}

			if( !this.IsLastFrame ) {
				if( this.projectile.timeLeft < 3 ) {
					this.projectile.timeLeft = 3;
				}
			} else {
				if( this.projectile.timeLeft == 1 ) {
					BullwhipItem.AttemptWhipStrike( ownerPlr, this.projectile.velocity );
				}
			}
		}

		private void UpdatePosition() {
			Player ownerPlr = Main.player[this.projectile.owner];
			Vector2 ownerMountedCenter = ownerPlr.RotatedRelativePoint( ownerPlr.MountedCenter, true );

			this.projectile.direction = ownerPlr.direction;
			this.projectile.Center = ownerMountedCenter - (Vector2.Normalize(this.projectile.velocity) * 12f);
			//this.projectile.position.X = ownerMountedCenter.X - (float)( this.projectile.width / 2 );
			//this.projectile.position.Y = ownerMountedCenter.Y - (float)( this.projectile.height / 2 );

			this.projectile.rotation = this.projectile.velocity.ToRotation();// + MathHelper.ToRadians( 135f );

			// Offset by 90 degrees here
			if( this.projectile.spriteDirection == -1 ) {
				this.projectile.rotation -= MathHelper.ToRadians( 90f );
			}
		}


		////////////////

		private void WalkFrames() {
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
			}
		}


		////////////////

		public override bool PreDraw( SpriteBatch spriteBatch, Color lightColor ) {
			return base.PreDraw( spriteBatch, lightColor );
		}
	}
}
