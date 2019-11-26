using HamstarHelpers.Helpers.Collisions;
using HamstarHelpers.Helpers.Debug;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static void PlaySound( Vector2 pos ) {
			int soundSlot = BullwhipMod.Instance.GetSoundSlot( SoundType.Custom, "Sounds/Custom/BullwhipCrackSound" );
			Main.PlaySound( (int)SoundType.Custom, (int)pos.X, (int)pos.Y, soundSlot, 0.5f );
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
			int endTileX = (int)maxPos.X >> 4;
			int endTileY = (int)maxPos.Y >> 4;

			bool isTileHit = false;
			IEnumerable<NPC> hitNpcs = null;

			///

			Func<Vector2, bool> checkPerUnit = (wldPos) => {
				return BullwhipItem.FindWhipUnitCollisionAt( plrCenter, wldPos, minWhipDist, out hitNpcs );
			};

			Func<int, int, bool> checkPerTile = (currTileX, currTileY) => {
				bool isPlatform;
				if( BullwhipItem.FindWhipTileCollisionAt(currTileX, currTileY, out isPlatform) && !isPlatform ) {
					isTileHit = true;
					endTileX = currTileX;
					endTileY = currTileY;
					return true;
				}
				return false;
			};

			///

			CollisionHelpers.CastRay( plrCenter, direction, maxWhipDist, checkPerUnit, checkPerTile );

			if( hitNpcs != null ) {
				foreach( NPC npc in hitNpcs ) {
					BullwhipItem.Strike( player, direction, npc );
				}
			}

			if( isTileHit ) {
				BullwhipItem.CreateHitFx( new Vector2(endTileX<<4, endTileY<<4), false );
			}
		}


		////

		private static bool FindWhipUnitCollisionAt(
					Vector2 startPos,
					Vector2 currPos,
					int minNpcHitWorldDistance,
					out IEnumerable<NPC> hitNpcs ) {
			int minNpcTileDist = minNpcHitWorldDistance >> 4;
			int minNpcTileDistSqr = minNpcTileDist * minNpcTileDist;
			Vector2 dist = startPos - currPos;
			Vector2 distSqr = dist * dist;

			if( (distSqr.X + distSqr.Y) >= minNpcTileDistSqr ) {
				hitNpcs = Main.npc.Where( anyNpc => {
					if( anyNpc == null || !anyNpc.active || anyNpc.immortal ) {
						return false;
					}

					//if( Math.Abs( nTileX - currTileX ) <= 2 && Math.Abs( nTileY - currTileY ) <= 2 ) {
					return Vector2.DistanceSquared(anyNpc.Center, currPos) < 1024;	// 32^2
				} );
			} else {
				hitNpcs = new NPC[] { };
			}

			return hitNpcs.Count() > 0;
		}

		private static bool FindWhipTileCollisionAt( int currTileX, int currTileY, out bool isPlatform ) {
			Tile tile = Framing.GetTileSafely( currTileX, currTileY );

			isPlatform = !Main.tileSolidTop[ tile.type ];
			return tile.active() && Main.tileSolid[ tile.type ];
		}


		////////////////

		public static void Strike( Player player, Vector2 direction, NPC npc ) {
			if( npc.immortal ) {
				return;
			}

			BullwhipConfig config = BullwhipConfig.Instance;
			int dmg = config.WhipDamage;
			float kb = config.WhipKnockback;

			switch( npc.aiStyle ) {
			case 1:     // slimes
				break;
			case 3:     // fighters
				break;
			case 14:    // bats
				if( BullwhipConfig.Instance.IncapacitatesBats && npc.aiStyle == 14 ) {//&& NPCID.Search.GetName(npc.type).Contains("Bat") ) {
					npc.aiStyle = 16;
					kb = 1f;
				}
				break;
			}

			npc.velocity += direction * kb;
			npc.StrikeNPC( dmg, kb, player.direction );

			BullwhipItem.CreateHitFx( npc.Center, true );
		}
	}
}