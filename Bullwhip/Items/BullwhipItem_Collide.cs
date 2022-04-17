using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static Rectangle GetRectangle( Entity ent, int minBoxDim ) {
			var rect = new Rectangle( (int)ent.position.X, (int)ent.position.Y, ent.width, ent.height );

			if( rect.Width < minBoxDim ) {
				rect.X = (rect.X + (rect.Width / 2)) - (minBoxDim / 2);
				rect.Width = minBoxDim;
			}
			if( rect.Height < minBoxDim ) {
				rect.Y = (rect.Y + (rect.Height / 2)) - (minBoxDim / 2);
				rect.Height = minBoxDim;
			}

			return rect;
		}


		////////////////

		/// <summary>
		/// Performs a collision check at a given world position. A step along the way of a ray cast.
		/// </summary>
		/// <param name="whipOwner"></param>
		/// <param name="start"></param>
		/// <param name="wldPos"></param>
		/// <param name="minDist"></param>
		/// <param name="hitNpcsAt"></param>
		/// <param name="hitProjsAt"></param>
		/// <param name="hitItemsAt"></param>
		/// <param name="hitPlayersAt"></param>
		/// <param name="hitNpc"></param>
		/// <param name="hitProj"></param>
		private static void CheckCollisionPerUnit(
					Player whipOwner,
					Vector2 start,
					Vector2 wldPos,
					int minDist,
					ref IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt,
					ref IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt,
					ref IDictionary<Vector2, IEnumerable<Item>> hitItemsAt,
					ref IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt,
					out bool hitNpc,
					out bool hitProj ) {
			hitNpc = false;
			hitProj = false;

			var config = BullwhipConfig.Instance;
			int minDistSqr = minDist * minDist;
			float distSqr = Vector2.DistanceSquared( start, wldPos );
			if( distSqr < minDistSqr ) {
				return;
			}

			int maxHits = config.Get<int>( nameof( BullwhipConfig.MaxWhipEntityHits ) );
			maxHits = maxHits == 0 ? Int32.MaxValue - 1 : maxHits;

			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickDust( wldPos, Color.Yellow );
			}

			if( hitNpcsAt != null ) {
				hitNpcsAt[wldPos] = BullwhipItem.FindStrikeNpcCollisionAt( wldPos );
				hitNpc = hitNpcsAt[wldPos].ToArray().Length >= maxHits;
			}
			if( hitProjsAt != null ) {
				hitProjsAt[wldPos] = BullwhipItem.FindStrikeProjectileCollisionAt( wldPos );
				hitProj = hitProjsAt[wldPos].ToArray().Length >= maxHits;
			}
			if( hitItemsAt != null ) {
				hitItemsAt[wldPos] = BullwhipItem.FindStrikeItemCollisionAt( wldPos );
			}
			if( hitPlayersAt != null ) {
				hitPlayersAt[wldPos] = BullwhipItem.FindStrikePlayerCollisionAt( whipOwner, wldPos );
			}
		}
	}
}