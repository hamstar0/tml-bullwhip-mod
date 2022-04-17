using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;
using ModLibsCore.Libraries.DotNET.Extensions;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
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
					int tileX,
					int tileY,
					ref bool isTileHit,
					ref ISet<(int x, int y)> hitTiles,
					ref (int TileX, int TileY)? hitTileAt,
					ref (int TileX, int TileY)? hitPlatformAt,
					ref IDictionary<int, ISet<int>> breakables ) {
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