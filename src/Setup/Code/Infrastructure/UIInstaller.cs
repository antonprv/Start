// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Console.Interfaces;
using Game.Code.Common.Debug.UI;
using Game.Code.Common.DevConsole;
using Game.Code.Components.Mover;
using Godot;
using Zenjex;

namespace Game.Code.Infrastructure
{
	public partial class UIInstaller : RootInstaller
	{
		[Export] private UIMessage _uIMessage;
		[Export] private DevConsoleNode _devConsole;
		[Export] private MoverComponent _moverComponent;

		public override void InstallBindings( DiContainerBuilder builder )
		{
			builder.Register<IUIMessage>()
				.FromInstanceDebug( _uIMessage )
				.AsSingleton();

			builder.Register<IDevConsole>()
				.FromInstanceDebug( _devConsole.Service )
				.AsSingleton();

			builder.Register<IMoverComponent>()
				.FromInstance( _moverComponent )
				.AsSingleton();
		}

		public override void LaunchGame()
		{
		}
	}
}
