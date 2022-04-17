using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ModLibsCore.Classes.Errors;
using ModLibsCore.Libraries.Debug;
using ModLibsCore.Services.ProjectileOwner;
using Bullwhip.Items;


namespace Bullwhip.Projectiles {
	public partial class BullwhipProjectile : ModProjectile {
		public override void AI() {
			Player ownerPlr = this.projectile.GetPlayerOwner();
			if( ownerPlr?.active != true || ownerPlr.CCed || ownerPlr.noItems || ownerPlr.dead ) {
				this.projectile.Kill();
				
				return;
			}

			//

			bool isCurrentPlayer = Main.netMode != NetmodeID.Server
				&& Main.myPlayer == ownerPlr.whoAmI;

			//

			ownerPlr.heldProj = this.projectile.whoAmI;

			//
			
			if( isCurrentPlayer ) {
				this.UpdatePositionRotationDirection_Local( ownerPlr, true );
			}

			//

			if( !isCurrentPlayer ) {
				Vector2 ownerRelativeMountedCenter = ownerPlr.RotatedRelativePoint( ownerPlr.MountedCenter, true );
				this.UpdatePosition( ownerPlr, ownerRelativeMountedCenter );

				this.projectile.spriteDirection = this.LastKnownSpriteDirection;
				this.projectile.rotation = this.LastKnownRotation;
			}

			//

			this.StepThroughFrames();
			//if( this.StepThroughFrames() ) {
			//	if( isCurrentPlayer ) {
			//		this.projectile.netUpdate = true;
			//	}
			//}

			//

			if( !this.IsLastFrame ) {
				if( this.projectile.timeLeft < 3 ) {
					this.projectile.timeLeft = 3;
				}
			}
		}


		////////////////

		private void UpdatePositionRotationDirection_Local( Player ownerPlr, bool storeRotationAndDirectionToAi1 ) {
			// Here be dragons!

			Vector2 ownerRelativeMountedCenter = ownerPlr.RotatedRelativePoint( ownerPlr.MountedCenter, true );

			//

			this.projectile.spriteDirection = ownerPlr.direction;
			//this.projectile.direction = ownerPlr.direction;

			//this.projectile.rotation = this.projectile.velocity.ToRotation();
			this.projectile.rotation = this.GetAimDirection_Local( ownerPlr ).ToRotation();

			//

			this.UpdatePosition( ownerPlr, ownerRelativeMountedCenter );

			//

			// Offset by 180 degrees here
			if( this.projectile.spriteDirection == -1 ) {
				this.projectile.rotation -= MathHelper.ToRadians( 180f );
			}

			//

			if( storeRotationAndDirectionToAi1 ) {
				this.projectile.ai[1] = this.projectile.rotation
					+ ( this.projectile.spriteDirection * 1000 );
			}
		}

		////

		private void UpdatePosition( Player ownerPlr, Vector2 ownerRelativeMountedCenter ) {
			int frameCount = Main.projFrames[ this.projectile.type ];

			float swingPercent = (float)this.projectile.frame / (float)frameCount;
			float swingPercentDir = ownerPlr.direction > 0
				? swingPercent
				: (1f - swingPercent);
			float swingDegreesArc = 180f * swingPercentDir;
			float swingDegrees = 180f + swingDegreesArc;
			float swingRadians = MathHelper.ToRadians( swingDegrees );
			var swingDir = new Vector2( (float)Math.Cos(swingRadians), (float)Math.Sin(swingRadians) );
			var swingPos = ownerRelativeMountedCenter + (swingDir * 10f);

			//

			this.projectile.Center = swingPos;
		}


		////////////////

		private bool StepThroughFrames() {
			var config = BullwhipConfig.Instance;
			Projectile proj = this.projectile;
			bool stepAnim = false;

			proj.frameCounter++;

			switch( proj.frame ) {
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
				stepAnim = proj.frameCounter >= 3;
				break;
			case 5:
				stepAnim = proj.frameCounter >= 2;
				break;
			case 6:
			case 7:
				stepAnim = proj.frameCounter >= 1;
				break;
			case 8:
				stepAnim = proj.frameCounter >= 2;
				break;
			}

			if( stepAnim ) {
				proj.frameCounter = 0;

				if( !this.IsLastFrame ) {
					proj.frame++;
				}

				if( Main.netMode != NetmodeID.Server ) {
					if( proj.frame == 4 ) {
						float maxHitDist = config.Get<int>( nameof(BullwhipConfig.MaximumWhipHitDist) );

						Vector2 sndPos = proj.Center;
						sndPos += Vector2.Normalize(proj.velocity) * maxHitDist;

						BullwhipItem.PlaySound( sndPos );
					}
				}
			}

			return stepAnim;
		}
	}
}
