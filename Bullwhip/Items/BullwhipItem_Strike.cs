using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;
using ModLibsCore.Libraries.DotNET.Extensions;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static void ApplyWhipStrike(
					Player whipOwner,
					Vector2 start,
					Vector2 direction,
					(int TileX, int TileY)? hitTileAt,
					(int TileX, int TileY)? hitPlatformAt,
					IDictionary<int, ISet<int>> breakables,
					IEnumerable<NPC> hitNpcs,
					IEnumerable<Projectile> hitProjs,
					IEnumerable<Item> hitItems,
					IEnumerable<Player> hitPlayers,
					bool fxOnly ) {
//LogHelpers.Log("WHIP 2 - start:"+start.ToShortString()+", hitNpcsAt:"+hitNpcsAt.Count2D()+", hitProjsAt:"+hitProjsAt.Count2D()+", hitItemsAt:"+hitItemsAt.Count2D());
			int maxWhipDist = BullwhipConfig.Instance.Get<int>( nameof(BullwhipConfig.MaximumWhipHitDist) );
			Vector2 maxPos = start + (direction * maxWhipDist);

			//

			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickDust( maxPos, Color.Red );
			}

			//

			if( !fxOnly ) {
				foreach( (int tileX, ISet<int> tileYs) in breakables ) {
					foreach( int tileY in tileYs ) {
/*Timers.SetTimer("break_"+tileX+"_"+tileY, 50, false, () => {
	Dust.QuickDust( new Point(tileX, tileY), Color.Red );
	return true;
} );*/
						WorldGen.KillTile( tileX, tileY );

						if( Main.netMode != NetmodeID.SinglePlayer ) {
							NetMessage.SendData( MessageID.TileChange, -1, -1, null, 0, (float)tileX, (float)tileY, 0f, 0, 0, 0 );
						}
					}
				}
			}

			//

			bool isNpcHit = BullwhipItem.ApplyWhipStrikeOnNPC( whipOwner, direction, hitNpcs, fxOnly );
			bool isProjHit = BullwhipItem.ApplyWhipStrikeOnProjectile( whipOwner, direction, hitProjs, fxOnly );
			bool isItemHit = BullwhipItem.ApplyWhipStrikeOnItem( whipOwner, direction, hitItems, fxOnly );
			bool isPlrHit = BullwhipItem.ApplyWhipStrikeOnPlayer( whipOwner, direction, hitPlayers, fxOnly );

			//

			if( !isNpcHit && !isPlrHit && !fxOnly ) {
				if( hitPlatformAt.HasValue ) {
					BullwhipItem.GrabPlatform( whipOwner, hitPlatformAt.Value.TileX, hitPlatformAt.Value.TileY );
				}
			}

			//

			if( !isNpcHit && !isPlrHit ) {
				if( !hitTileAt.HasValue && !hitPlatformAt.HasValue ) {
					BullwhipItem.CreateHitAirFx( maxPos );
				} else if( hitPlatformAt.HasValue ) {
					BullwhipItem.CreateHitSolidFx( hitPlatformAt.Value.ToVector2() * 16f );
				} else if( hitTileAt.HasValue ) {
					BullwhipItem.CreateHitSolidFx( hitTileAt.Value.ToVector2() * 16f );
				}
			}
		}


		////////////////

		private static bool ApplyWhipStrikeOnNPC(
					Player player,
					Vector2 direction,
					IEnumerable<NPC> hitNpcs,
					bool fxOnly ) {
					//IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt ) {
			bool isNpcHit = false;
			var checkedNpcs = new HashSet<NPC>();

			//foreach( (Vector2 target, IEnumerable<NPC> npcs) in hitNpcsAt ) {
			foreach( NPC npc in hitNpcs ) {
				if( checkedNpcs.Contains(npc) ) {
					continue;
				}

				checkedNpcs.Add( npc );

				//

				BullwhipItem.StrikeNPC_If( player, direction, /*target,*/ npc, fxOnly );

				isNpcHit = true;
			}

			return isNpcHit;
		}

		private static bool ApplyWhipStrikeOnProjectile(
					Player player,
					Vector2 direction,
					IEnumerable<Projectile> hitProjs,
					bool fxOnly ) {
					//IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt ) {
			var checkedProjs = new HashSet<Projectile>();
			bool isProjHit = false;

			//foreach( (Vector2 target, IEnumerable<Projectile> projs) in hitProjsAt ) {
			foreach( Projectile proj in hitProjs ) {
				if( checkedProjs.Contains(proj) ) {
					continue;
				}

				checkedProjs.Add( proj );

				//
				
				if( BullwhipItem.StrikeProjectile_If(player, direction, /*target,*/ proj, fxOnly) ) {
					proj.friendly = true;
					proj.hostile = false;
				}

				//

				isProjHit = true;
			}

			return isProjHit;
		}

		private static bool ApplyWhipStrikeOnItem(
					Player player,
					Vector2 direction,
					IEnumerable<Item> hitItems,
					bool fxOnly ) {
					//IDictionary<Vector2, IEnumerable<Item>> hitItemsAt ) {
			var checkedItems = new HashSet<Item>();
			bool isItemHit = false;

			//foreach( (Vector2 target, IEnumerable<Item> items) in hitItemsAt ) {
			foreach( Item item in hitItems ) {
				if( checkedItems.Contains(item) ) {
					continue;
				}

				checkedItems.Add( item );

				//

				BullwhipItem.StrikeItem_If( player, direction, /*target,*/ item, fxOnly );

				//

				isItemHit = true;
			}

			return isItemHit;
		}

		private static bool ApplyWhipStrikeOnPlayer(
					Player player,
					Vector2 direction,
					IEnumerable<Player> hitPlayers,
					bool fxOnly ) {
					//IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt ) {
			var checkedPlayers = new HashSet<Player>();
			bool isPlayerHit = false;

			//foreach( (Vector2 target, IEnumerable<Player> plrs) in hitPlayersAt ) {
			foreach( Player plr in hitPlayers ) {
				if( checkedPlayers.Contains(plr) ) {
					continue;
				}

				checkedPlayers.Add( plr );

				//

				BullwhipItem.StrikePlayer_If( player, direction, /*target,*/ plr, fxOnly );

				//

				isPlayerHit = true;
			}

			return isPlayerHit;
		}
	}
}