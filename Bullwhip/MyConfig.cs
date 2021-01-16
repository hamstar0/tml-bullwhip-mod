using System;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using HamstarHelpers.Classes.UI.ModConfig;


namespace Bullwhip {
	class MyFloatInputElement : FloatInputElement { }




	public partial class BullwhipConfig : ModConfig {
		public static BullwhipConfig Instance => ModContent.GetInstance<BullwhipConfig>();



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

		[DefaultValue( 20f )]//25f
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float WhipKnockback { get; set; } = 20f;


		[Range( 1, 128 )]
		[DefaultValue( 32 )]
		public int WhipNPCMinHitRadius { get; set; } = 32;

		[Range( 1, 128 )]
		[DefaultValue( 36 )]
		public int WhipProjectileMinHitRadius { get; set; } = 36;

		[Range( 1, 128 )]
		[DefaultValue( 36 )]
		public int WhipItemHitMinRadius { get; set; } = 36;


		[DefaultValue( true )]
		public bool IncapacitatesBats { get; set; } = true;


		[Range( 0f, 100f )]
		[DefaultValue( 0f )] //11.4f
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float WhipLedgePullStrength { get; set; } = 0f;


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
