using Terraria.ModLoader;


namespace Bullwhip {
	public class BullwhipMod : Mod {
		public static string GithubUserName => "hamstar0";
		public static string GithubProjectName => "tml-bullwhip-mod";


		////////////////

		public static BullwhipMod Instance { get; private set; }



		////////////////

		internal ModHotKey QuickWhip = null;



		////////////////

		public BullwhipMod() {
			BullwhipMod.Instance = this;
		}

		////

		public override void Load() {
			this.QuickWhip = this.RegisterHotKey( "Quick Whip", "Q" );
		}

		public override void Unload() {
			BullwhipMod.Instance = null;
		}
	}
}