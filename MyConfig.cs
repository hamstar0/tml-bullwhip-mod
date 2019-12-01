using HamstarHelpers.Classes.UI.ModConfig;
using HamstarHelpers.Services.Configs;
using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;


namespace Bullwhip {
	class MyFloatInputElement : FloatInputElement { }




	public partial class BullwhipConfig : StackableModConfig {
		public static BullwhipConfig Instance => ModConfigStack.GetMergedConfigs<BullwhipConfig>();



		////////////////

		public override ConfigScope Mode => ConfigScope.ServerSide;


		////////////////

		public bool DebugModeStrikeInfo { get; set; } = false;


		[DefaultValue( true )]
		public bool RecipeEnabled { get; set; } = true;


		[DefaultValue( true )]
		public bool PlayerSpawnsWithWhip { get; set; } = true;


		[DefaultValue( 128 )]
		public int MinimumWhipDist { get; set; } = 144;

		[DefaultValue( 192 )]
		public int MaximumWhipDist { get; set; } = 192;

		[DefaultValue( 1 )]
		public int WhipDamage { get; set; } = 1;

		[DefaultValue( 20f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float WhipKnockback { get; set; } = 20f;

		[Range( 1, 128 )]
		[DefaultValue( 32 )]
		public int WhipNPCHitRadius { get; set; } = 32;
		[Range( 1, 128 )]
		[DefaultValue( 36 )]
		public int WhipProjectileHitRadius { get; set; } = 36;


		[DefaultValue( true )]
		public bool IncapacitatesBats { get; set; } = true;


		[DefaultValue( 0.1f )]
		[Range( 0f, 0.5f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float WhipLedgePullStrength { get; set; } = 0.1f;
		


		////////////////

		/*public override ModConfig Clone() {
			var clone = (AdventureModeConfig)base.Clone();
			return clone;
		}*/
	}
}
