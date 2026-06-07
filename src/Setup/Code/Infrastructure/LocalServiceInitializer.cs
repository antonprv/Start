// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Common.Draw;
using Common.Time;
using Godot;
using Zenjex;

namespace Game.Code.Infrastructure
{
	public partial class LocalServiceInitializer : Node
	{
		[Export] private Node3D _rootNode;

		private ITimeService _timeService;

		[Inject]
		private void Construct( ITimeService timeService ) => 
			_timeService = timeService;

		public override void _EnterTree() => 
			DiContainer.Instance.Inject( this );

		public override void _Ready() => 
			DrawDebugRuntime.Initialize( GetTree(), _rootNode );

		public override void _Process( double delta ) =>
			_timeService.Tick( delta );
	}
}
