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
			int minWhipDist = BullwhipConfig.Instance.MinimumWhipDist;
			int maxWhipDist = BullwhipConfig.Instance.MaximumWhipDist;
			direction.Normalize();

			Vector2 start = player.RotatedRelativePoint( player.MountedCenter, true );
			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickDust( start, Color.Lime );
			}

			int platformHitX, platformHitY;
			IDictionary<int, ISet<int>> breakables = new Dictionary<int, ISet<int>>();
			IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt = new Dictionary<Vector2, IEnumerable<NPC>>();
			IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt = new Dictionary<Vector2, IEnumerable<Projectile>>();

			///

			bool tileHit = BullwhipItem.CastWhipScanRay(
				start,
				direction,
				minWhipDist,
				maxWhipDist,
				out platformHitX,
				out platformHitY,
				breakables,
				hitNpcsAt,
				hitProjsAt
			);

			///

			BullwhipItem.ApplyWhipStrike(
				player,
				start,
				direction,
				breakables,
				hitNpcsAt,
				hitProjsAt,
				platformHitX,
				platformHitY
			);
		}

		////

		public static bool CastWhipScanRay(
					Vector2 start,
					Vector2 direction,
					int minDist,
					int maxDist,
					out int hitPlatformX,
					out int hitPlatformY,
					IDictionary<int, ISet<int>> breakables,
					IDictionary<Vector2, IEnumerable<NPC>> hitNpcAt,
					IDictionary<Vector2, IEnumerable<Projectile>> hitProjAt ) {
			int firstPlatformX = -1;
			int firstPlatformY = -1;

			bool checkPerUnit( Vector2 wldPos ) {
				return BullwhipItem.CheckCollisionPerUnit( start, wldPos, minDist, hitNpcAt, hitProjAt );
			};

			bool checkPerTile( int tileX, int tileY ) {
				bool isPlatform, isBreakable;
				bool isTile = BullwhipItem.FindWhipTileCollisionAt( tileX, tileY, out isPlatform, out isBreakable );

				if( isPlatform && firstPlatformX == -1 ) {
					firstPlatformX = tileX;
					firstPlatformY = tileY;
				}
				if( isBreakable ) {
					breakables.Set2D( tileX, tileY );
				}
				return isTile;
			};

			///

			hitPlatformX = (int)firstPlatformX;
			hitPlatformY = (int)firstPlatformY;

			///

			return CollisionHelpers.CastRay(
				start,
				direction,
				maxDist,
				checkPerUnit,
				checkPerTile
			);
		}


		////////////////

		private static bool CheckCollisionPerUnit(
					Vector2 start,
					Vector2 wldPos,
					int minDist,
					IDictionary<Vector2, IEnumerable<NPC>> hitNpcAt,
					IDictionary<Vector2, IEnumerable<Projectile>> hitProjAt ) {
			int minDistSqr = minDist * minDist;
			float distSqr = Vector2.DistanceSquared( start, wldPos );
			if( distSqr < minDistSqr ) {
				return false;
			}

			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickDust( wldPos, Color.Yellow );
			}

			hitNpcAt[wldPos] = BullwhipItem.FindWhipNpcCollisionAt( wldPos );
			hitProjAt[wldPos] = BullwhipItem.FindWhipProjectileCollisionAt( wldPos );

			return hitNpcAt[ wldPos ].ToArray().Count() > 0
				|| hitProjAt[ wldPos ].ToArray().Count() > 0;
		}
	}
}