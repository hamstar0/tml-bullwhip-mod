using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Libraries.Debug;


namespace Bullwhip.Items {
	public partial class BullwhipItem : ModItem {
		private static bool ApplyWhipStrikeOnProjectiles(
					Player player,
					Vector2 direction,
					IEnumerable<Projectile> hitProjs,
					bool fxOnly,
					bool syncIfServer ) {
			//IDictionary<Vector2, IEnumerable<Projectile>> hitProjsAt ) {
			var checkedProjs = new HashSet<Projectile>();
			bool isProjHit = false;

			//foreach( (Vector2 target, IEnumerable<Projectile> projs) in hitProjsAt ) {
			foreach( Projectile proj in hitProjs ) {
				if( proj?.active != true ) {
					continue;
				}

				//

				if( checkedProjs.Contains( proj ) ) {
					continue;
				}

				checkedProjs.Add( proj );

				//

				if( BullwhipItem.StrikeProjectile_If(player, direction, /*target,*/ proj, fxOnly, syncIfServer) ) {
					proj.friendly = true;
					proj.hostile = false;
				}

				//

				isProjHit = true;
			}

			return isProjHit;
		}


		////////////////

		public static bool StrikeProjectile_If(
					Player player,
					Vector2 direction,
					/*Vector2 hitWorldPosition,*/
					Projectile proj,
					bool fxOnly,
					bool syncIfServer ) {
			bool _ = false;
			if( !BullwhipAPI.OnPreBullwhipEntityHit(player, proj, fxOnly, ref _) ) {
				return false;
			}

			//

			//BullwhipConfig config = BullwhipConfig.Instance;
			//int dmg = config.WhipDamage;
			//float kb = config.WhipKnockback;

			// Ignore beehives
			if( proj.type == ProjectileID.BeeHive ) {
				return false;
			}

			//

			if( !fxOnly ) {
				BullwhipItem.StrikeProjectileNoFx( player, direction, proj, syncIfServer );
			}

			//
			
			BullwhipItem.CreateHitEntityFx( proj.Center );
			BullwhipItem.CreateHitEntityFx( proj.Center );

			//

			return true;
		}


		////

		public static void StrikeProjectileNoFx(
					Player player,
					Vector2 direction,
					/*Vector2 hitWorldPosition,*/
					Projectile proj,
					bool syncIfServer ) {
			float speed = proj.velocity.Length();
			direction.Normalize();
			proj.velocity.Normalize();

			proj.velocity = Vector2.Normalize( direction + proj.velocity );
			proj.velocity *= speed;

			//

			if( syncIfServer && Main.netMode == NetmodeID.Server ) {
				int projWho = Main.projectileIdentity[proj.owner, proj.projUUID];

				NetMessage.SendData( MessageID.SyncProjectile, -1, -1, null, projWho );
			}
		}
	}
}