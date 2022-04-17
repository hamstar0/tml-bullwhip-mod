using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		private static bool ApplyWhipStrikeOnItems(
					Player player,
					Vector2 direction,
					IEnumerable<Item> hitItems,
					bool fxOnly,
					bool syncIfServer ) {
			//IDictionary<Vector2, IEnumerable<Item>> hitItemsAt ) {
			var checkedItems = new HashSet<Item>();
			bool isItemHit = false;

			//foreach( (Vector2 target, IEnumerable<Item> items) in hitItemsAt ) {
			foreach( Item item in hitItems ) {
				if( item?.active != true ) {
					continue;
				}

				//

				if( checkedItems.Contains( item ) ) {
					continue;
				}

				checkedItems.Add( item );

				//

				BullwhipItem.StrikeItem_If( player, direction, /*target,*/ item, fxOnly, syncIfServer );

				//

				isItemHit = true;
			}

			return isItemHit;
		}


		////////////////

		public static void StrikeItem_If(
					Player player,
					Vector2 direction,
					/*Vector2 hitWorldPosition,*/
					Item item,
					bool fxOnly,
					bool syncIfServer ) {
			bool _ = false;
			if( !BullwhipAPI.OnPreBullwhipEntityHit(player, item, fxOnly, ref _ ) ) {
				return;
			}

			//

			if( !fxOnly ) {
				item.Center = player.MountedCenter;

				//

				if( syncIfServer && Main.netMode == NetmodeID.Server ) {
					NetMessage.SendData( MessageID.SyncItem, -1, -1, null, item.whoAmI );
				}
			}
		}
	}
}