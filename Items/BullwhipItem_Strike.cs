using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.DotNET.Extensions;
using HamstarHelpers.Helpers.NPCs;
using HamstarHelpers.Helpers.TModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;


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
					IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt ) {
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
			
			bool isNpcHit = false;
			foreach( (Vector2 target, IEnumerable<NPC> npcs) in hitNpcsAt ) {
				foreach( NPC npc in npcs ) {
					isNpcHit = true;
					BullwhipItem.Strike( player, direction, target, npc );
				}
			}

			bool isProjHit = false;
			foreach( (Vector2 target, IEnumerable<Projectile> projs) in hitProjsAt ) {
				foreach( Projectile proj in projs ) {
					isProjHit = true;
					BullwhipItem.Strike( player, direction, target, proj );
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

		public static void Strike( Player player, Vector2 direction, Vector2 hitWorldPosition, NPC npc ) {
			if( npc.immortal ) {
				return;
			}

			BullwhipConfig config = BullwhipConfig.Instance;
			int dmg = config.WhipDamage;
			float kb = config.WhipKnockback;

			switch( npc.aiStyle ) {
			case 1:     // slimes
				BullwhipItem.ApplySlimeshot( npc );
				break;
			case 3:     // fighters
				if( BullwhipItem.IsHeadshot( npc, hitWorldPosition ) ) {
					BullwhipItem.ApplyHeadshot( npc );
				}
				break;
			case 14:    // bats
				if( BullwhipConfig.Instance.IncapacitatesBats && npc.aiStyle == 14 ) {//&& NPCID.Search.GetName(npc.type).Contains("Bat") ) {
					npc.aiStyle = 16;
					kb = 1f;
				}
				break;
			}

			npc.velocity += direction * kb;
			npc.StrikeNPC( dmg, kb, player.direction );

			Mod tricksterMod = ModLoader.GetMod( "TheTrickster" );
			if( tricksterMod != null ) {
				if( npc.type == tricksterMod.NPCType("TricksterNPC") ) {
					var rand = TmlHelpers.SafelyGetRand();
					if( rand.NextBool() ) {
						NPCHelpers.Remove( npc );
					}
				}
			}

			BullwhipItem.CreateHitEntityFx( npc.Center );
		}


		public static void Strike( Player player, Vector2 direction, Vector2 hitWorldPosition, Projectile proj ) {
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


		////////////////

		public static void ApplySlimeshot( NPC npc ) {
			if( Main.netMode == 1 ) {
				return;
			}

			UnifiedRandom rand = TmlHelpers.SafelyGetRand();
			var mynpc = npc.GetGlobalNPC<BullwhipNPC>();

			if( rand.Next(4) == 0 ) {
				mynpc.ApplyEnrage( npc );
			}
		}

		////////////////

		public static bool IsHeadshot( NPC npc, Vector2 targetPoint ) {
			var mynpc = npc.GetGlobalNPC<BullwhipNPC>();
			if( mynpc.IsHeadWhipped ) {
				return false;
			}

			Rectangle rect = npc.getRect();
			rect.X -= (2 * rect.Width) / 3;
			rect.Y -= rect.Height / 3;
			rect.Width = (2 * rect.Width) + (rect.Width / 3);
			rect.Height = rect.Height / 2;

			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickBox(
					new Vector2( targetPoint.X-2, targetPoint.Y-2 ),
					new Vector2( targetPoint.X+2, targetPoint.Y+2 ),
					2,
					Color.Purple,
					d => { }
				);
				Dust.QuickBox(
					new Vector2(rect.X, rect.Y),
					new Vector2(rect.X+rect.Width, rect.Y+rect.Height),
					2,
					Color.Red,
					d => { }
				);
			}

			return rect.Contains( (int)targetPoint.X, (int)targetPoint.Y );
		}

		public static void ApplyHeadshot( NPC npc ) {
			UnifiedRandom rand = TmlHelpers.SafelyGetRand();
			int tickDuration = 60 * rand.Next(4, 9);
			var mynpc = npc.GetGlobalNPC<BullwhipNPC>();

			npc.AddBuff( BuffID.Confused, tickDuration );
			mynpc.IsHeadWhipped = true;
		}


		////////////////

		public static void GrabPlatform( Player player, int tileX, int tileY ) {
			var bi = ModContent.GetInstance<BullwhipItem>();
			var target = new Vector2( tileX << 4, tileY << 4 );

			bi.SoundInstance?.Stop();

			var myplayer = player.GetModPlayer<BullwhipPlayer>();
			myplayer.SetPullHeading( player.Center - target );

			Main.PlaySound( SoundID.Dig, target );
		}
	}
}