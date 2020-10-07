using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using HamstarHelpers.Helpers.Debug;


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
					Vector2 start,
					Vector2 wldPos,
					int minDist,
					ref IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt,
					ref IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt,
					ref IDictionary<Vector2, IEnumerable<Item>> hitItemsAt,
					ref IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt,
					ref bool hitNpc,
					ref bool hitProj ) {
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

			if( !hitNpc ) {
				hitNpcsAt[wldPos] = BullwhipItem.FindWhipNpcCollisionAt( wldPos );
			}
			if( !hitProj ) {
				hitProjsAt[wldPos] = BullwhipItem.FindWhipProjectileCollisionAt( wldPos );
			}
			hitItemsAt[wldPos] = BullwhipItem.FindWhipItemCollisionAt( wldPos );
			hitPlayersAt[wldPos] = BullwhipItem.FindWhipPlayerCollisionAt( wldPos );

			hitNpc = hitNpc || ( hitNpcsAt[wldPos].ToArray().Length >= maxHits );
			hitProj = hitProj || ( hitProjsAt[wldPos].ToArray().Length >= maxHits );
		}
	}
}