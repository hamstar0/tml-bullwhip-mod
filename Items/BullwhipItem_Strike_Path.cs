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
		public static void AttemptWhipStrike( Player player, Vector2 direction ) {
			int minWhipDist = BullwhipConfig.Instance.MinimumWhipDist;
			int maxWhipDist = BullwhipConfig.Instance.MaximumWhipDist;
			direction.Normalize();
			
			Vector2 plrCenter = player.RotatedRelativePoint( player.MountedCenter, true );
			Vector2 maxPos = plrCenter + (direction * maxWhipDist);

			int endTileX = (int)maxPos.X >> 4;
			int endTileY = (int)maxPos.Y >> 4;

			bool isTileHit = false;
			IDictionary<Vector2, IEnumerable<NPC>> hitNpcAt = new Dictionary<Vector2, IEnumerable<NPC>>();

			///

			Func<Vector2, bool> checkPerUnit = (wldPos) => {
				IEnumerable<NPC> hitNpcs = null;
				if( BullwhipItem.FindWhipUnitCollisionAt( plrCenter, wldPos, minWhipDist, out hitNpcs ) ) {
					hitNpcAt[wldPos] = hitNpcs;
					return true;
				}
				return false;
			};

			Func<int, int, bool> checkPerTile = (currTileX, currTileY) => {
				bool isPlatform;
				if( BullwhipItem.FindWhipTileCollisionAt(currTileX, currTileY, out isPlatform) && !isPlatform ) {
					isTileHit = true;
					endTileX = currTileX;
					endTileY = currTileY;
					return true;
				}
				return false;
			};

			///

			CollisionHelpers.CastRay( plrCenter, direction, maxWhipDist, checkPerUnit, checkPerTile );

			foreach( (Vector2 target, IEnumerable<NPC> npcs) in hitNpcAt ) {
				foreach( NPC npc in npcs ) {
					BullwhipItem.Strike( player, direction, target, npc );
				}
			}

			if( isTileHit ) {
				BullwhipItem.CreateHitFx( new Vector2(endTileX<<4, endTileY<<4), false );
			}
		}


		////

		private static bool FindWhipUnitCollisionAt(
					Vector2 startPos,
					Vector2 currPos,
					int minNpcHitWorldDistance,
					out IEnumerable<NPC> hitNpcsAt ) {
			int minNpcTileDist = minNpcHitWorldDistance >> 4;
			int minNpcTileDistSqr = minNpcTileDist * minNpcTileDist;
			Vector2 dist = startPos - currPos;
			Vector2 distSqr = dist * dist;

			if( (distSqr.X + distSqr.Y) >= minNpcTileDistSqr ) {
				hitNpcsAt = Main.npc.Where( anyNpc => {
					if( anyNpc == null || !anyNpc.active || anyNpc.immortal ) {
						return false;
					}

					//if( Math.Abs( nTileX - currTileX ) <= 2 && Math.Abs( nTileY - currTileY ) <= 2 ) {
					return Vector2.DistanceSquared(anyNpc.Center, currPos) < 1024;	// 32^2
				} );
			} else {
				hitNpcsAt = new NPC[] { };
			}

			return hitNpcsAt.Count() > 0;
		}

		private static bool FindWhipTileCollisionAt( int currTileX, int currTileY, out bool isPlatform ) {
			Tile tile = Framing.GetTileSafely( currTileX, currTileY );

			isPlatform = !Main.tileSolidTop[ tile.type ];
			return tile.active() && Main.tileSolid[ tile.type ];
		}
	}
}