using Bullwhip.Items;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace Bullwhip {
	class MyPlayer : ModPlayer {
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
	}
}
