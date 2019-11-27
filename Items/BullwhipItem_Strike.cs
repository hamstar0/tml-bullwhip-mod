using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.DotNET.Extensions;
using HamstarHelpers.Helpers.Draw;
using HamstarHelpers.Helpers.HUD;
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
					IDictionary<int, ISet<int>> breakables,
					IDictionary<Vector2, IEnumerable<NPC>> hitNpcsAt,
					IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt,
					int platformHitX,
					int platformHitY ) {
			int maxWhipDist = BullwhipConfig.Instance.MaximumWhipDist;
			Vector2 maxPos = start + (direction * maxWhipDist);

			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickDust( maxPos, Color.Red );
			}

			foreach( (int tileX, ISet<int> tileYs) in breakables ) {
				foreach( int tileY in tileYs ) {
					WorldGen.KillTile( tileX, tileY );
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
				if( platformHitX != -1 ) {
					BullwhipItem.GrabPlatform( player, platformHitX, platformHitY );
				}
			}

			if( !isNpcHit && platformHitX == -1 ) {
				BullwhipItem.CreateHitFx( maxPos, false );
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
			
			if( config.DebugModeStrikeInfo ) {
				int durationTicks = 20;
				DrawHelpers.AddPostDrawTilesAction( () => {
					Vector2 pos = hitWorldPosition - Main.screenPosition;
					HUDHelpers.DrawBorderedRect( Main.spriteBatch, null, Color.Red, pos, new Vector2(32), 2 );
					return durationTicks-- > 0;
				} );
			}

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

			BullwhipItem.CreateHitFx( npc.Center, true );
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

			BullwhipItem.CreateHitFx( proj.Center, false );
			BullwhipItem.CreateHitFx( proj.Center, false );
		}


		////////////////

		public static void ApplySlimeshot( NPC npc ) {
			UnifiedRandom rand = TmlHelpers.SafelyGetRand();
			var mynpc = npc.GetGlobalNPC<BullwhipNPC>();

			if( rand.Next(4) == 0 ) {
				mynpc.ApplyEnrage( npc );
			}
		}

		////////////////

		public static bool IsHeadshot( NPC npc, Vector2 targetPoint ) {
			Rectangle rect = npc.getRect();
			rect.X -= rect.Width / 4;
			rect.Width += rect.Width / 2;
			rect.Height /= 3;

			return rect.Contains( (int)targetPoint.X, (int)targetPoint.Y );
		}

		public static void ApplyHeadshot( NPC npc ) {
			UnifiedRandom rand = TmlHelpers.SafelyGetRand();
			int tickDuration = 60 * rand.Next(4, 16);

			npc.AddBuff( BuffID.Confused, tickDuration );
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