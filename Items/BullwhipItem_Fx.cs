using HamstarHelpers.Helpers.Debug;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static void PlaySound( Vector2 pos ) {
			int soundSlot = BullwhipMod.Instance.GetSoundSlot( SoundType.Custom, "Sounds/Custom/BullwhipCrackSound" );
			Main.PlaySound( (int)SoundType.Custom, (int)pos.X, (int)pos.Y, soundSlot, 0.5f );
		}

		public static void CreateHitFx( Vector2 pos, bool isNpc ) {
			Color color = isNpc ? Color.Lerp(Color.Red, Color.White, 0.5f) : Color.White;
			int alpha = isNpc ? 128 : 192;
			float scale = 0.75f;
			int width = 16;
			pos.X -= width / 2;
			pos.Y -= width / 2;

			Dust dust;
			dust = Main.dust[Dust.NewDust( pos, width, width, 31, 0f, 0f, alpha, color, scale )];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[Dust.NewDust( pos, width, width, 31, 0f, 0f, alpha, color, scale )];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[Dust.NewDust( pos, width, width, 31, 0f, 0f, alpha, color, scale )];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[Dust.NewDust( pos, width, width, 31, 0f, 0f, alpha, color, scale )];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[Dust.NewDust( pos, width, width, 31, 0f, 0f, alpha, color, scale )];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[Dust.NewDust( pos, width, width, 31, 0f, 0f, alpha, color, scale )];
			dust.noGravity = true;
			dust.fadeIn = 3f;
		}
	}
}