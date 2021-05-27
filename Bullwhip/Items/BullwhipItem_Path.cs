using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;
using ModLibsCore.Libraries.DotNET.Extensions;
using ModLibsGeneral.Libraries.Collisions;

namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		/// <summary>
		/// Casts a whip strike in a given direction. First step in the whip attack process.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="direction"></param>
		public static void CastWhipStrike( Player player, Vector2 direction ) {
			int minWhipDist = BullwhipConfig.Instance.Get<int>( nameof(BullwhipConfig.MinimumWhipHitDist) );
			int maxWhipDist = BullwhipConfig.Instance.Get<int>( nameof(BullwhipConfig.MaximumWhipHitDist) );
			direction.Normalize();

			Vector2 start = player.RotatedRelativePoint( player.MountedCenter, true );
			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickDust( start, Color.Lime );
			}

			(int TileX, int TileY)? hitTileAt;
			(int TileX, int TileY)? hitPlatformAt;
			IDictionary<int, ISet<int>> breakables = new Dictionary<int, ISet<int>>();
			IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt = new Dictionary<Vector2, IEnumerable<NPC>>();
			IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt = new Dictionary<Vector2, IEnumerable<Projectile>>();
			IDictionary<Vector2, IEnumerable<Item>> hitItemsAt = new Dictionary<Vector2, IEnumerable<Item>>();
			IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt = new Dictionary<Vector2, IEnumerable<Player>>();

			///

			bool hitAnything = BullwhipItem.CastWhipScanRay(
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

			///

			BullwhipItem.ApplyWhipStrike(
				whipOwner: player,
				start: start,
				direction: direction,
				breakables: breakables,
				hitTileAt: hitTileAt,
				hitPlatformAt: hitPlatformAt,
				hitNpcsAt: hitNpcsAt,
				hitProjsAt: hitProjsAt,
				hitItemsAt: hitItemsAt,
				hitPlayersAt: hitPlayersAt
			);
		}


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
		public static bool CastWhipScanRay(
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

			bool checkPerUnit( Vector2 wldPos ) {
				// Once we've hit a tile, only continue checking for items
				if( isTileHit && !isCastStoppedExceptItems ) {
					isCastStoppedExceptItems = true;
					currHitNpcsAt = null;
					currHitProjsAt = null;
					currHitPlayersAt = null;
				}

				BullwhipItem.CheckCollisionPerUnit(
					whipOwner: whipOwner,
					start: start,
					wldPos: wldPos,
					minDist: minDist,
					hitNpcsAt: ref currHitNpcsAt,
					hitProjsAt: ref currHitProjsAt,
					hitItemsAt: ref currHitItemsAt,
					hitPlayersAt: ref currHitPlayersAt,
					hitNpc: out bool hitNpc,
					hitProj: out bool hitProj
				);

				if( hitNpc ) {
					currHitNpcsAt = null;
				}
				if( hitProj ) {
					currHitProjsAt = null;
				}

				return false;
			};

			//

			var hitTiles = new HashSet<(int, int)>();
			(int TileX, int TileY)? myHitTileAt = null;
			(int TileX, int TileY)? myHitPlatformAt = null;

			var currBreakables = breakables;

			bool checkPerTile( int tileX, int tileY ) {
				if( isTileHit ) {
					return false;
				}

				bool isTile, isPlatform, isBreakable;
				isTile = BullwhipItem.FindWhipTileCollisionAt( tileX, tileY, out isPlatform, out isBreakable );
				IEnumerable<(int TileX, int TileY)> myBreakables = BullwhipItem.FindNearbyBreakableTiles( tileX, tileY );

				// Account for adjacent breakable tiles
				foreach( (int breakableTileX, int breakableTileY) in myBreakables ) {
					currBreakables.Set2D( breakableTileX, breakableTileY );
				}

				if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
					Dust.QuickDust( new Point(tileX, tileY), isTile ? Color.Red : Color.Lime );
				}

				if( isTile ) {
					isTile = hitTiles.Contains( (tileX, tileY) );
					if( isTile ) {
						myHitTileAt = (tileX, tileY);
					}
					hitTiles.Add( (tileX, tileY) );
				}
				if( isPlatform && !myHitPlatformAt.HasValue ) {
					myHitPlatformAt = (tileX, tileY);
				}
				if( isBreakable ) {
					isTile = false;
					currBreakables.Set2D( tileX, tileY );
				}

				isTileHit = isTile;
				return false;   //used to use isTile
			};

			//

			bool found = CollisionLibraries.CastRay(
				worldPosition: start,
				direction: direction,
				maxWorldDistance: maxDist,
				bothChecksOnly: false,
				checkPerUnit: checkPerUnit,
				checkPerTile: checkPerTile
			);

			hitPlatformAt = myHitPlatformAt;
			hitTileAt = myHitTileAt;
			return found;
		}
	}
}