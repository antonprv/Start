// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Common.Draw;
using Common.Time;
using Console.Interfaces;
using Godot;
using Logger;
using System;
using Zenjex;

namespace Game.Code.Infrastructure
{
	public partial class LocalServiceInitializer : Node
	{
		[Export] private Node3D _rootNode;

		private ITimeService _timeService;
		private IDevConsole _devConsole;

		[Inject]
		private void Construct( ITimeService timeService, IDevConsole devConsole )
		{
			_timeService = timeService;
			_devConsole = devConsole;
		}

		public override void _EnterTree() =>
			DiContainer.Instance.Inject( this );

		public override void _Ready()
		{
			DrawDebugRuntime.Initialize( GetTree(), _rootNode );
			GameLogger.Initialize( _devConsole );

			GetTree().Root.CloseRequested += OnCloseRequested;
		}

		private void OnCloseRequested() => 
			GameLogger.SaveLogsToFile();

		public override void _Process( double delta ) =>
			_timeService.Tick( delta );
	}
}
