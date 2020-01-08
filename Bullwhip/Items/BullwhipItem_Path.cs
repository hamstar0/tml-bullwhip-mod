using HamstarHelpers.Helpers.Collisions;
using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.DotNET.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static void CastWhipStrike( Player player, Vector2 direction ) {
			int minWhipDist = BullwhipConfig.Instance.MinimumWhipHitDist;
			int maxWhipDist = BullwhipConfig.Instance.MaximumWhipHitDist;
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
				start: start,
				direction: direction,
				minDist: minWhipDist,
				maxDist: maxWhipDist,
				hitTileAt: out hitTileAt,
				hitPlatformAt: out hitPlatformAt,
				breakables: breakables,
				hitNpcsAt: hitNpcsAt,
				hitProjsAt: hitProjsAt,
				hitItemsAt: hitItemsAt,
				hitPlayersAt: hitPlayersAt
			);

			///

			BullwhipItem.ApplyWhipStrike(
				player: player,
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

		////

		public static bool CastWhipScanRay(
					Vector2 start,
					Vector2 direction,
					int minDist,
					int maxDist,
					out (int TileX, int TileY)? hitTileAt,
					out (int TileX, int TileY)? hitPlatformAt,
					IDictionary<int, ISet<int>> breakables,
					IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt,
					IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt,
					IDictionary<Vector2, IEnumerable<Item>> hitItemsAt,
					IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt ) {
			var hitTiles = new HashSet<(int, int)>();
			(int TileX, int TileY)? myHitTileAt = null;
			(int TileX, int TileY)? myHitPlatformAt = null;

			bool hitNpc = false;
			bool hitProj = false;
			bool checkPerUnit( Vector2 wldPos ) {
				BullwhipItem.CheckCollisionPerUnit(
					start,
					wldPos,
					minDist,
					hitNpcsAt,
					hitProjsAt,
					hitItemsAt,
					hitPlayersAt,
					ref hitNpc,
					ref hitProj
				);
				return false;
			};

			bool checkPerTile( int tileX, int tileY ) {
				bool isTile, isPlatform, isBreakable;
				isTile = BullwhipItem.FindWhipTileCollisionAt( tileX, tileY, out isPlatform, out isBreakable );
				IEnumerable<(int TileX, int TileY)> myBreakables = BullwhipItem.FindNearbyBreakableTiles( tileX, tileY );

				// Account for adjacent breakable tiles
				foreach( var xy in myBreakables ) {
					breakables.Set2D( xy.TileX, xy.TileY );
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
					breakables.Set2D( tileX, tileY );
				}

				return isTile;
			};

			///

			bool found = CollisionHelpers.CastRay(
				worldPosition: start,
				direction: direction,
				maxWorldDistance: maxDist,
				checkPerUnit: checkPerUnit,
				checkPerTile: checkPerTile
			);

			hitPlatformAt = myHitPlatformAt;
			hitTileAt = myHitTileAt;
			return found;
		}


		////////////////

		private static void CheckCollisionPerUnit(
					Vector2 start,
					Vector2 wldPos,
					int minDist,
					IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt,
					IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt,
					IDictionary<Vector2, IEnumerable<Item>> hitItemsAt,
					IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt,
					ref bool hitNpc,
					ref bool hitProj ) {
			int minDistSqr = minDist * minDist;
			float distSqr = Vector2.DistanceSquared( start, wldPos );
			if( distSqr < minDistSqr ) {
				return;
			}

			int maxHits = BullwhipConfig.Instance.MaxWhipEntityHits;
			maxHits = maxHits == 0 ? Int32.MaxValue - 1 : maxHits;

			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickDust( wldPos, Color.Yellow );
			}

			if( !hitNpc ) {
				hitNpcsAt[wldPos] = BullwhipItem.FindWhipNpcCollisionAt( wldPos );
			}
			if( !hitProj ) {
				hitProjsAt[wldPos] = BullwhipItem.FindWhipProjectileCollisionAt( wldPos );
			}
			hitItemsAt[wldPos] = BullwhipItem.FindWhipItemCollisionAt( wldPos );
			hitPlayersAt[wldPos] = BullwhipItem.FindWhipPlayerCollisionAt( wldPos );

			hitNpc = hitNpc		|| (hitNpcsAt[wldPos].ToArray().Length >= maxHits);
			hitProj = hitProj	|| (hitProjsAt[wldPos].ToArray().Length >= maxHits);
		}
	}
}