using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using HamstarHelpers.Helpers.Debug;


namespace Bullwhip {
	class BullwhipNPC : GlobalNPC {
		private int WaterEscapeTickDurationBuffer = 0;


		////////////////

		public bool IsEnraged { get; private set; } = false;
		public bool IsConfuseWhipped { get; internal set; } = false;
		public bool IsCrippleWhipped { get; internal set; } = false;

		////

		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => false;



		////////////////

		public override bool PreAI( NPC npc ) {
			if( this.IsCrippleWhipped ) {
				this.WaterEscapeTickDurationBuffer--;

				switch( npc.aiStyle ) {
				case 1:    // slime
					if( !npc.wet && this.WaterEscapeTickDurationBuffer <= 0 ) {
						npc.aiStyle = 16;
					}
					break;
				case 16:	// "fish"
					if( npc.wet ) {
						this.WaterEscapeTickDurationBuffer = 90;

						npc.aiStyle = 1;
					}
					break;
				}
			}

			return base.PreAI( npc );
		}


		////////////////

		public void ApplyEnrage( NPC npc ) {
			if( this.IsEnraged ) {
				return;
			}

			switch( npc.netID ) {
			case NPCID.BunnySlimed:
			case NPCID.SlimeRibbonGreen:
			case NPCID.GreenSlime:
				npc.SetDefaults( NPCID.JungleSlime );
				npc.GetGlobalNPC<BullwhipNPC>().IsEnraged = true;
				break;
			case NPCID.BlueSlime:
			case NPCID.SlimeRibbonWhite:
			case NPCID.UmbrellaSlime:
				npc.SetDefaults( NPCID.SlimeSpiked );
				npc.GetGlobalNPC<BullwhipNPC>().IsEnraged = true;
				break;
			case NPCID.RedSlime:
			case NPCID.SlimeRibbonRed:
				//npc.SetDefaults( NPCID.BigCrimslime );
			case NPCID.YellowSlime:
			case NPCID.SlimeRibbonYellow:
			case NPCID.SandSlime:
				//npc.SetDefaults( NPCID.CorruptSlime );
			case NPCID.PurpleSlime:
			//npc.SetDefaults( NPCID.Slimer2 );
			case NPCID.BlackSlime:
			case NPCID.BabySlime:
			case NPCID.MotherSlime:
				npc.scale = 2f;
				npc.lifeMax *= 2;
				npc.life *= 2;
				npc.damage *= 2;
				npc.defense *= 2;
				this.IsEnraged = true;
				break;
			case NPCID.JungleSlime:
				npc.SetDefaults( NPCID.SpikedJungleSlime );
				npc.GetGlobalNPC<BullwhipNPC>().IsEnraged = true;
				break;
			case NPCID.IceSlime:
				npc.SetDefaults( NPCID.SpikedIceSlime );
				npc.GetGlobalNPC<BullwhipNPC>().IsEnraged = true;
				break;
			case NPCID.Pinky:
				npc.aiStyle = 2;	// Wheeee!
				npc.defense += 5;
				npc.knockBackResist -= 0.35f;
				npc.GetGlobalNPC<BullwhipNPC>().IsEnraged = true;
				//if( rand.Next( 4 ) == 0 ) {
				//	npc.SetDefaults( NPCID.Gastropod );	<- Drops Blessed Apple :(
				//	this.IsEnraged = true;
				//}
				break;
			}

			if( Main.netMode == NetmodeID.Server ) {
				NetMessage.SendData( MessageID.SyncNPC, -1, -1, null, npc.whoAmI, 0f, 0f, 0f, 0, 0, 0 );
			}

			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				if( this.IsEnraged ) {
					Main.NewText( npc.TypeName + " enraged!" );
				}
			}
		}
	}
}
