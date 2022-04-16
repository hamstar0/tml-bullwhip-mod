using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		private static bool ApplyWhipStrikeOnPlayers(
					Player player,
					Vector2 direction,
					IEnumerable<Player> hitPlayers,
					bool fxOnly ) {
			//IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt ) {
			var checkedPlayers = new HashSet<Player>();
			bool isPlayerHit = false;

			//foreach( (Vector2 target, IEnumerable<Player> plrs) in hitPlayersAt ) {
			foreach( Player plr in hitPlayers ) {
				if( checkedPlayers.Contains( plr ) ) {
					continue;
				}

				checkedPlayers.Add( plr );

				//

				BullwhipItem.StrikePlayer_If( player, direction, /*target,*/ plr, fxOnly );

				//

				isPlayerHit = true;
			}

			return isPlayerHit;
		}


		////////////////

		public static void StrikePlayer_If(
					Player player,
					Vector2 direction,
					/*Vector2 hitWorldPosition,*/
					Player targetPlr,
					bool fxOnly ) {
			if( targetPlr.dead || targetPlr.immune ) {
				return;
			}

			//

			bool _ = false;
			if( !BullwhipAPI.OnPreBullwhipEntityHit(player, targetPlr, fxOnly, ref _) ) {
				return;
			}

			//

			var config = BullwhipConfig.Instance;

			bool canPvp = player.hostile && targetPlr.hostile
				&& (targetPlr.team != player.team || player.team == 0);

			if( !canPvp && !config.Get<bool>( nameof(config.WhipIgnoresPvP) ) ) {
				return;
			}

			//

			if( !fxOnly ) {
				int dmg = config.Get<int>( nameof(BullwhipConfig.WhipDamage) );
				float kb = config.Get<float>( nameof(BullwhipConfig.WhipKnockback) );

				targetPlr.Hurt(
					damageSource: PlayerDeathReason.ByPlayer(player.whoAmI),
					Damage: dmg,
					hitDirection: player.direction,
					pvp: false,	// LUL
					quiet: Main.netMode != NetmodeID.Server
				);

				targetPlr.velocity += direction * kb;

				//

				if( Main.netMode == NetmodeID.Server ) {
					NetMessage.SendData( MessageID.SyncPlayer, -1, -1, null, targetPlr.whoAmI );
				}
			}

			//

			BullwhipItem.CreateHitEntityFx( targetPlr.Center );
		}
	}
}