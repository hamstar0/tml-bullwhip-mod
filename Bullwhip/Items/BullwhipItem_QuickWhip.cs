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
			Item whipItem = player.inventory
				.FirstOrDefault( i => i?.IsAir == false && i.type == whipType );

			if( whipItem == null ) {
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

			if( Timers.GetTimerTickDuration("BullwhipQuickwhipCooldown") >= 1 ) {
				return false;
			}
			Timers.SetTimer( "BullwhipQuickwhipCooldown", 32, false, () => false );

			//

			int projWho = Projectile.NewProjectile(
				position: player.MountedCenter,
				velocity: player.velocity,
				Type: whipProj,
				Damage: 1,
				KnockBack: 1f,
				Owner: player.whoAmI
			);

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