using Bullwhip.Items;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace Bullwhip {
	class BullwhipPlayer : ModPlayer {
		private Vector2? PullHeading = null;

		////

		public override bool CloneNewInstances => false;



		////////////////

		public override void SetupStartInventory( IList<Item> items, bool mediumcoreDeath ) {
			if( !mediumcoreDeath && BullwhipConfig.Instance.PlayerSpawnsWithWhip ) {
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

			if( (whip?.active ?? false) && whip.type == ModContent.ItemType<BullwhipItem>() ) {
				((BullwhipItem)whip.modItem)?.UpdateWhip( this.player );
			}
		}

		////////////////

		public override void PreUpdateMovement() {
			if( this.PullHeading.HasValue ) {
				this.player.velocity -= this.PullHeading.Value * BullwhipConfig.Instance.WhipLedgePullStrength;
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
	}
}
