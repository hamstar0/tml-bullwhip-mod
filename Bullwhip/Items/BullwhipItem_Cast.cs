using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;
using Bullwhip.Packets;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		/// <summary>
		/// Casts a whip strike in a given direction. First step in the whip attack process.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="direction"></param>
		/// <param name="fxOnly"></param>
		/// <param name="syncWholeStrikeAction">After deciding how the strike should work, this indicates whether
		/// to sync it to the server and/or other players.</param>
		public static void CastStrike( Player player, Vector2 direction, bool fxOnly, bool syncWholeStrikeAction ) {
			int minWhipDist = BullwhipConfig.Instance.Get<int>( nameof( BullwhipConfig.MinimumWhipHitDist ) );
			int maxWhipDist = BullwhipConfig.Instance.Get<int>( nameof( BullwhipConfig.MaximumWhipHitDist ) );
			direction.Normalize();

			//

			Vector2 start = player.RotatedRelativePoint( player.MountedCenter, true );
			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickDust( start, Color.Lime );
			}

			//

			(int TileX, int TileY)? hitTileAt;
			(int TileX, int TileY)? hitPlatformAt;
			IDictionary<int, ISet<int>> breakables = new Dictionary<int, ISet<int>>();
			IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt = new Dictionary<Vector2, IEnumerable<NPC>>();
			IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt = new Dictionary<Vector2, IEnumerable<Projectile>>();
			IDictionary<Vector2, IEnumerable<Item>> hitItemsAt = new Dictionary<Vector2, IEnumerable<Item>>();
			IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt = new Dictionary<Vector2, IEnumerable<Player>>();

			//

			bool hitAnything = BullwhipItem.CastStrikeScanRay(
				whipOwner: player,
				start: start,
				direction: direction,
				minDist: minWhipDist,
				maxDist: maxWhipDist,
				hitTileAt: out hitTileAt,
				hitPlatformAt: out hitPlatformAt,
				breakables: ref breakables,
				hitNpcsAt: ref hitNpcsAt,
				hitProjsAt: ref hitProjsAt,
				hitItemsAt: ref hitItemsAt,
				hitPlayersAt: ref hitPlayersAt
			);

			IEnumerable<NPC> hitNpcs = hitNpcsAt.SelectMany( kv => kv.Value );
			IEnumerable<Projectile> hitProjs = hitProjsAt.SelectMany( kv => kv.Value );
			IEnumerable<Item> hitItems = hitItemsAt.SelectMany( kv => kv.Value );
			IEnumerable<Player> hitPlayers = hitPlayersAt.SelectMany( kv => kv.Value );

			//

			BullwhipItem.ApplyStrike(
				whipOwner: player,
				start: start,
				direction: direction,
				breakables: breakables,
				hitTileAt: hitTileAt,
				hitPlatformAt: hitPlatformAt,
				hitNpcs: hitNpcs,
				hitProjs: hitProjs,
				hitItems: hitItems,
				hitPlayers: hitPlayers,
				fxOnly: fxOnly,
				syncSpecificHitsIfServer: syncWholeStrikeAction
			);

			//

			if( syncWholeStrikeAction ) {
				if( Main.netMode == NetmodeID.MultiplayerClient ) {
					BullwhipHitsPacket.BroadcastFromClient(
						player: player,
						start: start,
						direction: direction,
						hitTileAt: hitTileAt,
						hitPlatformAt: hitPlatformAt,
						breakables: breakables,
						hitNpcs: hitNpcs,
						hitProjs: hitProjs,
						hitItems: hitItems,
						hitPlayers: hitPlayers,
						fxOnly: fxOnly,
						fxOnlyToClients: true
					);
				} else if( Main.netMode == NetmodeID.Server ) {
					BullwhipHitsPacket.BroadcastFromServer(
						player: player,
						start: start,
						direction: direction,
						hitTileAt: hitTileAt,
						hitPlatformAt: hitPlatformAt,
						breakables: breakables,
						hitNpcs: hitNpcs,
						hitProjs: hitProjs,
						hitItems: hitItems,
						hitPlayers: hitPlayers,
						fxOnly: fxOnly,
						fxOnlyToClients: true
					);
				}
			}
		}
	}
}