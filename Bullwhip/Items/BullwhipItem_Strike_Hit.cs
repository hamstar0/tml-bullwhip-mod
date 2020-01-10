using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.NPCs;
using HamstarHelpers.Helpers.TModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static void StrikeNPC( Player player, Vector2 direction, Vector2 hitWorldPosition, NPC npc ) {
			if( npc.immortal || npc.dontTakeDamage ) {
				return;
			}

			BullwhipConfig config = BullwhipConfig.Instance;
			var mynpc = npc.GetGlobalNPC<BullwhipNPC>();
			int dmg = config.WhipDamage;
			float kb = config.WhipKnockback;

			if( npc.type == NPCID.Bee || npc.type == NPCID.BeeSmall ) {
				NPCHelpers.Kill( npc );
			} else {
				switch( npc.aiStyle ) {
				case 1:     // slimes
					BullwhipItem.ApplySlimeshot( npc );
					break;
				case 3:     // fighters
					if( !mynpc.IsConfuseWhipped && BullwhipItem.IsHeadshot( npc, hitWorldPosition ) ) {
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

				npc.StrikeNPC( dmg, kb, player.direction );

				Mod tricksterMod = ModLoader.GetMod( "TheTrickster" );
				if( tricksterMod != null ) {
					if( npc.type == tricksterMod.NPCType( "TricksterNPC" ) ) {
						BullwhipItem.StrikeTrickster( npc );
					}
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

			// Ignore beehives
			if( proj.type == ProjectileID.BeeHive ) {
				return;
			}

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


		public static void StrikePlayer( Player player, Vector2 direction, Vector2 hitWorldPosition, Player targetPlr ) {
			if( targetPlr.dead || targetPlr.immune ) {
				return;
			}

			BullwhipConfig config = BullwhipConfig.Instance;
			int dmg = config.WhipDamage;
			float kb = config.WhipKnockback;

			targetPlr.velocity += direction * kb;
			targetPlr.Hurt( PlayerDeathReason.ByPlayer(player.whoAmI), dmg, player.direction );

			BullwhipItem.CreateHitEntityFx( targetPlr.Center );
		}
	}
}