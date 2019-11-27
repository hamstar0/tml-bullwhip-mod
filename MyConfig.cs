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

		[DefaultValue( true )]
		public bool RecipeEnabled { get; set; } = true;


		[DefaultValue( true )]
		public bool PlayerSpawnsWithWhip { get; set; } = true;


		[DefaultValue( 144 )]
		public int MinimumWhipDist { get; set; } = 144;

		[DefaultValue( 192 )]
		public int MaximumWhipDist { get; set; } = 192;

		[DefaultValue( 15 )]
		public int WhipDamage { get; set; } = 15;

		[DefaultValue( 20f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float WhipKnockback { get; set; } = 20f;


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
