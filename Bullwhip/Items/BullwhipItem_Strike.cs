using System;
using System.Collections.Generic;
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
					bool fxOnly ) {
//LogHelpers.Log("WHIP 2 - start:"+start.ToShortString()+", hitNpcsAt:"+hitNpcsAt.Count2D()+", hitProjsAt:"+hitProjsAt.Count2D()+", hitItemsAt:"+hitItemsAt.Count2D());
			int maxWhipDist = BullwhipConfig.Instance.Get<int>( nameof(BullwhipConfig.MaximumWhipHitDist) );
			Vector2 maxPos = start + (direction * maxWhipDist);

			//

			if( BullwhipConfig.Instance.DebugModeStrikeInfo ) {
				Dust.QuickDust( maxPos, Color.Red );
			}

			//

			if( !fxOnly ) {
				foreach( (int tileX, ISet<int> tileYs) in breakables ) {
					foreach( int tileY in tileYs ) {
/*Timers.SetTimer("break_"+tileX+"_"+tileY, 50, false, () => {
	Dust.QuickDust( new Point(tileX, tileY), Color.Red );
	return true;
} );*/
						WorldGen.KillTile( tileX, tileY );

						if( Main.netMode != NetmodeID.SinglePlayer ) {
							NetMessage.SendData( MessageID.TileChange, -1, -1, null, 0, (float)tileX, (float)tileY, 0f, 0, 0, 0 );
						}
					}
				}
			}

			//

			bool isNpcHit = BullwhipItem.ApplyWhipStrikeOnNPCs( whipOwner, direction, hitNpcs, fxOnly );
			bool isProjHit = BullwhipItem.ApplyWhipStrikeOnProjectiles( whipOwner, direction, hitProjs, fxOnly );
			bool isItemHit = BullwhipItem.ApplyWhipStrikeOnItems( whipOwner, direction, hitItems, fxOnly );
			bool isPlrHit = BullwhipItem.ApplyWhipStrikeOnPlayers( whipOwner, direction, hitPlayers, fxOnly );

			//

			if( !isNpcHit && !isPlrHit && !fxOnly ) {
				if( hitPlatformAt.HasValue ) {
					BullwhipItem.GrabPlatform( whipOwner, hitPlatformAt.Value.TileX, hitPlatformAt.Value.TileY );
				}
			}

			//

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