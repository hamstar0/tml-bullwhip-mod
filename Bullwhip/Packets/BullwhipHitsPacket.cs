using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using ModLibsCore.Classes.Errors;
using ModLibsCore.Libraries.Debug;
using ModLibsCore.Services.Network.SimplePacket;
using ModLibsCore.Libraries.DotNET.Extensions;
using Bullwhip.Items;


namespace Bullwhip.Packets {
	class BullwhipHitsPacket : SimplePacketPayload {
		public static void BroadcastFromClient(
					Player player,
					Vector2 start,
					Vector2 direction,
					(int TileX, int TileY)? hitTileAt,
					(int TileX, int TileY)? hitPlatformAt,
					IDictionary<int, ISet<int>> breakables,
					IEnumerable<NPC> hitNpcs,
					IEnumerable<Projectile> hitProjs,
					IEnumerable<Item> hitItems,
					IEnumerable<Player> hitPlayers,
					bool fxOnly,
					bool fxOnlyToClients ) {
			if( Main.netMode != NetmodeID.MultiplayerClient ) {
				throw new ModLibsException("Not client");
			}
			
			var payload = new BullwhipHitsPacket(
				playerWho: player.whoAmI,
				start: start,
				direction: direction,
				hitTileAt: hitTileAt,
				hitPlatformAt: hitPlatformAt,
				breakables: breakables,
				hitNpcWhos: hitNpcs.Select( n=>n.whoAmI ).ToArray(),
				hitProjWhos: hitProjs.Select( p=>Array.IndexOf(Main.projectile, p) ).ToArray(), //Main.projectileIdentity[p.owner, p.projUUID]
				hitItemWhos: hitItems.Select( i=>Array.IndexOf(Main.item, i) ).ToArray(),
				hitPlayerWhos: hitPlayers.Select( p=>p.whoAmI ).ToArray(),
				fxOnly: fxOnly,
				fxOnlyToClients: fxOnlyToClients
			);
			SimplePacket.SendToServer( payload );
		}
		

		public static void BroadcastFromServer(
					Player player,
					Vector2 start,
					Vector2 direction,
					(int TileX, int TileY)? hitTileAt,
					(int TileX, int TileY)? hitPlatformAt,
					IDictionary<int, ISet<int>> breakables,
					IEnumerable<NPC> hitNpcs,
					IEnumerable<Projectile> hitProjs,
					IEnumerable<Item> hitItems,
					IEnumerable<Player> hitPlayers,
					bool fxOnly,
					bool fxOnlyToClients ) {
			if( Main.netMode != NetmodeID.Server ) {
				throw new ModLibsException("Not server");
			}
			
			var payload = new BullwhipHitsPacket(
				playerWho: player.whoAmI,
				start: start,
				direction: direction,
				hitTileAt: hitTileAt,
				hitPlatformAt: hitPlatformAt,
				breakables: breakables,
				hitNpcWhos: hitNpcs.Select( n=>n.whoAmI ).ToArray(),
				hitProjWhos: hitProjs.Select( p=>Array.IndexOf(Main.projectile, p) ).ToArray(),
				hitItemWhos: hitItems.Select( i=>Array.IndexOf(Main.item, i) ).ToArray(),
				hitPlayerWhos: hitPlayers.Select( p=>p.whoAmI ).ToArray(),
				fxOnly: fxOnly,
				fxOnlyToClients: fxOnlyToClients
			);
			SimplePacket.SendToClient( payload, -1, -1 );
		}



		////////////////

		public int PlayerWho;
		
		public float StartX;
		public float StartY;
		public float DirectionX;
		public float DirectionY;

		public int HitTileAtX;
		public int HitTileAtY;
		public int HitPlatformAtX;
		public int HitPlatformAtY;

		public int BreakablesCount;
		public int[] BreakablesX;
		public int[] BreakablesY;

		public int[] HitNpcsWhos;
		public int[] HitProjWhos;
		public int[] HitItemWhos;
		public int[] HitPlayerWhos;

		public bool FxOnly;
		public bool FxOnlyToClients;



		////////////////

		private BullwhipHitsPacket() { }

