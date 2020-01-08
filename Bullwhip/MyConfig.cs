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


		[DefaultValue( 32 )]
		public int MinimumWhipHitDist { get; set; } = 32;//128;

		[DefaultValue( 192 )]
		public int MaximumWhipHitDist { get; set; } = 192;

		[DefaultValue( 1 )]
		public int WhipDamage { get; set; } = 1;

		[DefaultValue( 17f )]//20f
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float WhipKnockback { get; set; } = 17f;


		[Range( 1, 128 )]
		[DefaultValue( 28 )]//32
		public int WhipNPCHitRadius { get; set; } = 28;

		[Range( 1, 128 )]
		[DefaultValue( 36 )]
		public int WhipProjectileHitRadius { get; set; } = 36;

		[Range( 1, 128 )]
		[DefaultValue( 36 )]
		public int WhipItemHitRadius { get; set; } = 36;


		[DefaultValue( true )]
		public bool IncapacitatesBats { get; set; } = true;


		[DefaultValue( 11.4f )]
		[Range( 0f, 100f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float WhipLedgePullStrength { get; set; } = 11.4f;


		[Range( 0, 16 )]
		[DefaultValue( 3 )]
		public int MaxWhipEntityHits { get; set; } = 3;


		[DefaultValue( 1f / 2.5f )]
		[Range( 0f, 1f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float WhipConfuseChance { get; set; } = 1f / 2.5f;



		////////////////

		/*public override ModConfig Clone() {
			var clone = (AdventureModeConfig)base.Clone();
			return clone;
		}*/
	}
}
