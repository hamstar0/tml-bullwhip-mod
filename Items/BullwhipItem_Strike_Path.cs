using HamstarHelpers.Helpers.Collisions;
using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.DotNET.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static void CastWhipStrike( Player player, Vector2 direction ) {
			int minWhipDist = BullwhipConfig.Instance.MinimumWhipDist;
			int maxWhipDist = BullwhipConfig.Instance.MaximumWhipDist;
			direction.Normalize();
			
			Vector2 plrCenter = player.RotatedRelativePoint( player.MountedCenter, true );
			Vector2 maxPos = plrCenter + (direction * maxWhipDist);
			
			int endTileX = (int)maxPos.X >> 4;
			int endTileY = (int)maxPos.Y >> 4;

			int platformHitX, platformHitY;
			IDictionary<int, ISet<int>> breakables = new Dictionary<int, ISet<int>>();
			IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt = new Dictionary<Vector2, IEnumerable<NPC>>();
			IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt = new Dictionary<Vector2, IEnumerable<Projectile>>();

			///

			bool tileHit = BullwhipItem.CastWhipScanRay(
				plrCenter,
				direction,
				minWhipDist,
				out platformHitX,
				out platformHitY,
				breakables,
				hitNpcsAt,
				hitProjsAt
			);

			///

			BullwhipItem.ApplyWhipStrike();
			bool isNpcHit = false;
			foreach( (Vector2 target, IEnumerable<NPC> npcs) in hitNpcsAt ) {
				foreach( NPC npc in npcs ) {
					isNpcHit = true;
					BullwhipItem.Strike( player, direction, target, npc );
				}
			}

			bool isProjHit = false;
			foreach( (Vector2 target, IEnumerable<Projectile> projs) in hitProjsAt ) {
				foreach( Projectile proj in projs ) {
					isProjHit = true;
					BullwhipItem.Strike( player, direction, target, proj );
				}
			}

			if( !isNpcHit ) {
				if( platformHitX != -1 ) {
					BullwhipItem.GrabPlatform( player, platformHitX, platformHitY );
				}
			}

			if( !isNpcHit && platformHitX == -1 ) {
				BullwhipItem.CreateHitFx( new Vector2(endTileX<<4, endTileY<<4), false );
			}
		}


		private static bool CastWhipScanRay(
					Vector2 start,
					Vector2 direction,
					int minDist,
					out int hitPlatformX,
					out int hitPlatformY,
					IDictionary<int, ISet<int>> breakables,
					IDictionary<Vector2, IEnumerable<NPC>> hitNpcAt,
					IDictionary<Vector2, IEnumerable<Projectile>> hitProjAt ) {
			int firstPlatformX = -1;
			int firstPlatformY = -1;

			bool checkPerUnit( Vector2 wldPos ) {
				bool hit = BullwhipItem.CheckNpcCollisionPerUnit( start, wldPos, minDist, hitNpcAt );
				hit |= BullwhipItem.CheckProjCollisionPerUnit( start, wldPos, minDist, hitProjAt );
				return hit;
			};

			bool checkPerNonPlatformTile( int tileX, int tileY ) {
				bool isPlatform, isBreakable;
				bool isTile = BullwhipItem.FindWhipTileCollisionAt( tileX, tileY, out isPlatform, out isBreakable );

				if( isPlatform && firstPlatformX != -1 ) {
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
				minDist,
				checkPerUnit,
				checkPerNonPlatformTile
			);
		}


		////////////////

		public static bool CheckNpcCollisionPerUnit(
					Vector2 from,
					Vector2 to,
					int minDist,
					IDictionary<Vector2, IEnumerable<NPC>> hitNpcAt ) {
			IEnumerable<NPC> hitNpcs = null;

			if( BullwhipItem.FindWhipNpcCollisionAt( from, to, minDist, out hitNpcs ) ) {
				hitNpcAt[to] = hitNpcs;
				return true;
			}
			return false;
		}
		

		public static bool CheckProjCollisionPerUnit(
					Vector2 beg,
					Vector2 end,
					int minDist,
					IDictionary<Vector2, IEnumerable<Projectile>> hitProjAt ) {
			IEnumerable<Projectile> hitProjs = null;

			if( BullwhipItem.FindWhipProjectileCollisionAt( beg, end, minDist, out hitProjs ) ) {
				hitProjAt[end] = hitProjs;
				return true;
			}
			return false;
		}


		public static bool CheckCollisionPerTile(
					int tileX,
					int tileY,
					out bool isPlatformHit,
					out bool isBreakable ) {
			return BullwhipItem.FindWhipTileCollisionAt( tileX, tileY, out isPlatformHit, out isBreakable );
		}
	}
}