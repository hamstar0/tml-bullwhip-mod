using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.DotNET.Extensions;
using HamstarHelpers.Helpers.TModLoader;
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
					IDictionary<Vector2, IEnumerable<Item>> hitItemsAt ) {
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

			var checkedNpcs = new HashSet<NPC>();
			bool isNpcHit = false;
			foreach( (Vector2 target, IEnumerable<NPC> npcs) in hitNpcsAt ) {
				foreach( NPC npc in npcs ) {
					isNpcHit = true;
					if( checkedNpcs.Contains(npc) ) { continue; }
					checkedNpcs.Add( npc );

					BullwhipItem.StrikeNPC( player, direction, target, npc );
				}
			}

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

		public static void StrikeNPC( Player player, Vector2 direction, Vector2 hitWorldPosition, NPC npc ) {
			if( npc.immortal ) {
				return;
			}

			BullwhipConfig config = BullwhipConfig.Instance;
			var mynpc = npc.GetGlobalNPC<BullwhipNPC>();
			int dmg = config.WhipDamage;
			float kb = config.WhipKnockback;

			switch( npc.aiStyle ) {
			case 1:     // slimes
				BullwhipItem.ApplySlimeshot( npc );
				break;
			case 3:     // fighters
				if( !mynpc.IsConfuseWhipped && BullwhipItem.IsHeadshot(npc, hitWorldPosition) ) {
					BullwhipItem.ApplyConfuse( npc );
				}
				break;
			case 14:    // bats
				if( BullwhipConfig.Instance.IncapacitatesBats && npc.aiStyle == 14 ) {//&& NPCID.Search.GetName(npc.type).Contains("Bat") ) {
					npc.aiStyle = 16;
					kb = 1f;
				}
				break;
			}

			if( !mynpc.IsConfuseWhipped ) {
				// Doesn't work on slimes
				if( npc.aiStyle != 1 ) {
					if( TmlHelpers.SafelyGetRand().NextFloat() <= config.WhipConfuseChance ) {
						BullwhipItem.ApplyConfuse( npc );
					}
				}
			}

			npc.velocity += direction * kb;
			npc.StrikeNPC( dmg, kb, player.direction );

			Mod tricksterMod = ModLoader.GetMod( "TheTrickster" );
			if( tricksterMod != null ) {
				if( npc.type == tricksterMod.NPCType( "TricksterNPC" ) ) {
					BullwhipItem.StrikeTrickster( npc );
				}
			}

			BullwhipItem.CreateHitEntityFx( npc.Center );
//LogHelpers.Log("WHIP 3 "+npc.TypeName+" ("+npc.whoAmI+"), direction:"+direction.ToShortString()+", hitWorldPosition:"+hitWorldPosition.ToShortString());
		}


		private static void StrikeTrickster( NPC npc ) {
			if( Main.netMode != 1 ) {
				return;
			}

			var rand = TmlHelpers.SafelyGetRand();
			if( !rand.NextBool() ) {
				return;
			}

			//NPCHelpers.Remove( npc );
			var mynpc = (TheTrickster.NPCs.TricksterNPC)npc.modNPC;
			mynpc.Flee();

			if( Main.netMode == 2 ) {
				NetMessage.SendData( MessageID.SyncNPC, -1, -1, null, npc.whoAmI );
			}
		}


		public static void StrikeProjectile( Player player, Vector2 direction, Vector2 hitWorldPosition, Projectile proj ) {
			BullwhipConfig config = BullwhipConfig.Instance;
			//int dmg = config.WhipDamage;
			//float kb = config.WhipKnockback;

			float speed = proj.velocity.Length();
			direction.Normalize();
			proj.velocity.Normalize();

			proj.velocity = Vector2.Normalize( direction + proj.velocity );
			proj.velocity *= speed;

			BullwhipItem.CreateHitEntityFx( proj.Center );
			BullwhipItem.CreateHitEntityFx( proj.Center );
		}


		public static void StrikeItem( Player player, Vector2 direction, Vector2 hitWorldPosition, Item item ) {
			item.position = player.position;
		}
	}
}