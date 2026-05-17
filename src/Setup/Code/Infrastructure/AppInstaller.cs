// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Common.Extensions.Logging;
using Code.Services.Input;
using ZenjexGodot;

namespace Code.Infrastructure
{
	public partial class AppInstaller : RootInstaller
	{
		public override void InstallBindings( DiContainerBuilder builder )
		{
			builder.Register<IGameLog>()
				.To<GameLogger>()
				.AsSingleton();

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