using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Bullwhip.Projectiles;


namespace Bullwhip {
	public partial class BullwhipMod : Mod {
		private static void Load_WeakRef_SoulBarriers() {
			SoulBarriers.SoulBarriersAPI.AddBarrierCreateHook( barrier => {
				if( !(barrier is SoulBarriers.Barriers.BarrierTypes.Spherical.Personal.PersonalBarrier) ) {
					return;
				}
				//if( barrier.HostType != SoulBarriers.Barriers.BarrierHostType.NPC ) {
				//	return;
				//}

				//

				bool OnPreBarrierEntityCollision( ref Entity intruder ) {
					if( BullwhipMod.CanEntityCollideWithBarrier(barrier, intruder) ) {
						BullwhipMod.OnPreBarrierEntityCollision_WeakRef_SoulBarriers( barrier, ref intruder );

						return false;
					}

					return true;
				}

				//

				barrier.AddPreBarrierEntityCollisionHook( OnPreBarrierEntityCollision );
			} );
		}


		////////////////

		private static bool CanEntityCollideWithBarrier( object rawBarrier, Entity intruder ) {
			Projectile intruderProj = intruder as Projectile;

			if( intruderProj == null ) {
				return false;
			}

			int whipProjType = ModContent.ProjectileType<BullwhipProjectile>();
			if( intruderProj.type != whipProjType ) {
				return false;
			}

			//

			var mybarrier = (SoulBarriers.Barriers.BarrierTypes.Barrier)rawBarrier;

			if( mybarrier.HostType == SoulBarriers.Barriers.BarrierHostType.NPC ) {
				return true;
			}

			//if( mybarrier.HostType == SoulBarriers.Barriers.BarrierHostType.Player ) {
			//}

			return false;
		}


		private static void OnPreBarrierEntityCollision_WeakRef_SoulBarriers( object rawBarrier, ref Entity intruder ) {
			Projectile intruderProj = intruder as Projectile;
			var mybarrier = (SoulBarriers.Barriers.BarrierTypes.Barrier)rawBarrier;

			Vector2 barrierPos = mybarrier.GetBarrierWorldCenter();
			Vector2 midOffset = (barrierPos - intruderProj.Center) * 0.5f;
			Vector2 midPos = intruderProj.Center + midOffset;

			mybarrier?.ApplyRawHit(
				hitAt: midPos,
				damage: mybarrier.Strength + 1,
				syncIfServer: true
			);

			//

			if( Main.netMode != NetmodeID.MultiplayerClient ) {
				Mod necMod = ModLoader.GetMod( "Necrotis" );

				if( necMod != null ) {
					var config = BullwhipConfig.Instance;
					float ectoPerc = config.Get<float>( nameof(config.WhippedBarrierEctoplasmPercentChance) );

					if( Main.rand.NextFloat() < ectoPerc ) {
						Item.NewItem(
							position: barrierPos,
							Type: necMod.ItemType("DillutedEctoplasmItem")
						);
					}
				}
			}
		}
	}
}