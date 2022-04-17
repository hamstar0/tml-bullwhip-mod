using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;
using ModLibsCore.Libraries.DotNET.Extensions;
using ModLibsGeneral.Libraries.Collisions;
using Bullwhip.Packets;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		/// <summary>
		/// Casts a whip strike in a given direction. First step in the whip attack process.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="direction"></param>
		/// <param name="fxOnly"></param>
		/// <param name="syncStrikeAction">After deciding how the strike should work, this indicates whether to sync it
		/// to the server and/or other players.</param>
		public static void CastStrike( Player player, Vector2 direction, bool fxOnly, bool syncStrikeAction ) {
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
				syncSpecificHitsIfServer: syncStrikeAction
			);

			//

			if( syncStrikeAction ) {
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


		////////////////

		/// <summary>
		/// Casts a scan ray to determine what tile, breakable tiles, npcs, projectiles, items, or players get hit as if by
		/// a whip strike.
		/// </summary>
		/// <param name="whipOwner"></param>
		/// <param name="start"></param>
		/// <param name="direction"></param>
		/// <param name="minDist"></param>
		/// <param name="maxDist"></param>
		/// <param name="hitTileAt"></param>
		/// <param name="hitPlatformAt"></param>
		/// <param name="breakables"></param>
		/// <param name="hitNpcsAt"></param>
		/// <param name="hitProjsAt"></param>
		/// <param name="hitItemsAt"></param>
		/// <param name="hitPlayersAt"></param>
		/// <returns>`true` if something gets hit.</returns>
		public static bool CastStrikeScanRay(
					Player whipOwner,
					Vector2 start,
					Vector2 direction,
					int minDist,
					int maxDist,
					out (int TileX, int TileY)? hitTileAt,
					out (int TileX, int TileY)? hitPlatformAt,
					ref IDictionary<int, ISet<int>> breakables,
					ref IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt,
					ref IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt,
					ref IDictionary<Vector2, IEnumerable<Item>> hitItemsAt,
					ref IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt ) {
			bool isTileHit = false;
			bool isCastStoppedExceptItems = false;

			//

			var currHitNpcsAt = hitNpcsAt;
			var currHitProjsAt = hitProjsAt;
			var currHitItemsAt = hitItemsAt;
			var currHitPlayersAt = hitPlayersAt;

			bool CheckPerUnit( Vector2 wldPos ) {
				return BullwhipItem.CastStrikeScanPerUnit(
					whipOwner: whipOwner,
					start: start,
					direction: direction,
					minDist: minDist,
					maxDist: maxDist,
					wldPos: wldPos,
					isTileHit: ref isTileHit,
					isCastStoppedExceptItems: ref isCastStoppedExceptItems,
					hitNpcsAt: ref currHitNpcsAt,
					hitProjsAt: ref currHitProjsAt,
					hitItemsAt: ref currHitItemsAt,
					hitPlayersAt: ref currHitPlayersAt
				);
			};

			//

			ISet<(int x, int y)> hitTiles = new HashSet<(int, int)>();
			(int TileX, int TileY)? myHitTileAt = null;
			(int TileX, int TileY)? myHitPlatformAt = null;

			var currBreakables = breakables;

			bool CheckPerTile( int tileX, int tileY ) {
				return BullwhipItem.CastStrikeScanPerTile(
					whipOwner: whipOwner,
					start: start,
					direction: direction,
					minDist: minDist,
					maxDist: maxDist,
					tileX: tileX,
					tileY: tileY,
					isTileHit: ref isTileHit,
					hitTiles: ref hitTiles,
					hitTileAt: ref myHitTileAt,
					hitPlatformAt: ref myHitPlatformAt,
					breakables: ref currBreakables,
					hitNpcsAt: ref currHitNpcsAt,
					hitProjsAt: ref currHitProjsAt,
					hitItemsAt: ref currHitItemsAt,
					hitPlayersAt: ref currHitPlayersAt
				);
			};

			//

			bool found = CollisionLibraries.CastRay(
				worldPosition: start,
				direction: direction,
				maxWorldDistance: maxDist,
				bothChecksOnly: false,
				checkPerUnit: CheckPerUnit,
				checkPerTile: CheckPerTile
			);

			hitPlatformAt = myHitPlatformAt;
			hitTileAt = myHitTileAt;
			return found;
		}


		////

		private static bool CastStrikeScanPerUnit(
					Player whipOwner,
					Vector2 start,
					Vector2 direction,
					int minDist,
					int maxDist,
					Vector2 wldPos,
					ref bool isTileHit,
					ref bool isCastStoppedExceptItems,
					ref IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt,
					ref IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt,
					ref IDictionary<Vector2, IEnumerable<Item>> hitItemsAt,
					ref IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt ) {
			// Once we've hit a tile, only continue checking for items
			if( isTileHit && !isCastStoppedExceptItems ) {
				isCastStoppedExceptItems = true;
				hitNpcsAt = null;
				hitProjsAt = null;
				hitPlayersAt = null;
			}

			//

			BullwhipItem.CheckCollisionPerUnit(
				whipOwner: whipOwner,
				start: start,
				wldPos: wldPos,
				minDist: minDist,
				hitNpcsAt: ref hitNpcsAt,
				hitProjsAt: ref hitProjsAt,
				hitItemsAt: ref hitItemsAt,
				hitPlayersAt: ref hitPlayersAt,
				hitNpc: out bool hitNpc,
				hitProj: out bool hitProj
			);

			if( hitNpc ) {
				hitNpcsAt = null;
			}
			if( hitProj ) {
				hitProjsAt = null;
			}

			//

			return false;
		}


		private static bool CastStrikeScanPerTile(
					Player whipOwner,
					Vector2 start,
					Vector2 direction,
					int minDist,
					int maxDist,
					int tileX,
					int tileY,
					ref bool isTileHit,
					ref ISet<(int x, int y)> hitTiles,
					ref (int TileX, int TileY)? hitTileAt,
					ref (int TileX, int TileY)? hitPlatformAt,
					ref IDictionary<int, ISet<int>> breakables,
					ref IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt,
					ref IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt,
					ref IDictionary<Vector2, IEnumerable<Item>> hitItemsAt,
					ref IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt ) {
			if( isTileHit ) {
				return false;
			}

			//

			bool isTile, isPlatform, isBreakable;
			isTile = BullwhipItem.FindStrikeTileCollisionAt( tileX, tileY, out isPlatform, out isBreakable );

			//

			IEnumerable<(int TileX, int TileY)> myBreakables = BullwhipItem.FindNearbyBreakableTiles( tileX, tileY );

			// Account for adjacent breakable tiles
			foreach( (int breakableTileX, int breakableTileY) in myBreakables ) {
				breakables.Set2D( breakableTileX, breakableTileY );
			}

			//

			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickDust( new Point( tileX, tileY ), isTile ? Color.Red : Color.Lime );
			}

			//

			if( isTile ) {
				isTile = hitTiles.Contains( (tileX, tileY) );
				if( isTile ) {
					hitTileAt = (tileX, tileY);
				}
				hitTiles.Add( (tileX, tileY) );
			}
			if( isPlatform && !hitPlatformAt.HasValue ) {
				hitPlatformAt = (tileX, tileY);
			}
			if( isBreakable ) {
				isTile = false;
				breakables.Set2D( tileX, tileY );
			}

			//

			isTileHit = isTile;
			return false;   //used to use isTile
		}
	}
}