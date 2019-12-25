using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using HamstarHelpers.Helpers.Debug;
using Bullwhip.Items;


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
			this.projectile.width = 384;
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

		private Vector2 GetAimDirection( Player player ) {
			/*if( this.projectile.frame > 4 ) {
				return this.projectile.velocity;
			}*/

			Vector2 mousePos = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY);

			Vector2 aim = mousePos - player.Center;
			aim.Normalize();

			this.projectile.velocity = aim;

			return aim;
		}


		////////////////

		public override void AI() {
			Player ownerPlr = Main.player[ this.projectile.owner ];
			ownerPlr.heldProj = this.projectile.whoAmI;
			//ownerPlr.itemTime = ownerPlr.itemAnimation;

			if( Main.netMode != 2 ) {
				if( Main.myPlayer == ownerPlr.whoAmI ) {
					this.UpdatePosition();
					NetMessage.SendData( MessageID.SyncProjectile, -1, -1, null, this.projectile.whoAmI );
				}
			} else {
				NetMessage.SendData( MessageID.SyncProjectile, -1, this.projectile.owner, null, this.projectile.whoAmI );
			}

			if( !ownerPlr.frozen ) {
				if( !this.IsBegun ) {
					this.IsBegun = true;
					this.projectile.netUpdate = true;
				}
				this.StepThroughFrames();
			}

			if( !this.IsLastFrame ) {
				if( this.projectile.timeLeft < 3 ) {
					this.projectile.timeLeft = 3;
				}
			}
		}

		public override void Kill( int timeLeft ) {
			Player ownerPlr = Main.player[this.projectile.owner];
//LogHelpers.Log( "whip at "+ownerPlr.position.ToShortString()+", vel:"+this.projectile.velocity.ToString() );
			BullwhipItem.CastWhipStrike( ownerPlr, this.projectile.velocity );
		}


		////////////////

		private void UpdatePosition() {
			Player ownerPlr = Main.player[this.projectile.owner];
			Vector2 ownerMountedCenter = ownerPlr.RotatedRelativePoint( ownerPlr.MountedCenter, true );

			this.projectile.direction = ownerPlr.direction;
			this.projectile.Center = ownerMountedCenter - (Vector2.Normalize(this.projectile.velocity) * 12f);
			//this.projectile.position.X = ownerMountedCenter.X - (float)( this.projectile.width / 2 );
			//this.projectile.position.Y = ownerMountedCenter.Y - (float)( this.projectile.height / 2 );

			//this.projectile.rotation = this.projectile.velocity.ToRotation();
			this.projectile.rotation = this.GetAimDirection( ownerPlr ).ToRotation();

			// Offset by 90 degrees here
			if( this.projectile.spriteDirection == -1 ) {
				this.projectile.rotation -= MathHelper.ToRadians( 90f );
			}
		}


		////////////////

		private void StepThroughFrames() {
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
					sndPos += Vector2.Normalize(proj.velocity) * BullwhipConfig.Instance.MaximumWhipHitDist;

					BullwhipItem.PlaySound( sndPos );
				}
			}
		}


		////////////////

		public override bool PreDraw( SpriteBatch spriteBatch, Color lightColor ) {
			return base.PreDraw( spriteBatch, lightColor );
		}
	}
}
