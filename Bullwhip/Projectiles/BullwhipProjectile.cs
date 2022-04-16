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
		internal int PlayerOwnerWho = -1;



		////////////////

		public override bool CloneNewInstances => false;

		////////////////
		
		public bool IsBegun {
			get => this.projectile.ai[0] != 0f;
			set => this.projectile.ai[0] = (value ? 1 : 0);
		}

		public bool IsLastFrame {
			get {
				int lastFrame = Main.projFrames[this.projectile.type] - 1;
				return this.projectile.frame >= lastFrame;
			}
		}

		////

		public int LastKnownSpriteDirection => (int)this.projectile.ai[1] / 1000;

		public float LastKnownRotation => this.projectile.ai[1] > 0f
			? this.projectile.ai[1] - 1000f
			: this.projectile.ai[1] + 1000f;



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

		public override void Kill( int timeLeft ) {
			if( Main.netMode == NetmodeID.Server ) {
				return;
			}

			//

			Player plr = this.projectile.GetPlayerOwner();
			if( plr?.active != true ) {
				return;
			}
			if( Main.myPlayer != plr.whoAmI ) {
				return;	// <- Local sourced only
			}

			//

			//LogHelpers.Log( "whip at "+ownerPlr.position.ToShortString()+", vel:"+this.projectile.velocity.ToString() );
			BullwhipItem.CastStrike( plr, this.projectile.velocity, false );
		}
	}
}
