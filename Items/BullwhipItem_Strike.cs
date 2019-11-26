using HamstarHelpers.Helpers.Collisions;
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
			
			Vector2 center = player.RotatedRelativePoint( player.MountedCenter, true );
			Vector2 pos = center + (direction * maxWhipDist);

			int tileHitX = ((int)pos.X)>>4, tileHitY = ((int)pos.Y)>>4, hitNpcWho = -1;

			Func<int, int, bool> collider = ( x, y ) => {
				if( BullwhipItem.FindWhipCollisionAt(x, y, out hitNpcWho) ) {
					tileHitX = x;
					tileHitY = y;
					return true;
				}
				return false;
			};

			bool isHit = TileCollisionHelpers.CastTileRay(
				pos + (direction * minWhipDist),
				direction,
				(int)maxWhipDist,
				collider
			);

			if( hitNpcWho != -1 ) {
				BullwhipItem.Strike( player, direction, hitNpcWho );
			}

			///

			var hitPos = new Vector2( tileHitX<<4, tileHitY<<4 );
			int width = 16;
			pos.X -= width / 2;
			pos.Y -= width / 2;

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

		private static bool FindWhipCollisionAt( int tileX, int tileY, out int npcWho ) {
			NPC npc = Main.npc.First( npc => {
				if( npc == null || !npc.active ) {
					return false;
				}

				int nPosX = (int)npc.position.X >> 4;
				int nPosY = (int)npc.position.Y >> 4;

				if( Math.Abs( nPosX - tileX ) <= 2 && Math.Abs( nPosY - tileY ) <= 2 ) {
					return true;
				}
				return false;
			} );

			npcWho = npc?.whoAmI ?? -1;
			if( npc != null ) {
				return true;
			}

			Tile tile = Framing.GetTileSafely( tileX, tileY );
			if( tile.active() && Main.tileSolid[tile.type] && !Main.tileSolidTop[tile.type] ) {
				return true;
			}

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