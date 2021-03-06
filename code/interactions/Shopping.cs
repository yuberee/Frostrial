using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Frostrial
{

	partial class Player : Sandbox.Player
	{

		[Net, Local, Change] public float Money { get; set; } = 0f;
		[Net] public bool ShopOpen { get; set; } = false;
		[Net] public bool UpgradedDrill { get; set; } = false;
		[Net] public bool UpgradedRod { get; set; } = false;
		[Net] public bool UpgradedCoat { get; set; } = false;
		[Net] public bool MultiItems { get; set; } = false;

		public void HandleShopping()
		{

			SetClothing( "jacket", UpgradedCoat ? "models/clothing/jackets/parka.vmdl" : "models/clothing/jackets/jumper.vmdl" );

			if ( Jumpscare == 3 )
			{

				if ( JumpscareTimer <= -2 )
				{

					Game.Instance.CurrentTitle = "Thank you for playing";
					Game.Instance.CurrentSubtitle = "Game made by SmallFish and friends for JamBox 2021";
					Curtains = true;
					Delay( 7 );

				}

				if ( JumpscareTimer <= -9 )
				{

					Curtains = false;
					Jumpscare = 1;

				}

			}

			if ( Jumpscare == 1 )
			{

				if ( JumpscareTimer <= -9.8f )
				{

					Client.Kick();

				}

			}

			MultiItems = Input.Down( InputButton.Run );

		}

		[ConCmd.Server]
		public static void CloseShop()
		{

			Player player = ConsoleSystem.Caller.Pawn as Player;

			player.ShopOpen = false;
			player.BlockMovement = false;

		}

		[ConCmd.Server]
		public static void BuyBait()
		{

			Player player = ConsoleSystem.Caller.Pawn as Player;

			int totalBought = player.MultiItems ? (int)Math.Min( (int)player.Money / Game.Prices["bait"], 10 ) : 1;

			if ( player.Money >= Game.Prices["bait"] * totalBought )
			{

				player.Baits += totalBought;
				player.AddMoney( -Game.Prices["bait"] * totalBought );

			}

		}

		[ConCmd.Server]
		public static void BuyCampfire()
		{

			Player player = ConsoleSystem.Caller.Pawn as Player;

			int totalBought = player.MultiItems ? (int)Math.Min( (int)player.Money / Game.Prices["campfire"], 10 ) : 1;

			if ( player.Money >= Game.Prices["campfire"] * totalBought )
			{

				player.Campfires += totalBought;
				player.AddMoney( -Game.Prices["campfire"] * totalBought );

			}

		}

		[ConCmd.Server]
		public static void UpgradeCoat()
		{

			Player player = ConsoleSystem.Caller.Pawn as Player;

			if ( player.Money >= Game.Prices["coat"] && !player.UpgradedCoat )
			{

				player.UpgradedCoat = true;
				player.AddMoney( -Game.Prices["coat"] );

			}

		}

		[ConCmd.Server]
		public static void UpgradeDrill()
		{

			Player player = ConsoleSystem.Caller.Pawn as Player;

			if ( player.Money >= Game.Prices["drill"] && !player.UpgradedDrill )
			{

				player.UpgradedDrill = true;
				player.AddMoney( -Game.Prices["drill"] );

			}

		}

		[ConCmd.Server]
		public static void UpgradeRod()
		{

			Player player = ConsoleSystem.Caller.Pawn as Player;

			if ( player.Money >= Game.Prices["rod"] && !player.UpgradedRod )
			{

				player.UpgradedRod = true;
				player.AddMoney( -Game.Prices["rod"] );

			}


		}

		[ConCmd.Server]
		public static void Win()
		{

			Player player = ConsoleSystem.Caller.Pawn as Player;

			if ( player.Money >= Game.Prices["plane"] )
			{

				player.BlockMovement = true;
				player.Velocity = Vector3.Zero;
				player.Jumpscare = 3;
				player.JumpscareTimer = 6f;

				player.BlockMovement = true;
				player.Say( VoiceLine.Outro );

				player.AddMoney( -Game.Prices["plane"] );

			}

		}

		public void AddMoney( float amount )
		{
			Host.AssertServer();

			Money += amount;

		}

		public void OnMoneyChanged( float oldValue, float newValue )
		{
			Event.Run( "frostrial.money", newValue - oldValue );
		}

	}

	public class Shop : Panel
	{

		Button baitButton;
		Button campfireButton;
		Button coatButton;
		Button drillButton;
		Button rodButton;
		Button planeButton;
		Label baitText;
		Label campfireText;
		Label coatText;
		Label drillText;
		Label rodText;
		Label planeText;

		public Shop()
		{

			Player player = Local.Pawn as Player;

			Panel shopPanel = Add.Panel( "Shop" ).Add.Panel( "container" );

			Add.Panel( "Close" ).Add.Button( "", "button", () =>
			{

				Sound.FromScreen( "button_click" );
				Player.CloseShop();

			} ).Add.Label( "Close", "title" );

			baitButton = shopPanel.Add.Button( "", "button", () =>
			{

				Sound.FromScreen( "button_click" );
				Player.BuyBait();

			} );

			baitText = baitButton.Add.Label( "Buy Bait", "title" );

			campfireButton = shopPanel.Add.Button( "", "button", () =>
			{

				Sound.FromScreen( "button_click" );
				Player.BuyCampfire();

			} );

			campfireText = campfireButton.Add.Label( "Buy Campfire", "title" );

			coatButton = shopPanel.Add.Button( "", "button", () =>
			{

				Sound.FromScreen( "button_click" );
				Player.UpgradeCoat();

			} );

			coatText = coatButton.Add.Label( "Upgrade Coat", "title" );

			drillButton = shopPanel.Add.Button( "", "button", () =>
			{

				Sound.FromScreen( "button_click" );
				Player.UpgradeDrill();

			} );

			drillText = drillButton.Add.Label( "Upgrade Drill", "title" );

			rodButton = shopPanel.Add.Button( "", "button", () =>
			{

				Sound.FromScreen( "button_click" );
				Player.UpgradeRod();

			} );

			rodText = rodButton.Add.Label( "Upgrade Rod", "title" );

			planeButton = shopPanel.Add.Button( "", "button", () =>
			{

				Sound.FromScreen( "button_click" );
				Player.CloseShop();
				Player.Win();

			} );

			planeText = planeButton.Add.Label( "BUY PLANE TICKET", "title" );

		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player player )
				return;

			Parent.Style.PointerEvents = player.ShopOpen ? "all" : "visible";
			Style.Opacity = player.ShopOpen ? 1 : 0;

			baitText.Text = $"( €{Game.Prices["bait"]} ) Buy Bait ({player.Baits})";
			campfireText.Text = $"( €{Game.Prices["campfire"]} ) Buy Campfire ({player.Campfires})";
			coatText.Text = player.UpgradedCoat ? "[BOUGHT]" : $"( €{Game.Prices["coat"]} ) Upgrade Coat";
			drillText.Text = player.UpgradedDrill ? "[BOUGHT]" : $"( €{Game.Prices["drill"]} ) Upgrade Drill";
			rodText.Text = player.UpgradedRod ? "[BOUGHT]" : $"( €{Game.Prices["rod"]} ) Upgrade Rod";
			planeText.Text = $"( €{Game.Prices["plane"]} ) Buy Plane Ticket";

		}

	}

}
