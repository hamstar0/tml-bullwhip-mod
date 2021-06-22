using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using ModLibsCore.Classes.Errors;
using ModLibsCore.Libraries.Debug;
using ModLibsCore.Services.Network.SimplePacket;
using Bullwhip.Items;


namespace Bullwhip.Packets {
	class BullwhipStrikePacket : SimplePacketPayload {
		public static void BroadcastFromClient( Player player, Vector2 direction ) {
			if( Main.netMode != NetmodeID.MultiplayerClient ) {
				throw new ModLibsException("Not client");
			}

			var payload = new BullwhipStrikePacket( player.whoAmI, direction );
			SimplePacket.SendToServer( payload );
		}



		////////////////

		public int PlayerWho;
		public Vector2 Direction;



		////////////////

		private BullwhipStrikePacket() { }

		private BullwhipStrikePacket( int playerWho, Vector2 direction ) {
			this.PlayerWho = playerWho;
			this.Direction = direction;
		}


		////////////////

		public override void ReceiveOnClient() {
			Player plr = Main.player[ this.PlayerWho ];
			if( plr?.active != true ) {
				LogLibraries.Alert( "Invalid whipper player index " + this.PlayerWho );
				return;
			}

			BullwhipItem.CastWhipStrike( plr, this.Direction, false );
		}

		public override void ReceiveOnServer( int fromWho ) {
			Player plr = Main.player[ this.PlayerWho ];
			if( plr?.active != true ) {
				LogLibraries.Alert( "Invalid whipper player index " + this.PlayerWho );
				return;
			}

			BullwhipItem.CastWhipStrike( plr, this.Direction, false );

			SimplePacket.SendToClient( this, -1, this.PlayerWho );
		}
	}
}
