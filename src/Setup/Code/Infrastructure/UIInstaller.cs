// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Game.Code.Common.Debug.UI;
using Godot;
using Zenjex;

namespace Game.Code.Infrastructure
{
	public partial class UIInstaller : RootInstaller
	{
		[Export] private UIMessage _uIMessage;

		public override void InstallBindings( DiContainerBuilder builder )
		{
			builder.Register<IUIMessage>()
				.FromInstance( _uIMessage )
				.AsSingleton();
		}

		public override void LaunchGame()
		{
		}
	}
}
