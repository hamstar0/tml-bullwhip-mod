using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;
using ModLibsCore.Libraries.TModLoader;
using ModLibsGeneral.Libraries.NPCs;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		private static bool ApplyWhipStrikeOnNPCs(
					Player player,
					Vector2 direction,
					IEnumerable<NPC> hitNpcs,
					bool fxOnly ) {
			//IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt ) {
			bool isNpcHit = false;
			var checkedNpcs = new HashSet<NPC>();

			//foreach( (Vector2 target, IEnumerable<NPC> npcs) in hitNpcsAt ) {
			foreach( NPC npc in hitNpcs ) {
				if( npc?.active != true ) {
					continue;
				}

				//

				if( checkedNpcs.Contains( npc ) ) {
					continue;
				}

				checkedNpcs.Add( npc );

				//

				BullwhipItem.StrikeNPC_If( player, direction, /*target,*/ npc, fxOnly );

				isNpcHit = true;
			}

			return isNpcHit;
		}


		////////////////

		public static void StrikeNPC_If(
					Player player,
					Vector2 direction,
					/*Vector2 hitWorldPosition,*/
					NPC npc,
					bool fxOnly ) {
			if( npc.immortal || npc.dontTakeDamage ) {
				return;
			}
			
			bool _ = false;
			if( !BullwhipAPI.OnPreBullwhipEntityHit(player, npc, fxOnly, ref _) ) {
				return;
			}

			//

			if( npc.type == NPCID.Bee || npc.type == NPCID.BeeSmall ) {
				if( !fxOnly ) {
					NPCLibraries.Kill( npc, false );
				}
			} else {
				if( !fxOnly ) {
					var config = BullwhipConfig.Instance;
					int dmg = config.Get<int>( nameof( config.WhipDamage ) );
					float kb = config.Get<float>( nameof( config.WhipKnockback ) );

					if( npc.aiStyle == 1 ) {	// slimes
						kb *= 0.65f;
					}

					BullwhipItem.StrikeNPC_AI( player, /*hitWorldPosition,*/ npc, ref kb );

					npc.StrikeNPC( dmg, kb, player.direction );
				}

				//

				Mod tricksterMod = ModLoader.GetMod( "TheTrickster" );

				if( tricksterMod != null ) {
					if( npc.type == tricksterMod.NPCType("TricksterNPC") ) {
						BullwhipItem.StrikeTrickster_If( player, npc, fxOnly );
					}
				}
			}

			//

			BullwhipItem.CreateHitEntityFx( npc.Center );
//LogHelpers.Log("WHIP 3 "+npc.TypeName+" ("+npc.whoAmI+"), direction:"+direction.ToShortString()+", hitWorldPosition:"+hitWorldPosition.ToShortString());
		}

		////
		
		private static void StrikeNPC_AI( Player player, /*Vector2 hitWorldPosition,*/ NPC npc, ref float knockback ) {
			var config = BullwhipConfig.Instance;
			var mynpc = npc.GetGlobalNPC<BullwhipNPC>();

			switch( npc.aiStyle ) {
			case 1:     // slimes
				BullwhipItem.ApplySlimeshot( npc );
				break;
			case 3:     // fighters
				if( !mynpc.IsConfuseWhipped /*&& BullwhipItem.IsHeadshot(npc, hitWorldPosition)*/ ) {
					BullwhipItem.ApplyConfuse( npc );
				}
				break;
			case 14:    // bats
				if( config.Get<bool>( nameof(BullwhipConfig.IncapacitatesBats) ) ) {
					//if( npc.aiStyle == 14 ) {	//&& NPCID.Search.GetName(npc.type).Contains("Bat") ) {
					npc.aiStyle = 16;
					knockback = 1f;

					mynpc.IsCrippleWhipped = true;
				}
				break;
			}

			if( !mynpc.IsConfuseWhipped ) {
				// Doesn't work on slimes
				if( npc.aiStyle != 1 ) {
					float confuseChance = config.Get<float>( nameof( BullwhipConfig.WhipConfuseChance ) );

					if( TmlLibraries.SafelyGetRand().NextFloat() <= confuseChance ) {
						BullwhipItem.ApplyConfuse( npc );
					}
				}
			}
		}


		////

		private static void StrikeTrickster_If( Player player, NPC npc, bool fxOnly ) {
			if( Main.netMode == NetmodeID.MultiplayerClient ) {
				return;
			}

			if( fxOnly ) {
				return;
			}

			//
			
			var rand = TmlLibraries.SafelyGetRand();

			if( rand.NextBool() ) {	// 50% chance
				//NPCLibraries.Remove( npc );
				var mynpc = (TheTrickster.NPCs.TricksterNPC)npc.modNPC;
				mynpc.FleeAction( false );

				if( Main.netMode == NetmodeID.Server ) {
					NetMessage.SendData( MessageID.SyncNPC, -1, -1, null, npc.whoAmI );
				}
			}
		}
	}
}