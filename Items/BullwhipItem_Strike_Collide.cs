using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		private static IEnumerable<NPC> FindWhipNpcCollisionAt( Vector2 wldPos ) {
			return Main.npc.Where( anyNpc => {
				if( anyNpc == null || !anyNpc.active || anyNpc.immortal ) {
					return false;
				}
				return Vector2.DistanceSquared(anyNpc.Center, wldPos) < 1024;	// 32^2
			} );
		}
		

		private static IEnumerable<Projectile> FindWhipProjectileCollisionAt( Vector2 wldPos ) {
			return Main.projectile.Where( anyProj => {
				if( anyProj == null || !anyProj.active || !anyProj.hostile ) {
					return false;
				}
				return Vector2.DistanceSquared(anyProj.Center, wldPos) < 1024;	// 32^2
			} );
		}


		////

		private static bool FindWhipTileCollisionAt( int tileX, int tileY, out bool isPlatform, out bool isBreakable ) {
			Tile tile = Framing.GetTileSafely( tileX, tileY );
			bool isActive = tile.active() && !tile.inActive();

			if( isActive ) {
				isPlatform = Main.tileSolidTop[tile.type];
				isBreakable = TileAttributeHelpers.IsBreakable( tileX, tileY );
			} else {
				isPlatform = false;
				isBreakable = false;
			}

			return isActive && Main.tileSolid[tile.type] && !isPlatform;
		}

		private static IEnumerable<(int TileX, int TileY)> FindNearbyBreakableTiles( int tileX, int tileY ) {
			if( TileAttributeHelpers.IsBreakable(tileX, tileY) ) {
				yield return (tileX, tileY);
			}
			if( TileAttributeHelpers.IsBreakable(tileX - 1, tileY) ) {
				yield return (tileX - 1, tileY);
			}
			if( TileAttributeHelpers.IsBreakable(tileX + 1, tileY) ) {
				yield return (tileX + 1, tileY);
			}
			if( TileAttributeHelpers.IsBreakable(tileX, tileY - 1) ) {
				yield return (tileX, tileY - 1);
			}
			if( TileAttributeHelpers.IsBreakable(tileX, tileY + 1) ) {
				yield return (tileX, tileY + 1);
			}
		}
	}
}