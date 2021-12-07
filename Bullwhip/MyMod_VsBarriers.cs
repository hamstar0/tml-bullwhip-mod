using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Bullwhip.Projectiles;
using ModLibsCore.Services.ProjectileOwner;

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
		}
	}
}