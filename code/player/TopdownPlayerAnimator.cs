using Sandbox;
using System;

namespace Frostrial
{
	public class TopdownPlayerAnimator : PawnAnimator
	{
		TimeSince TimeSinceFootShuffle = 60;

		public override void Simulate()
		{


			DoWalk();

			Player ply = Pawn as Player;

			if ( ply.BlockMovement ) return;

			var idealRotation = Rotation.LookAt( ply.MouseWorldPosition.WithZ( Position.z ) - Position, Vector3.Up );

			DoRotation( idealRotation );

			Vector3 aimPos = Pawn.EyePosition + idealRotation.Forward * 200;
			Vector3 lookPos = aimPos;
			SetLookAt( "aim_eyes", lookPos );
			SetLookAt( "aim_head", lookPos );
			SetLookAt( "aim_body", aimPos );


		}

		public virtual void DoRotation( Rotation idealRotation )
		{

			//
			// Our ideal player model rotation is the way we're facing
			//
			var allowYawDiff = (Pawn as Player).ActiveChild == null ? 90 : 50;

			float turnSpeed = 0.01f;

			//
			// If we're moving, rotate to our ideal rotation
			//
			Rotation = Rotation.Slerp( Rotation, idealRotation, WishVelocity.Length * Time.Delta * turnSpeed );

			//
			// Clamp the foot rotation to within 120 degrees of the ideal rotation
			//
			Rotation = Rotation.Clamp( idealRotation, allowYawDiff, out var change );

			//
			// If we did restrict, and are standing still, add a foot shuffle
			//
			if ( change > 1 && WishVelocity.Length <= 1 ) TimeSinceFootShuffle = 0;

			SetAnimParameter( "b_shuffle", TimeSinceFootShuffle < 0.1 );
		}

		void DoWalk()
		{
			{
				var dir = Velocity;
				var forward = Rotation.Forward.Dot( dir );
				var sideward = Rotation.Right.Dot( dir );

				var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

				SetAnimParameter( "move_direction", angle );
				SetAnimParameter( "move_speed", Velocity.Length );
				SetAnimParameter( "move_groundspeed", Velocity.WithZ( 0 ).Length );
				SetAnimParameter( "move_y", sideward );
				SetAnimParameter( "move_x", forward );

				Player ply = Pawn as Player;

				if ( ply.BlockMovement )
				{

					SetAnimParameter( "move_direction", 0 );
					SetAnimParameter( "move_speed", 0 );
					SetAnimParameter( "move_groundspeed", 0 );
					SetAnimParameter( "move_y", 0 );
					SetAnimParameter( "move_x", 0 );

				}

			}

		}

	}

}
