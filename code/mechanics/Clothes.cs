using Sandbox;
using System.Collections.Generic;

namespace Frostrial
{
	partial class Player : Sandbox.Player
	{

		private Dictionary<string, ModelEntity> clothes = new();

		public void SetClothing( string clothingSlot, string modelPath )
		{

			// Delete pre-existing clothes on the same slot
			if ( clothes.ContainsKey( clothingSlot ) )
			{

				if ( clothes[clothingSlot].GetModelName() == modelPath ) return;

				clothes[clothingSlot].Delete();

			}

			if ( modelPath != "none" )
			{

				var entity = new ModelEntity();

				entity.SetModel( modelPath );
				entity.SetParent( this, true );

				clothes[clothingSlot] = entity;

			}

		}
		public void BasicClothes()
		{

			SetClothing( "hat", "models/clothing/hats/ushanka.vmdl" );
			SetClothing( "jacket", "models/clothing/jackets/jumper.vmdl");
			SetClothing( "trousers", "models/clothing/trousers/fishing_trousers.vmdl" );
			SetClothing( "gloves", "models/citizen_clothes/gloves/gloves_workgloves.vmdl" );
			SetClothing( "boots", "models/clothing/shoes/winter_boots.vmdl" );

			SetBodyGroup( 4, 1 ); // Remove Feet
			SetBodyGroup( 3, 1 ); // Remove Hands
			SetBodyGroup( 2, 1 ); // Remove Legs

		}

	}

}
