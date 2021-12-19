﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System;

namespace Frostrial
{
	
	public class HutIndicator : Panel
	{

		Panel arrow;

		public HutIndicator()
		{

			Panel hut = Add.Panel( "HutIndicator" );
			arrow = hut.Add.Panel( "Arrow" );

		}

		public override void Tick()
		{

			Game current = Game.Current as Game;
			var player = Local.Pawn as Player;
			var hut = current.Hut;
			var hutScreen = hut.Position.ToScreen();
			var left = MathX.Clamp( hutScreen.x, 0.15f / Screen.Aspect, 1 - 0.15f / Screen.Aspect );
			var top = MathX.Clamp( hutScreen.y, 0.15f, 0.85f );

			Style.Left = Length.Fraction( left );
			Style.Top = Length.Fraction( top );

			var baseDistance = 500f;
			var baseOpacity = 0.25f;
			var dangerLevel = 1 - player.Warmth;

			Style.Opacity = MathX.Clamp( ( player.Position.Distance( hut.Position ) - baseDistance ) / baseDistance , 0, 1f ) * baseOpacity * ( baseOpacity + 1 / baseOpacity * dangerLevel );

			var rotation = -MathX.RadianToDegree( (float)Math.Atan2( 0.5f - left , 0.5 - top ) );

			var arrowRotate = new PanelTransform();
			arrowRotate.AddTranslateX( Length.Percent( ( MathF.Sin( rotation.DegreeToRadian() ) / 2 + 1 ) * 25 ) );
			arrowRotate.AddTranslateY( Length.Percent( ( MathF.Cos( rotation.DegreeToRadian() ) / 2 + 1 ) * 25 ) );
			arrowRotate.AddRotation( 0, 0, rotation );

			arrow.Style.Transform = arrowRotate;
			arrow.Style.Left = Length.Percent( -100 );
			arrow.Style.Top = Length.Percent( -125 );

		}
		
	}

	public class Hint : Panel
	{

		Label hintTitle;

		public Hint()
		{

			Player player = Local.Pawn as Player;

			Panel hintContainer = Add.Panel( "Hint" ).Add.Panel( "HintContainer" );
			hintTitle = hintContainer.Add.Label( "Lorem Ipsum", "HintTitle" );

		}

		public override void Tick()
		{

			float fadeTime = 1f;
			float textSpeed = 20f; // Letters per second

			Player player = Local.Pawn as Player;

			hintTitle.Text = player.HintText.Truncate( (int)( ( Time.Now - player.HintLifeTime ) * textSpeed ) );

			// Don't punish me, RealTimeSince doesn't seem to work when networked
			Style.Opacity = Math.Clamp( player.HintLifeDuration + fadeTime - ( Time.Now - player.HintLifeTime ), 0, 1 );
		}

	}

	public class Map : Panel
	{

		public Map()
		{

		}

		public override void Tick()
		{



		}

	}

	public class Trophies : Panel
	{

		public Trophies()
		{

		}

		public override void Tick()
		{

		}

	}

	public class Items : Panel
	{

		public Items()
		{

		}

		public override void Tick()
		{

		}

	}

	public partial class FrostrialHUD : Sandbox.HudEntity<RootPanel>
	{

		public FrostrialHUD()
		{

			if ( !IsClient ) return;

			var player = Local.Pawn as Player;

			PostProcess.Add( new StandardPostProcess() );

			RootPanel.StyleSheet.Load( "hud/FrostrialHUD.scss" );

			RootPanel.AddChild<HutIndicator>();
			RootPanel.AddChild<Hint>();

		}

		[Event.Tick.Client]
		private void ClientTick()
		{

			var player = Local.Pawn as Player;

			var pp = PostProcess.Get<StandardPostProcess>();

			pp.Saturate.Enabled = true;
			pp.Saturate.Amount = player.Warmth;

			pp.Blur.Enabled = true;
			pp.Blur.Strength = ( 1f - player.Warmth ) * 0.2f;

			pp.Vignette.Enabled = true;
			pp.Vignette.Intensity = 1f - player.Warmth;
			pp.Vignette.Color = Color.Black;
			pp.Vignette.Smoothness = 3f;
			pp.Vignette.Roundness = 2f;

		}

	}

	partial class Player : Sandbox.Player
	{
		[Net] public string HintText { get; set; } = "";
		[Net] public float HintLifeTime { get; set; } = 0f;
		[Net] public float HintLifeDuration { get; set; } = 0f;

		public void Hint( string text, float duration )
		{

			HintText = text;
			HintLifeDuration = duration;
			HintLifeTime = Time.Now;

		}

	}

}
