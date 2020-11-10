using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.Tiles.Attributes;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		private static IEnumerable<NPC> FindWhipNpcCollisionAt( Vector2 wldPos ) {
			//float npcRadiusSqr = BullwhipConfig.Instance.WhipNPCHitRadius;

			return Main.npc.Where( anyNpc => {
				if( anyNpc == null || !anyNpc.active || anyNpc.immortal ) {
					return false;
				}

				var config = BullwhipConfig.Instance;
				return BullwhipItem.GetRectangle( anyNpc, config.Get<int>(nameof(BullwhipConfig.WhipNPCMinHitRadius)) )
					.Contains( (int)wldPos.X, (int)wldPos.Y );
				/*float npcRadiusSqr = (anyNpc.width + anyNpc.height) / 2;
				npcRadiusSqr *= npcRadiusSqr;

				float hitDistSqr = Vector2.DistanceSquared( anyNpc.Center, wldPos );
				return hitDistSqr < npcRadiusSqr;*/
			} );
		}
		

		private static IEnumerable<Projectile> FindWhipProjectileCollisionAt( Vector2 wldPos ) {
			//int projRadiusSqr = BullwhipConfig.Instance.WhipProjectileHitRadius;
			//projRadiusSqr *= projRadiusSqr;

			return Main.projectile.Where( anyProj => {
				if( anyProj == null || !anyProj.active || !anyProj.hostile ) {
					return false;
				}

				var config = BullwhipConfig.Instance;
				return BullwhipItem.GetRectangle( anyProj, config.Get<int>(nameof(BullwhipConfig.WhipProjectileMinHitRadius)) )
					.Contains( (int)wldPos.X, (int)wldPos.Y );
				//return Vector2.DistanceSquared(anyProj.Center, wldPos) < projRadiusSqr;
			} );
		}
		

		private static IEnumerable<Item> FindWhipItemCollisionAt( Vector2 wldPos ) {
			//int itemRadiusSqr = BullwhipConfig.Instance.WhipItemHitRadius;
			//itemRadiusSqr *= itemRadiusSqr;

			return Main.item.Where( anyItem => {
				if( anyItem == null || !anyItem.active ) {
					return false;
				}

				var config = BullwhipConfig.Instance;
				return BullwhipItem.GetRectangle( anyItem, config.Get<int>(nameof(BullwhipConfig.WhipItemHitMinRadius)) )
					.Contains( (int)wldPos.X, (int)wldPos.Y );
				//return Vector2.DistanceSquared(anyItem.Center, wldPos) < itemRadiusSqr;
			} );
		}


		private static IEnumerable<Player> FindWhipPlayerCollisionAt( Player whipOwner, Vector2 wldPos ) {
			//int plrRadiusSqr = BullwhipConfig.Instance.WhipNPCHitRadius;
			//plrRadiusSqr *= plrRadiusSqr;

			return Main.player.Where( plr => {
				if( plr == null || !plr.active || plr.dead || plr.whoAmI == whipOwner.whoAmI ) {
					return false;
				}

				var config = BullwhipConfig.Instance;
				return BullwhipItem.GetRectangle( plr, config.Get<int>(nameof(BullwhipConfig.WhipNPCMinHitRadius)) )
					.Contains( (int)wldPos.X, (int)wldPos.Y );
				//return Vector2.DistanceSquared(anyPlr.Center, wldPos) < plrRadiusSqr;
			} );
		}


		////

		private static bool FindWhipTileCollisionAt( int tileX, int tileY, out bool isPlatform, out bool isBreakable ) {
			Tile tile = Framing.GetTileSafely( tileX, tileY );
			bool isActive = tile.active() && !tile.inActive();

			if( isActive ) {
				isPlatform = Main.tileSolidTop[tile.type] && Main.tileSolid[tile.type];
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

			if( TileAttributeHelpers.IsBreakable(tileX - 1, tileY - 1) ) {
				yield return (tileX - 1, tileY - 1);
			}
			if( TileAttributeHelpers.IsBreakable(tileX, tileY - 1) ) {
				yield return (tileX, tileY - 1);
			}
			if( TileAttributeHelpers.IsBreakable(tileX + 1, tileY - 1) ) {
				yield return (tileX + 1, tileY - 1);
			}

			if( TileAttributeHelpers.IsBreakable(tileX - 1, tileY) ) {
				yield return (tileX - 1, tileY);
			}
			if( TileAttributeHelpers.IsBreakable(tileX + 1, tileY) ) {
				yield return (tileX + 1, tileY);
			}

			if( TileAttributeHelpers.IsBreakable(tileX - 1, tileY + 1) ) {
				yield return (tileX - 1, tileY + 1);
			}
			if( TileAttributeHelpers.IsBreakable(tileX, tileY + 1) ) {
				yield return (tileX, tileY + 1);
			}
			if( TileAttributeHelpers.IsBreakable(tileX + 1, tileY + 1) ) {
				yield return (tileX + 1, tileY + 1);
			}
		}
	}
}