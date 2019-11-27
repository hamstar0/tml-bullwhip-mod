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
				isBreakable = Main.tileCut[tile.type] && WorldGen.CanCutTile(tileX, tileY, TileCuttingContext.AttackMelee);
			} else {
				isPlatform = false;
				isBreakable = false;
			}

			return isActive && Main.tileSolid[tile.type] && !isPlatform;
		}
	}
}