// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Console.Interfaces;
using Game.Code.Common.Debug.UI;
using Game.Code.Common.DevConsole;
using Godot;
using Zenjex;

namespace Game.Code.Infrastructure
{
	public partial class UIInstaller : RootInstaller
	{
		[Export] private UIMessage _uIMessage;
		[Export] private DevConsoleNode _devConsole;

		public override void InstallBindings( DiContainerBuilder builder )
		{
			builder.Register<IUIMessage>()
				.FromInstance( _uIMessage )
				.AsSingleton();

			builder.Register<IDevConsole>()
				.FromInstance( _devConsole.Service )
				.AsSingleton();
		}

		public override void LaunchGame()
		{
		}
	}
}
