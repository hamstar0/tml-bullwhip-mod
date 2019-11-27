using HamstarHelpers.Helpers.Debug;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		private static bool FindWhipNpcCollisionAt(
					Vector2 startPos,
					Vector2 currPos,
					int minHitWorldDistance,
					out IEnumerable<NPC> hitNpcsAt ) {
			int minNpcTileDist = minHitWorldDistance >> 4;
			int minNpcTileDistSqr = minNpcTileDist * minNpcTileDist;
			Vector2 dist = startPos - currPos;
			Vector2 distSqr = dist * dist;

			if( (distSqr.X + distSqr.Y) >= minNpcTileDistSqr ) {
				hitNpcsAt = Main.npc.Where( anyNpc => {
					if( anyNpc == null || !anyNpc.active || anyNpc.immortal ) {
						return false;
					}

					return Vector2.DistanceSquared(anyNpc.Center, currPos) < 1024;	// 32^2
				} );
			} else {
				hitNpcsAt = new NPC[] { };
			}

			return hitNpcsAt.Count() > 0;
		}
		

		private static bool FindWhipProjectileCollisionAt(
					Vector2 startPos,
					Vector2 currPos,
					int minHitWorldDistance,
					out IEnumerable<Projectile> hitProjsAt ) {
			int minNpcTileDist = minHitWorldDistance >> 4;
			int minNpcTileDistSqr = minNpcTileDist * minNpcTileDist;
			Vector2 dist = startPos - currPos;
			Vector2 distSqr = dist * dist;

			if( (distSqr.X + distSqr.Y) >= minNpcTileDistSqr ) {
				hitProjsAt = Main.projectile.Where( anyProj => {
					if( anyProj == null || !anyProj.active || !anyProj.hostile ) {
						return false;
					}

					return Vector2.DistanceSquared(anyProj.Center, currPos) < 1024;	// 32^2
				} );
			} else {
				hitProjsAt = new Projectile[] { };
			}

			return hitProjsAt.Count() > 0;
		}


		////

		private static bool FindWhipTileCollisionAt( int tileX, int tileY, out bool isPlatform, out bool isBreakable ) {
			Tile tile = Framing.GetTileSafely( tileX, tileY );
			isPlatform = Main.tileSolidTop[tile.type];
			isBreakable = Main.tileCut[tile.type] && WorldGen.CanCutTile( tileX, tileY, TileCuttingContext.AttackMelee );

			return tile.active() && !tile.inActive() && Main.tileSolid[ tile.type ] && !isPlatform;
		}
	}
}