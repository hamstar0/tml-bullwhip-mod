using HamstarHelpers.Helpers.Collisions;
using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.NPCs;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static void AttemptWhipStrike( Player player, Vector2 direction ) {
			float minWhipDist = BullwhipConfig.Instance.MinimumWhipDist;
			float maxWhipDist = BullwhipConfig.Instance.MaximumWhipDist;
			direction.Normalize();
			
			Vector2 plrCenter = player.RotatedRelativePoint( player.MountedCenter, true );
			Vector2 maxPos = plrCenter + (direction * maxWhipDist);

			int hitTileX = (int)maxPos.X >> 4;
			int hitTileY = (int)maxPos.Y >> 4;
			int hitNpcWho = -1;

			Func<int, int, bool> collider = ( x, y ) => {
LogHelpers.Log( "testing "+x+", "+y);
Dust.NewDustPerfect( new Vector2(x<<4, y<<4), 1, null, 0, default(Color), 3f );
				bool _;
				if( BullwhipItem.FindWhipCollisionAt(x, y, out hitNpcWho, out _) ) {
					hitTileX = x;
					hitTileY = y;
LogHelpers.Log( "hit at "+x+", "+y+" npc:"+hitNpcWho);
					return true;
				}
				return false;
			};

			bool isHit = TileCollisionHelpers.CastTileRay(
				plrCenter,
				direction,
				(int)maxWhipDist,
				collider
			);

			if( hitNpcWho != -1 ) {
				BullwhipItem.Strike( player, direction, hitNpcWho );
			}

			///

			var hitPos = new Vector2( hitTileX<<4, hitTileY<<4 );
			int width = 16;
			hitPos.X -= width / 2;
			hitPos.Y -= width / 2;

			Dust dust;
			dust = Main.dust[ Dust.NewDust( hitPos, width, width, 31, 0f, 0f, 192, new Color(255, 255, 255), 0.75f ) ];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[ Dust.NewDust( hitPos, width, width, 31, 0f, 0f, 192, new Color(255, 255, 255), 0.75f ) ];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[ Dust.NewDust( hitPos, width, width, 31, 0f, 0f, 192, new Color(255, 255, 255), 0.75f ) ];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[ Dust.NewDust( hitPos, width, width, 31, 0f, 0f, 192, new Color(255, 255, 255), 0.75f ) ];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[ Dust.NewDust( hitPos, width, width, 31, 0f, 0f, 192, new Color(255, 255, 255), 0.75f ) ];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[ Dust.NewDust( hitPos, width, width, 31, 0f, 0f, 192, new Color(255, 255, 255), 0.75f ) ];
			dust.noGravity = true;
			dust.fadeIn = 3f;

			int soundSlot = BullwhipMod.Instance.GetSoundSlot( SoundType.Custom, "Sounds/Custom/BullwhipCrackSound" );
			Main.PlaySound( (int)SoundType.Custom, (int)hitPos.X, (int)hitPos.Y, soundSlot, 0.8f );
		}


		////

		private static bool FindWhipCollisionAt( int tileX, int tileY, out int hitNpcWho, out bool isPlatform ) {
			NPC hitNpc = Main.npc.FirstOrDefault( anyNpc => {
				if( anyNpc == null || !anyNpc.active || anyNpc.immortal ) {
					return false;
				}

				int nTileX = (int)anyNpc.position.X >> 4;
				int nTileY = (int)anyNpc.position.Y >> 4;

LogHelpers.LogOnce( "npc "+anyNpc.TypeName+"("+anyNpc.whoAmI+") distX:"+Math.Abs(nTileX - tileX)+", distY:"+Math.Abs(nTileY - tileY) );
				if( Math.Abs(nTileX - tileX) <= 2 && Math.Abs(nTileY - tileY) <= 2 ) {
					return true;
				}
				return false;
			} );

			hitNpcWho = hitNpc?.whoAmI ?? -1;
			if( hitNpcWho != -1 ) {
				isPlatform = false;
				return true;
			}

			Tile tile = Framing.GetTileSafely( tileX, tileY );
			if( tile.active() && Main.tileSolid[tile.type] ) {
				isPlatform = !Main.tileSolidTop[tile.type];
				return true;
			}

			isPlatform = false;
			return false;
		}


		////////////////

		public static void Strike( Player player, Vector2 direction, int npcWho ) {
			BullwhipConfig config = BullwhipConfig.Instance;
			NPC npc = Main.npc[ npcWho ];

			if( !npc.immortal ) {
				NPCHelpers.RawHurt( npc, config.WhipDamage, direction * config.WhipKnockback );
			}
		}
	}
}