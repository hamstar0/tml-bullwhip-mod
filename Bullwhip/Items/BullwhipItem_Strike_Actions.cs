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
			Rectangle rect = npc.getRect();
			rect.X -= rect.Width;
			rect.Y -= rect.Height / 3;
			rect.Width = 3 * rect.Width;
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

		////

		public static void ApplyConfuse( NPC npc ) {
			UnifiedRandom rand = TmlHelpers.SafelyGetRand();
			int tickDuration = 60 * rand.Next(4, 9);
			var mynpc = npc.GetGlobalNPC<BullwhipNPC>();

			npc.AddBuff( BuffID.Confused, tickDuration );
			mynpc.IsConfuseWhipped = true;
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