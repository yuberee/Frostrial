﻿using Sandbox;

namespace Frostrial
{

	partial class Player : Sandbox.Player
	{

		public float InteractionRange { get; set; } = 40f;
		public float InteractionMaxDistance { get; set; } = 120f;

		protected override void TickPlayerUse()
		{
			if ( !IsServer ) return;

			Entity found = null;
			using ( Prediction.Off() )
			{
				if ( Input.Pressed( InputButton.Attack2 ) )
				{
					PlayClick();

					found = FindUsable();

					if (found is WorldEntity)
					{
						Hint( "I hate this place.", 1f );
						return;
					}

					if (found == null)
					{
						Hint( "That's too far away!", 2f );
						return;
					}
				}

				if ( found is IUse use && use.OnUse( this ) )
					return;
			}
		}

		protected override Entity FindUsable()
		{
			// First try a direct 0 width line
			var selectedEntity = Game.NearestInteractiveEntity( MouseWorldPosition, InteractionRange );

			if ( !(selectedEntity is WorldEntity) && selectedEntity.Position.Distance( Position ) < InteractionMaxDistance )
				return null;

			return selectedEntity;
		}

		public void HandleInteractions()
		{

			if ( IsClient ) return;

			if ( Input.Pressed( InputButton.Attack2 ) )
			{

				var selectedEntity = Game.NearestDescribableEntity( MouseWorldPosition, InteractionRange );

				if ( selectedEntity is not WorldEntity )
				{

					if ( selectedEntity.Position.Distance( Position ) < InteractionMaxDistance )
					{

						if ( selectedEntity is Player )
						{

							if ( selectedEntity == this )
							{

								ItemsOpen = true;
								BlockMovement = true;

								Hint( "Let's see...", 1.2f );
								PlayClick();

							}
							else
							{

								Hint( "Idiot.", 1f );
								PlayClick();

							}

						}

						if ( selectedEntity is Hole )
						{

							Fishing = true;
							BlockMovement = true;

							CurrentHole = selectedEntity;

							Hole hole = CurrentHole as Hole;
							hole.Bobber = true;

							Hint( ".   .   .   .   .", 1f );
							Play3D( "rod_woosh", this );
							Play3D( "rod_throw", selectedEntity );
							PlayClick();

						}

						if ( selectedEntity is YetiHand )
						{

							selectedEntity.Delete();

							AddMoney( 800f );

							Hint( "This Yeti Hand is old, lucky", 2f, true );
							PlayClick();

						}

						if ( selectedEntity is YetiScalp )
						{

							selectedEntity.Delete();

							AddMoney( 2500f );

							var yeti = new Yeti()
							{
								Position = new Vector3( 3275f, 3511.5f, 8f ),
								Victim = this as Entity

							};

							Play3D( "yeti_roar", yeti );

							Hint( "It's the Finnish Yeti! I must head back to the cabin!", 4f, true );
							PlayClick();

						}

						if ( selectedEntity is FishAuPoopooCaca )
						{

							selectedEntity.Delete();

							AddMoney( 19.84f );

							FishPoopoo( To.Single( Client ) );
							Hint( "I can always count on French Cuisine", 2.5f );
							PlayClick();

						}

						if ( selectedEntity is Hut )
						{


							ShopOpen = true;
							BlockMovement = true;
							Hint( "I'm almost there", 2f );
							PlayClick();
							Event.Run( "frostrial.crate_used" );

						}

						if ( selectedEntity is DeadFish )
						{

							DeadFish selectedFish = selectedEntity as DeadFish;


							switch ( selectedFish.Rarity )
							{

								case <= 0.3f:
									Hint( "This isn't going to cut it", 1.7f );
									break;

								case <= 0.6f:
									Hint( "There we go!", 1f );
									break;

								case > 0.6f:
									Hint( "That's a big one!", 1.2f );
									break;

							}

							Play3D( "fish_flop", selectedFish );
							PlayClick();

							AddMoney( selectedFish.Value );

							selectedFish.StopBuzzing();
							selectedFish.Delete();

						}


					}
					else
					{

						Hint( "That's too far away!", 2f );
						PlayClick();

					}

				}
				else
				{

					Hint( "I hate this place.", 1f );
					PlayClick();

				}

			}

		}

		[ClientRpc]
		public static void PlayClick()
		{

			Sound.FromScreen( "button_click" );

		}

		[ClientRpc]
		public static void Play3D( string sound, Entity source )
		{

			Sound.FromEntity( sound, source );

		}

		[ClientRpc]
		public static void FishPoopoo()
		{

			Event.Run( "frostrial.fish_caught", "fishaupoopoocaca", false );

		}

	}

}
