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


		private static IEnumerable<Player> FindWhipPlayerCollisionAt( Vector2 wldPos ) {
			//int plrRadiusSqr = BullwhipConfig.Instance.WhipNPCHitRadius;
			//plrRadiusSqr *= plrRadiusSqr;

			return Main.player.Where( anyPlr => {
				if( anyPlr == null || !anyPlr.active || anyPlr.dead ) {
					return false;
				}

				var config = BullwhipConfig.Instance;
				return BullwhipItem.GetRectangle( anyPlr, config.Get<int>(nameof(BullwhipConfig.WhipNPCMinHitRadius)) )
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