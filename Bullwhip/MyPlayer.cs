using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;
using Bullwhip.Items;


namespace Bullwhip {
	class BullwhipPlayer : ModPlayer {
		private Vector2? PullHeading = null;

		////

		public override bool CloneNewInstances => false;



		////////////////

		public override void SetupStartInventory( IList<Item> items, bool mediumcoreDeath ) {
			if( !mediumcoreDeath && BullwhipConfig.Instance.Get<bool>(nameof(BullwhipConfig.PlayerSpawnsWithWhip)) ) {
				var whip = new Item();
				whip.SetDefaults( ModContent.ItemType<BullwhipItem>() );

				items.Add( whip );
			}
		}


		////////////////

		public override void PreUpdate() {
			if( this.player.whoAmI == Main.myPlayer ) {
				this.PreUpdateLocal();
			}
		}

		private void PreUpdateLocal() {
			Item whip = this.player.HeldItem;

			if( whip?.active == true && whip.type == ModContent.ItemType<BullwhipItem>() ) {
				((BullwhipItem)whip.modItem)?.UpdateWhipForPlayer( this.player );
			}
		}

		////////////////

		public override void PreUpdateMovement() {
			if( this.PullHeading.HasValue ) {
				var config = BullwhipConfig.Instance;
				float ledgePullStr = config.Get<float>( nameof(config.WhipLedgePullStrength) );

				if( ledgePullStr > 0 ) {
					this.player.velocity -= this.PullHeading.Value * ledgePullStr;
				}

				this.PullHeading = null;
			}
		}


		////////////////

		public void SetPullHeading( Vector2 pullHeading ) {
			//if( pullHeading.LengthSquared() > 9216 ) {  //96^2
			pullHeading.Normalize();
			//pullHeading *= 96;
			//}

			this.PullHeading = pullHeading;
		}


		////////////////

		public override void ProcessTriggers( TriggersSet triggersSet ) {
			if( BullwhipMod.Instance.QuickWhip.JustPressed ) {
				if( !Main.gamePaused && !this.player.dead ) {
					BullwhipItem.QuickWhipIf( this.player, true );
				}
			}
		}
	}
}
