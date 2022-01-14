﻿using Sandbox;

namespace Frostrial
{
	public class IsometricCamera : Camera
	{
		public float AngleChangeDelay => 0.2f;
		public Vector3? PointOfInterest { get; set; } = null;

		Vector3 PositionBeforeZoomOut = Vector3.Zero;
		Vector3 PreviousInputDirection = Vector3.Zero;
		TimeSince LastAngleChange = 0;
		Angles TargetAngles = new Angles( 30, 90, 0 );
		Rotation TargetRotation = new();
		public float Zoom = 0.7f; // Sorry I need this!

		public IsometricCamera()
		{
			Ortho = true;

			TargetRotation = TargetAngles.ToRotation();
			Rotation = TargetRotation;

			LastAngleChange = AngleChangeDelay;
		}

		[ClientCmd( "camera_pitch" )]
		public static void ChangeCameraPitch( float newPitch )
		{
			var cam = (Local.Pawn as Player).Camera as IsometricCamera;
			if ( cam == null )
				return;

			cam.TargetAngles = cam.TargetAngles.WithPitch( newPitch.Clamp( 15.0f, 45.0f ) );
			cam.TargetRotation = cam.TargetAngles.ToRotation();
		}

		[ClientCmd( "debug_set_poi" )]
		public static void DebugSetPOI( float x, float y, float z )
		{
			var cam = (Local.Pawn as Player).Camera as IsometricCamera;
			if ( cam == null )
				return;

			var poi = new Vector3( x, y, z );
			Log.Info( $"New Point of Interest: {poi}" );
			cam.PointOfInterest = poi;
		}

		[ClientCmd( "debug_reset_poi" )]
		public static void DebugSetPOI()
		{
			var cam = (Local.Pawn as Player).Camera as IsometricCamera;
			if ( cam == null )
				return;

			cam.PointOfInterest = null;
		}

		public override void Update()
		{
			var player = Local.Pawn as Player;
			if ( player == null )
				return;

			Rotation = Rotation.Slerp( Rotation, TargetRotation, 5f * Time.Delta );
			if ( PointOfInterest.HasValue )
			{
				PositionBeforeZoomOut = PositionBeforeZoomOut.LerpTo( PointOfInterest.Value, 5f * Time.Delta );
			}
			else
			{
				PositionBeforeZoomOut = player.Position;
			}
			Position = PositionBeforeZoomOut + Rotation.Backward * 2000 + Rotation.FromYaw( Rotation.Yaw() ).Forward * 10; // move it back a little bit
			OrthoSize = MathX.LerpTo( OrthoSize, Zoom, 7.5f * Time.Delta );
			Viewer = null;
		}

		public override void BuildInput( InputBuilder input )
		{
			var ibCameraCW = input.UsingController ? InputButton.SlotNext : InputButton.Menu;
			var ibCameraCCW = input.UsingController ? InputButton.SlotPrev : InputButton.Use;
			var ibCameraZoom = input.UsingController ? (input.Down(InputButton.Slot1) ? 1 : 0) - (input.Down(InputButton.Slot3) ? 1 : 0) : input.MouseWheel;

			if ( LastAngleChange >= AngleChangeDelay )
			{
				float rotDir = (input.Pressed( ibCameraCW ) ? -1 : 0) + (input.Pressed( ibCameraCCW ) ? 1 : 0);

				if ( rotDir != 0 )
				{
					TargetAngles = TargetAngles.WithYaw( MathX.NormalizeDegrees( TargetAngles.yaw + rotDir * 90 ) );
					TargetRotation = TargetAngles.ToRotation();

					LastAngleChange = 0;

					Sound.FromScreen( "camera_crank" );
				}
			}

			if ( ibCameraZoom != 0 )
			{
				Zoom = (Zoom - ibCameraZoom * Time.Delta * Zoom).Clamp( 0.15f, 1f );
			}


			if ( Local.Pawn is Player player && player.IsValid && !player.BlockMovement )
			{
				// add the view move
				input.ViewAngles = Rotation.LookAt( (player.MouseWorldPosition - player.Position).WithZ( 0 ), Vector3.Up ).Angles();
				input.ViewAngles.roll = 0;
			}

			// Just copy input as is
			input.InputDirection = input.AnalogMove;

			if ( input.InputDirection.Distance( PreviousInputDirection ) > 0.1 ) // if the player has changed the input direction
			{
				Player.ChangeMovementDirection( TargetAngles.yaw ); // change the angle
				PreviousInputDirection = input.InputDirection;
			}
		}
	}
}
