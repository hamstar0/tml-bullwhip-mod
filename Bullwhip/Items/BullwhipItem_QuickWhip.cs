using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Services.ProjectileOwner;
using ModLibsCore.Services.Timers;
using Bullwhip.Projectiles;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static bool QuickWhipIf( Player player, bool sync ) {
			int whipType = ModContent.ItemType<BullwhipItem>();
			Item[] plrInv = player.inventory;
			int? whipIdx = plrInv
				.Select( (item, idx) => (int?)idx )
				.FirstOrDefault( idx => plrInv[idx.Value]?.IsAir == false && plrInv[idx.Value].type == whipType );

			if( !whipIdx.HasValue ) {
				return false;
			}

			if( player.itemTime > 0 || player.itemAnimation > 0 ) {
				return false;
			}

			if( player.heldProj != -1 ) {
				return false;
			}

			int whipProj = ModContent.ProjectileType<BullwhipProjectile>();
			if( Main.projectile.Any( p =>
						p?.active == true
						&& p.type == whipProj
						&& p.GetOwner() is Player
						&& p.GetOwner().whoAmI == player.whoAmI ) ) {
				return false;
			}

			//

			int oldSelItem = player.selectedItem;

			int projWho = Projectile.NewProjectile(
				position: player.MountedCenter,
				velocity: player.velocity,
				Type: whipProj,
				Damage: 1,
				KnockBack: 1f,
				Owner: player.whoAmI
			);

			player.selectedItem = whipIdx.Value;
			
			player.itemAnimation = 32;	//45;
			player.itemTime = 60;

			//

			Timers.SetTimer( "BullwhipQuickwhipRecover", 5, false, () => {
				if( player.itemTime > 0 || player.itemAnimation > 0 ) {
					return true;
				}

				player.selectedItem = oldSelItem;

				return false;
			} );

			//

			if( projWho >= 0 && Main.projectile[projWho].type == whipProj ) {
				ProjectileOwner.SetOwnerManually( Main.projectile[projWho], player );

				if( sync && Main.netMode != NetmodeID.SinglePlayer ) {
					NetMessage.SendData( MessageID.SyncProjectile, -1, -1, null, projWho );
				}
			}

			return true;
		}
	}
}