		private BullwhipHitsPacket(
					int playerWho,
					Vector2 start,
					Vector2 direction,
					(int TileX, int TileY)? hitTileAt,
					(int TileX, int TileY)? hitPlatformAt,
					IDictionary<int, ISet<int>> breakables,
					int[] hitNpcWhos,
					int[] hitProjWhos,
					int[] hitItemWhos,
					int[] hitPlayerWhos,
					bool fxOnly,
					bool fxOnlyToClients ) {
			this.PlayerWho = playerWho;

			this.StartX = start.X;
			this.StartY = start.Y;
			this.DirectionX = direction.X;
			this.DirectionY = direction.Y;

			this.HitTileAtX = hitTileAt.HasValue
				? hitTileAt.Value.TileX
				: -1;
			this.HitTileAtY = hitTileAt.HasValue
				? hitTileAt.Value.TileY
				: -1;

			this.HitPlatformAtX = hitPlatformAt.HasValue
				? hitPlatformAt.Value.TileX
				: -1;
			this.HitPlatformAtY = hitPlatformAt.HasValue
				? hitPlatformAt.Value.TileY
				: -1;

			this.BreakablesCount = breakables.Count2D();
			this.BreakablesX = new int[ this.BreakablesCount ];
			this.BreakablesY = new int[ this.BreakablesCount ];

			int idx = 0;
			foreach( int tileX in breakables.Keys ) {
				foreach( int tileY in breakables[tileX] ) {
					this.BreakablesX[idx] = tileX;
					this.BreakablesY[idx] = tileY;
					idx++;
				}
			}

			this.HitNpcsWhos = hitNpcWhos;
			this.HitProjWhos = hitProjWhos;
			this.HitItemWhos = hitItemWhos;
			this.HitPlayerWhos = hitPlayerWhos;

			this.FxOnly = fxOnly;
			this.FxOnlyToClients = fxOnlyToClients;
		}


		////////////////

		private void Receive() {
			(int, int)? hitTileAt = null;
			(int, int)? hitPlatAt = null;
			if( this.HitTileAtX != -1 ) {
				hitTileAt = (this.HitTileAtX, this.HitTileAtY);
			}

			if( this.HitPlatformAtX != -1 ) {
				hitPlatAt = (this.HitPlatformAtX, this.HitPlatformAtY);
			}

			var breakables = new Dictionary<int, ISet<int>>();
			for( int i=0; i<this.BreakablesCount; i++ ) {
				breakables.Set2D( this.BreakablesX[i], this.BreakablesY[i] );
			}

			//

			BullwhipItem.ApplyStrike(
				whipOwner: Main.player[this.PlayerWho],
				start: new Vector2( this.StartX, this.StartY ),
				direction: new Vector2( this.DirectionX, this.DirectionY ),
				hitTileAt: hitTileAt,
				hitPlatformAt: hitPlatAt,
				breakables: breakables,
				hitNpcs: this.HitNpcsWhos.Select( who => Main.npc[who] ),
				hitProjs: this.HitProjWhos.Select( who => Main.projectile[who] ),
				hitItems: this.HitItemWhos.Select( who => Main.item[who] ),
				hitPlayers: this.HitPlayerWhos.Select( who => Main.player[who] ),
				fxOnly: this.FxOnly || (this.FxOnlyToClients && Main.netMode == NetmodeID.MultiplayerClient),
				syncSpecificHitsIfServer: true
			);
		}

		////

		public override void ReceiveOnServer( int fromWho ) {
			Player plr = Main.player[ this.PlayerWho ];
			if( plr?.active != true ) {
				LogLibraries.Alert( "Invalid whipper player index " + this.PlayerWho );
				return;
			}

			//
			
			this.Receive();

			//

			SimplePacket.SendToClient( this, -1, this.PlayerWho );
		}

		public override void ReceiveOnClient() {
			Player plr = Main.player[ this.PlayerWho ];
			if( plr?.active != true ) {
				LogLibraries.Alert( "Invalid whipper player index " + this.PlayerWho );
				return;
			}

			//
			
			this.Receive();
		}
	}
}
