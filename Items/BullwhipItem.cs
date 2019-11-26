using Bullwhip.Projectiles;
using Bullwhip.Recipes;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		private bool IsWhipping = false;
		private bool IsWhippingOnce = false;


		////////////////

		public override bool CloneNewInstances => false;



		////////////////

		public override void SetStaticDefaults() {
			this.DisplayName.SetDefault( "Bullwhip" );
			this.Tooltip.SetDefault( "Long reach, low damage melee weapon."
				+"\nIncapacitates bats."
				+"\nHold attack to immobilize a single target within range (allows item changing)."
				+"\nMay attach to platforms or vines."
			);
		}

		public override void SetDefaults() {
			//this.item.damage = 10;
			//this.item.knockBack = 3;
			this.item.melee = true;
			this.item.noUseGraphic = true;
			this.item.autoReuse = false;
			this.item.shoot = ModContent.ProjectileType<BullwhipProjectile>();
			this.item.shootSpeed = 20f;
			this.item.useStyle = 1;
			this.item.useTime = 36;
			this.item.useAnimation = 32;
			this.item.width = 40;
			this.item.height = 40;
			this.item.value = 30000;
			this.item.rare = 1;
			this.item.UseSound = SoundID.Item1;
		}

		////////////////

		public override void AddRecipes() {
			var recipe = new BullwhipItemRecipe( (BullwhipMod)this.mod );
			recipe.AddRecipe();
		}


		////////////////

		/*public override bool CanUseItem( Player player ) {
Main.NewText("1 "+player.ownedProjectileCounts[ this.item.shoot ]);
			return player.ownedProjectileCounts[ this.item.shoot ] < 1;
		}*/

		/*public override bool Shoot(
					Player player,
					ref Vector2 position,
					ref float speedX,
					ref float speedY,
					ref int type,
					ref int damage,
					ref float knockBack ) {
			return base.Shoot( player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack );
		}*/


		////////////////

		public void UpdateWhip( Player player ) {
			if( !this.IsWhipping ) {
				this.IsWhipping = player.itemAnimation > 0;
			} else {
				if( player.itemAnimation > 0 ) {
					if( Main.mouseLeftRelease ) {
						this.IsWhippingOnce = true;
					}
				} else {
					if( !this.IsWhippingOnce && Main.mouseLeft ) {
						BullwhipItem.AttemptWhipEnvelop( player );
					}

					this.IsWhipping = false;
					this.IsWhippingOnce = false;
				}
			}
		}
	}
}