using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Bullwhip.Items;


namespace Bullwhip.Recipes {
	class BullwhipItemRecipe : ModRecipe {
		public BullwhipItemRecipe( BullwhipMod mymod ) : base( mymod ) {
			this.AddIngredient( ItemID.Leather, 15 );
			this.AddIngredient( ItemID.Silk, 15 );
			this.AddTile( TileID.WorkBenches );
			this.SetResult( ModContent.GetInstance<BullwhipItem>() );
			this.AddRecipe();
		}


		public override bool RecipeAvailable() {
			return BullwhipConfig.Instance.Get<bool>( nameof(BullwhipConfig.RecipeEnabled) );
		}
	}
}