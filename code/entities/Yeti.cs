using Sandbox;

namespace Frostrial
{

	public partial class Yeti : AnimatedEntity
	{

		[Net] public Entity Victim { get; set; }

		public override void Spawn()
		{

			base.Spawn();

			Scale = 1.7f;

			SetModel( "models/jorma/jorma.vmdl" );

			var suit = new ModelEntity();
			suit.SetModel( "models/randommodels/yeti/yeti.vmdl" );
			suit.SetParent( this, true );

			RenderColor = RenderColor.WithAlpha( 0 );

		}

		[Event.Tick.Server]
		public void OnTick()
		{

			Velocity = Rotation.Forward * 165f;
			Rotation = (Victim.Position.WithZ( 0 ) - Position.WithZ( 0 )).EulerAngles.ToRotation();

			if ( Victim.Position.Distance( Position ) < 40f )
			{
				if ( Victim is not Player player )
					return;

				if ( player.Jumpscare == 0 )
				{

					player.BlockMovement = true;
					player.Velocity = Vector3.Zero;
					player.Jumpscare = 1;
					player.JumpscareTimer = 4f;

					player.Delay( 4 );

					PlayCreep( false );

				}

				if ( player.JumpscareTimer <= 0f )
				{

					player.Client.Kick();

				}

			}

			if ( Game.IsInside( Victim.Position, new Vector3( -1395, -2745, 0 ), new Vector3( -1164, -2394, 40 ) ) )
			{

				if ( Position.Distance( Game.Instance.HutEntity.Position ) <= 210 )
				{

					if ( Victim is not Player player )
						return;

					if ( player.Jumpscare == 0 )
					{

						player.JumpscareTimer = 3f;
						player.BlockMovement = true;
						player.Velocity = Vector3.Zero;

						player.Jumpscare = 2;

						player.Delay( 7 ); // Keep him quiet during the cutscene

						PlayCreep( true );

					}

					if ( player.JumpscareTimer <= -4f )
					{

						player.BlockMovement = false;
						player.Jumpscare = 0;

						player.Say( VoiceLine.YetiJumpscare );

						Delete();

					}

					Velocity = Vector3.Zero;


				}

			}

			Position += Velocity * Time.Delta;

			SetAnimParameter( "move_x", Velocity.Length / Scale );

		}

		[ClientRpc]
		public static void PlayCreep( bool window )
		{

			Sound.FromScreen( window ? "yeti_window" : "yeti_jumpscare" );

		}

	}

}
