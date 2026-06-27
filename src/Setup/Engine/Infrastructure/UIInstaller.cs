// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Engine.Components.Mover;
using Framework.Console.Interfaces;
using Game.Code.Common.Debug.UI;
using Game.Code.Common.DevConsole;
using Godot;
using Setup.Engine.Common.Debug.UI;
using Zenjex;

namespace Engine.Infrastructure
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
