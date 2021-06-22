using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Bullwhip.Projectiles;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static bool QuickWhipIf( Player player, bool sync ) {
			int whipType = ModContent.ItemType<BullwhipItem>();
			Item whipItem = player.inventory
				.FirstOrDefault( i => i?.IsAir == false && i.type == whipType );

			if( whipItem == null ) {
				return false;
			}

			if( player.itemTime > 0 ) {
				return false;
			}

			if( player.HeldItem?.IsAir == false || player.heldProj != -1 ) {
				return false;
			}

			int projWho = Projectile.NewProjectile(
				position: player.MountedCenter,
				velocity: player.velocity,
				Type: ModContent.ProjectileType<BullwhipProjectile>(),
				Damage: 1,
				KnockBack: 1f,
				Owner: player.whoAmI
			);

			if( sync && Main.netMode != NetmodeID.SinglePlayer ) {
				NetMessage.SendData( MessageID.SyncProjectile, -1, -1, null, projWho );
			}

			return true;
		}
	}
}