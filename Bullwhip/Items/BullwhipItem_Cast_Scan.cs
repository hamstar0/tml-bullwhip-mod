using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;
using ModLibsGeneral.Libraries.Collisions;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
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

			IDictionary<Vector2, IEnumerable<NPC>> currHitNpcsAt = hitNpcsAt;
			IDictionary<Vector2, IEnumerable<Projectile>> currHitProjsAt = hitProjsAt;
			IDictionary<Vector2, IEnumerable<Item>> currHitItemsAt = hitItemsAt;
			IDictionary<Vector2, IEnumerable<Player>> currHitPlayersAt = hitPlayersAt;

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
					tileX: tileX,
					tileY: tileY,
					isTileHit: ref isTileHit,
					hitTiles: ref hitTiles,
					hitTileAt: ref myHitTileAt,
					hitPlatformAt: ref myHitPlatformAt,
					breakables: ref currBreakables
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
	}
}