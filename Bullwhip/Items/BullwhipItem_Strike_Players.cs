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
					bool fxOnly,
					bool syncIfServer ) {
			//IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt ) {
			var checkedPlayers = new HashSet<Player>();
			bool isPlayerHit = false;

			//foreach( (Vector2 target, IEnumerable<Player> plrs) in hitPlayersAt ) {
			foreach( Player plr in hitPlayers ) {
				if( plr?.active != true ) {
					continue;
				}

				//

				if( checkedPlayers.Contains( plr ) ) {
					continue;
				}

				checkedPlayers.Add( plr );

				//

				BullwhipItem.StrikePlayer_If( player, direction, /*target,*/ plr, fxOnly, syncIfServer );

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
					bool fxOnly,
					bool syncIfServer ) {
			if( targetPlr.dead || targetPlr.immune ) {
				return;
			}

			//

			bool _ = false;
			if( !BullwhipAPI.OnPreBullwhipEntityHit(player, targetPlr, fxOnly, ref _) ) {
				return;
			}

			//

			if( !fxOnly ) {
				BullwhipItem.StrikePlayerNoFx( player, direction, targetPlr, syncIfServer );
			}

			//

			BullwhipItem.CreateHitEntityFx( targetPlr.Center );
		}


		////////////////

		public static void StrikePlayerNoFx(
					Player player,
					Vector2 direction,
					/*Vector2 hitWorldPosition,*/
					Player targetPlr,
					bool syncIfServer ) {
			var config = BullwhipConfig.Instance;

			bool ignorePvp = config.Get<bool>( nameof(config.WhipIgnoresPvP) );
			bool onlyHitsIfPvp = !ignorePvp;

			//

			int dmg = config.Get<int>( nameof(BullwhipConfig.WhipDamage) );
			float kb = config.Get<float>( nameof(BullwhipConfig.WhipKnockback) );
			PlayerDeathReason reason = PlayerDeathReason.ByPlayer( player.whoAmI );

			bool syncHurt = syncIfServer && Main.netMode == NetmodeID.Server;

			//

			targetPlr.Hurt(
				damageSource: reason,
				Damage: dmg,
				hitDirection: player.direction,
				pvp: onlyHitsIfPvp,	// LUL
				quiet: !syncHurt
			);

			//

			if( syncHurt ) {
				NetMessage.SendPlayerHurt( targetPlr.whoAmI, reason, dmg, player.direction, false, onlyHitsIfPvp, -1 );
			}

			//

			targetPlr.velocity += direction * kb;

			if( syncHurt ) {
				NetMessage.SendData( MessageID.SyncPlayer, -1, -1, null, targetPlr.whoAmI );
			}
		}
	}
}