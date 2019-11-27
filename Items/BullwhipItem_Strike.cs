using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.TModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
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

			BullwhipItem.CreateHitFx( npc.Center, true );
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

			bi.SoundInstance?.Stop();

			var myplayer = player.GetModPlayer<BullwhipPlayer>();
			myplayer.PullHeading = player.Center - new Vector2(tileX<<4, tileY<<4);
		}
	}
}