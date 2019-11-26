using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Bullwhip {
	class BullwhipNPC : GlobalNPC {
		public bool IsEnraged { get; private set; } = false;

		////

		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => false;



		////////////////

		public void ApplyEnrage( NPC npc ) {
			if( this.IsEnraged ) { return; }

			switch( npc.netID ) {
			case NPCID.BunnySlimed:
			case NPCID.SlimeRibbonGreen:
			case NPCID.GreenSlime:
				npc.SetDefaults( NPCID.JungleSlime );
				this.IsEnraged = true;
				break;
			case NPCID.BlueSlime:
			case NPCID.SlimeRibbonWhite:
			case NPCID.UmbrellaSlime:
				npc.SetDefaults( NPCID.SlimeSpiked );
				this.IsEnraged = true;
				break;
			case NPCID.RedSlime:
			case NPCID.SlimeRibbonRed:
				npc.SetDefaults( NPCID.BigCrimslime );
				this.IsEnraged = true;
				break;
			case NPCID.YellowSlime:
			case NPCID.SlimeRibbonYellow:
			case NPCID.SandSlime:
				npc.SetDefaults( NPCID.CorruptSlime );
				this.IsEnraged = true;
				break;
			case NPCID.PurpleSlime:
				npc.SetDefaults( NPCID.Slimer2 );
				this.IsEnraged = true;
				break;
			case NPCID.JungleSlime:
				npc.SetDefaults( NPCID.SpikedJungleSlime );
				this.IsEnraged = true;
				break;
			case NPCID.IceSlime:
				npc.SetDefaults( NPCID.SpikedIceSlime );
				this.IsEnraged = true;
				break;
			case NPCID.Pinky:
				//if( rand.Next( 4 ) == 0 ) {
				//	npc.SetDefaults( NPCID.Gastropod );	<- Drops Blessed Apple
				//	mynpc.IsEnraged = true;
				//}
				break;
			}
		}
	}
}
