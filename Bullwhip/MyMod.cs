using Terraria;
using Terraria.ModLoader;
using Bullwhip.Projectiles;
using Microsoft.Xna.Framework;

namespace Bullwhip {
	public partial class BullwhipMod : Mod {
		public static string GithubUserName => "hamstar0";
		public static string GithubProjectName => "tml-bullwhip-mod";


		////////////////

		public static BullwhipMod Instance => ModContent.GetInstance<BullwhipMod>();



		////////////////

		internal ModHotKey QuickWhip = null;



		////////////////

		public override void Load() {
			this.QuickWhip = this.RegisterHotKey( "Quick Whip", "Q" );
		}

		////

		public override void PostSetupContent() {
			if( ModLoader.GetMod("SoulBarriers") != null ) {
				BullwhipMod.Load_WeakRef_SoulBarriers();
			}
		}
	}
}