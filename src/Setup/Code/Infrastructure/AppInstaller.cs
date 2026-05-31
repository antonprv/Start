// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Game.Code.Services.Input;
using Zenjex;

namespace Game.Code.Infrastructure
{
	public partial class AppInstaller : RootInstaller
	{
		public override void InstallBindings( DiContainerBuilder builder )
		{
			builder.Register<IInputService>()
			  .To<InputService>()
			  .AsSingleton();
		}

		public override void LaunchGame()
		{
			// does nothing yet.
		}
	}
}