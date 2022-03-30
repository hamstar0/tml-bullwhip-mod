using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static void PlaySound( Vector2 pos ) {
			var bi = ModContent.GetInstance<BullwhipItem>();

			int soundSlot = BullwhipMod.Instance.GetSoundSlot(
				SoundType.Custom,
				"Sounds/Custom/BullwhipCrackSound"
			);
			bi.SoundInstance = Main.PlaySound( (int)SoundType.Custom, (int)pos.X, (int)pos.Y, soundSlot, 0.5f );
		}


		////////////////

		public static void CreateHitAirFx( Vector2 pos ) {
			BullwhipItem.CreateHitFx(
				pos: pos,
				dustType: 31,
				count: 6,
				color: Color.White,
				alpha: 192,
				scale: 0.75f,
				fadeIn: 3f
			);
		}
		
		public static void CreateHitSolidFx( Vector2 pos ) {
			BullwhipItem.CreateHitFx(
				pos: pos,
				dustType: 1,
				count: 5,
				color: Color.White,
				alpha: 0,
				scale: 1.25f,
				fadeIn: 1.25f
			);
		}

		public static void CreateHitEntityFx( Vector2 pos ) {
			BullwhipItem.CreateHitFx(
				pos: pos,
				dustType: 31,
				count: 6,
				color: Color.Lerp(Color.Red, Color.White, 0.5f ),
				alpha: 128,
				scale: 0.75f,
				fadeIn: 3f
			);
		}

		////

		private static void CreateHitFx( Vector2 pos, int dustType, int count, Color color, int alpha, float scale, float fadeIn ) {
			int width = 16;
			pos.X -= width / 2;
			pos.Y -= width / 2;

			for( int i=0; i<count; i++ ) {
				Dust dust;
				dust = Main.dust[ Dust.NewDust( pos, width, width, dustType, 0f, 0f, alpha, color, scale ) ];
				dust.noGravity = true;
				dust.fadeIn = fadeIn;
			}
		}
	}
}