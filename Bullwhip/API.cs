using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ModLibsCore.Classes.Loadable;


namespace Bullwhip {
	public class BullwhipAPI : ILoadable {
		public delegate bool OnPreBullwhipHitEntity(
			Player whipOwner,
			/*Item whipItem,
			Projectile whipProjectile,*/
			Entity target,
			ref bool isDebuffImmune
		);



		////////////////

		public static void AddBullwhipEntityHitHook( OnPreBullwhipHitEntity hook ) {
			ModContent.GetInstance<BullwhipAPI>().HitHooks.Add( hook );
		}


		////////////////

		internal static bool ApplyBullwhipEntityHit(
					Player whipOwner,
					/*Item whipItem,
					Projectile whipProjectile,*/
					Entity target,
					ref bool isDebuffImmune ) {
			foreach( OnPreBullwhipHitEntity hook in ModContent.GetInstance<BullwhipAPI>().HitHooks ) {
				if( !hook.Invoke(whipOwner, /*whipItem, whipProjectile,*/ target, ref isDebuffImmune) ) {
					return false;
				}
			}
			return true;
		}



		////////////////

		private IList<OnPreBullwhipHitEntity> HitHooks = new List<OnPreBullwhipHitEntity>();



		////////////////

		void ILoadable.OnModsLoad() { }

		void ILoadable.OnPostModsLoad() { }

		void ILoadable.OnModsUnload() { }
	}
}
