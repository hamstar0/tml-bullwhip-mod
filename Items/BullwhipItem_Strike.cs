using HamstarHelpers.Helpers.Collisions;
using HamstarHelpers.Helpers.Debug;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static void PlaySound( Vector2 pos ) {
			int soundSlot = BullwhipMod.Instance.GetSoundSlot( SoundType.Custom, "Sounds/Custom/BullwhipCrackSound" );
			Main.PlaySound( (int)SoundType.Custom, (int)pos.X, (int)pos.Y, soundSlot, 0.8f );
		}

		public static void CreateHitFx( Vector2 pos, bool isNpc ) {
			Color color = isNpc ? Color.Lerp(Color.Red, Color.White, 0.5f) : Color.White;
			int alpha = isNpc ? 128 : 192;
			float scale = 0.75f;
			int width = 16;
			pos.X -= width / 2;
			pos.Y -= width / 2;

			Dust dust;
			dust = Main.dust[Dust.NewDust( pos, width, width, 31, 0f, 0f, alpha, color, scale )];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[Dust.NewDust( pos, width, width, 31, 0f, 0f, alpha, color, scale )];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[Dust.NewDust( pos, width, width, 31, 0f, 0f, alpha, color, scale )];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[Dust.NewDust( pos, width, width, 31, 0f, 0f, alpha, color, scale )];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[Dust.NewDust( pos, width, width, 31, 0f, 0f, alpha, color, scale )];
			dust.noGravity = true;
			dust.fadeIn = 3f;
			dust = Main.dust[Dust.NewDust( pos, width, width, 31, 0f, 0f, alpha, color, scale )];
			dust.noGravity = true;
			dust.fadeIn = 3f;
		}


		////////////////

		public static void AttemptWhipStrike( Player player, Vector2 direction ) {
			int minWhipDist = BullwhipConfig.Instance.MinimumWhipDist;
			int maxWhipDist = BullwhipConfig.Instance.MaximumWhipDist;
			direction.Normalize();
			
			Vector2 plrCenter = player.RotatedRelativePoint( player.MountedCenter, true );
			Vector2 maxPos = plrCenter + (direction * maxWhipDist);

			int srcTileX = (int)plrCenter.X >> 4;
			int srcTileY = (int)plrCenter.Y >> 4;
			int hitTileX = (int)maxPos.X >> 4;
			int hitTileY = (int)maxPos.Y >> 4;
			IEnumerable<NPC> hitNpcs = null;

			Func<int, int, bool> collider = (currTileX, currTileY) => {
				bool _;
				if( BullwhipItem.FindWhipCollisionAt(srcTileX, srcTileY, currTileX, currTileY, minWhipDist, out hitNpcs, out _) ) {
					hitTileX = currTileX;
					hitTileY = currTileY;
					return true;
				}
				return false;
			};

			TileCollisionHelpers.CastTileRay( plrCenter, direction, maxWhipDist, collider );

			if( hitNpcs != null ) {
				foreach( NPC npc in hitNpcs ) {
					BullwhipItem.Strike( player, direction, npc );
				}
			}

			if( hitNpcs == null || hitNpcs.Count() == 0 ) {
				var hitWorldPos = new Vector2( hitTileX << 4, hitTileY << 4 );

				BullwhipItem.CreateHitFx( hitWorldPos, false );
				//BullwhipItem.PlaySound( hitPos );
			}
		}


		////

		private static bool FindWhipCollisionAt(
					int srcTileX,
					int srcTileY,
					int currTileX,
					int currTileY,
					int minNpcHitWorldDistance,
					out IEnumerable<NPC> hitNpcs,
					out bool isPlatform ) {
			int minNpcTileDist = minNpcHitWorldDistance >> 4;
			int minNpcTileDistSqr = minNpcTileDist * minNpcTileDist;
			int distX = srcTileX - currTileX;
			int distY = srcTileY - currTileY;
			int distXSqr = distX * distX;
			int distYSqr = distY * distY;

			if( (distXSqr + distYSqr) >= minNpcTileDistSqr ) {
				hitNpcs = Main.npc.Where( anyNpc => {
					if( anyNpc == null || !anyNpc.active || anyNpc.immortal ) {
						return false;
					}

					int nTileX = (int)anyNpc.position.X >> 4;
					int nTileY = (int)anyNpc.position.Y >> 4;

					if( Math.Abs( nTileX - currTileX ) <= 2 && Math.Abs( nTileY - currTileY ) <= 2 ) {
						return true;
					}
					return false;
				} );

				if( hitNpcs.Count() > 0 ) {
					isPlatform = false;
					return true;
				}
			}

			Tile tile = Framing.GetTileSafely( currTileX, currTileY );
			if( tile.active() && Main.tileSolid[tile.type] ) {
				isPlatform = !Main.tileSolidTop[tile.type];
				hitNpcs = new List<NPC>();
				return true;
			}

			isPlatform = false;
			hitNpcs = new List<NPC>();
			return false;
		}


		////////////////

		public static void Strike( Player player, Vector2 direction, NPC npc ) {
			if( npc.immortal ) {
				return;
			}

			BullwhipConfig config = BullwhipConfig.Instance;
			int dmg = config.WhipDamage;
			float kb = config.WhipKnockback;

			if( BullwhipConfig.Instance.IncapacitatesBats && npc.aiStyle == 14 ) {//&& NPCID.Search.GetName(npc.type).Contains("Bat") ) {
				npc.aiStyle = 16;
				kb = 1f;
			}

			npc.velocity += direction * kb;
			npc.StrikeNPC( dmg, kb, player.direction );

			BullwhipItem.CreateHitFx( npc.Center, true );
		}
	}
}