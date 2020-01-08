using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.DotNET.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static void ApplyWhipStrike(
					Player player,
					Vector2 start,
					Vector2 direction,
					(int TileX, int TileY)? hitTileAt,
					(int TileX, int TileY)? hitPlatformAt,
					IDictionary<int, ISet<int>> breakables,
					IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt,
					IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt,
					IDictionary<Vector2, IEnumerable<Item>> hitItemsAt,
					IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt ) {
//LogHelpers.Log("WHIP 2 - start:"+start.ToShortString()+", hitNpcsAt:"+hitNpcsAt.Count2D()+", hitProjsAt:"+hitProjsAt.Count2D()+", hitItemsAt:"+hitItemsAt.Count2D());
			int maxWhipDist = BullwhipConfig.Instance.MaximumWhipHitDist;
			Vector2 maxPos = start + (direction * maxWhipDist);

			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickDust( maxPos, Color.Red );
			}

			foreach( (int tileX, ISet<int> tileYs) in breakables ) {
				foreach( int tileY in tileYs ) {
/*Timers.SetTimer("break_"+tileX+"_"+tileY, 50, false, () => {
	Dust.QuickDust( new Point(tileX, tileY), Color.Red );
	return true;
} );*/
					WorldGen.KillTile( tileX, tileY );
					if( Main.netMode != 0 ) {
						NetMessage.SendData( MessageID.TileChange, -1, -1, null, 0, (float)tileX, (float)tileY, 0f, 0, 0, 0 );
					}
				}
			}

			bool isNpcHit = BullwhipItem.ApplyWhipStrikeOnNPC( player, direction, hitNpcsAt );
			BullwhipItem.ApplyWhipStrikeOnProjectile( player, direction, hitProjsAt );
			BullwhipItem.ApplyWhipStrikeOnItem( player, direction, hitItemsAt );
			BullwhipItem.ApplyWhipStrikeOnPlayer( player, direction, hitPlayersAt );

			if( !isNpcHit ) {
				if( hitPlatformAt.HasValue ) {
					BullwhipItem.GrabPlatform( player, hitPlatformAt.Value.TileX, hitPlatformAt.Value.TileY );
				}
			}

			if( !isNpcHit ) {
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
					IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt ) {
			bool isNpcHit = false;
			var checkedNpcs = new HashSet<NPC>();

			foreach( (Vector2 target, IEnumerable<NPC> npcs) in hitNpcsAt ) {
				foreach( NPC npc in npcs ) {
					isNpcHit = true;
					if( checkedNpcs.Contains(npc) ) { continue; }
					checkedNpcs.Add( npc );

					BullwhipItem.StrikeNPC( player, direction, target, npc );
				}
			}

			return isNpcHit;
		}

		private static bool ApplyWhipStrikeOnProjectile(
					Player player,
					Vector2 direction,
					IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt ) {
			var checkedProjs = new HashSet<Projectile>();
			bool isProjHit = false;

			foreach( (Vector2 target, IEnumerable<Projectile> projs) in hitProjsAt ) {
				foreach( Projectile proj in projs ) {
					isProjHit = true;
					if( checkedProjs.Contains(proj) ) { continue; }
					checkedProjs.Add( proj );

					BullwhipItem.StrikeProjectile( player, direction, target, proj );
					proj.friendly = true;
					proj.hostile = false;
				}
			}

			return isProjHit;
		}

		private static bool ApplyWhipStrikeOnItem(
					Player player,
					Vector2 direction,
					IDictionary<Vector2, IEnumerable<Item>> hitItemsAt ) {
			var checkedItems = new HashSet<Item>();
			bool isItemHit = false;

			foreach( (Vector2 target, IEnumerable<Item> items) in hitItemsAt ) {
				foreach( Item item in items ) {
					isItemHit = true;
					if( checkedItems.Contains(item) ) { continue; }
					checkedItems.Add( item );

					BullwhipItem.StrikeItem( player, direction, target, item );
				}
			}

			return isItemHit;
		}

		private static bool ApplyWhipStrikeOnPlayer(
					Player player,
					Vector2 direction,
					IDictionary<Vector2, IEnumerable<Player>> hitPlayersAt ) {
			var checkedPlayers = new HashSet<Player>();
			bool isPlayerHit = false;

			foreach( (Vector2 target, IEnumerable<Player> plrs) in hitPlayersAt ) {
				foreach( Player plr in plrs ) {
					isPlayerHit = true;
					if( checkedPlayers.Contains( plr ) ) { continue; }
					checkedPlayers.Add( plr );

					BullwhipItem.StrikePlayer( player, direction, target, plr );
				}
			}

			return isPlayerHit;
		}
	}
}