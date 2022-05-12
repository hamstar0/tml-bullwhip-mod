using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;
using ModLibsCore.Libraries.DotNET.Extensions;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		public static void ApplyStrike(
					Player whipOwner,
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
					bool syncSpecificHitsIfServer ) {
//LogHelpers.Log("WHIP 2 - start:"+start.ToShortString()+", hitNpcsAt:"+hitNpcsAt.Count2D()+", hitProjsAt:"+hitProjsAt.Count2D()+", hitItemsAt:"+hitItemsAt.Count2D());
			int maxWhipDist = BullwhipConfig.Instance.Get<int>( nameof(BullwhipConfig.MaximumWhipHitDist) );
			Vector2 maxPos = start + (direction * maxWhipDist);

			//

			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickDust( maxPos, Color.Red );
			}

			//

			// Destroy vines and shit
			if( !fxOnly ) {
				foreach( (int tileX, ISet<int> tileYs) in breakables ) {
					foreach( int tileY in tileYs ) {
						WorldGen.KillTile( tileX, tileY );

						//

						if( syncSpecificHitsIfServer && Main.netMode == NetmodeID.Server ) {
							NetMessage.SendData( MessageID.TileChange, -1, -1, null, 0, (float)tileX, (float)tileY, 0f, 0, 0, 0 );
						}
					}
				}
			}

			//

			// Hit beasties and thingies
			bool isNpcHit = BullwhipItem.ApplyWhipStrikeOnNPCs(
				whipOwner, direction, hitNpcs, fxOnly, syncSpecificHitsIfServer
			);
			bool isProjHit = BullwhipItem.ApplyWhipStrikeOnProjectiles(
				whipOwner, direction, hitProjs, fxOnly, syncSpecificHitsIfServer
			);
			bool isItemHit = BullwhipItem.ApplyWhipStrikeOnItems(
				whipOwner, direction, hitItems, fxOnly, syncSpecificHitsIfServer
			);
//LogLibraries.Log( "WHIP 1 - "+string.Join(", ", hitPlayers.Select(p=>p.name)) );
			bool isPlrHit = BullwhipItem.ApplyWhipStrikeOnPlayers(
				whipOwner, direction, hitPlayers, fxOnly, syncSpecificHitsIfServer
			);

			//

			// Grab platform (disabled for now)
			if( !isNpcHit && !isPlrHit && !fxOnly ) {
				if( hitPlatformAt.HasValue ) {
					BullwhipItem.GrabPlatform( whipOwner, hitPlatformAt.Value.TileX, hitPlatformAt.Value.TileY, fxOnly );
				}
			}

			//

			// Make fx
			if( !isNpcHit && !isPlrHit ) {
				if( !hitTileAt.HasValue && !hitPlatformAt.HasValue ) {
					BullwhipItem.CreateHitAirFx( maxPos );
				} else if( hitPlatformAt.HasValue ) {
					BullwhipItem.CreateHitSolidFx( hitPlatformAt.Value.ToVector2() * 16f );
				} else if( hitTileAt.HasValue ) {
					BullwhipItem.CreateHitSolidFx( hitTileAt.Value.ToVector2() * 16f );
				}
			}
		}
	}
}