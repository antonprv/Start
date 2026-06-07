// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Common.Random;
using Common.Time;
using Game.Code.Services.Input;
using Zenjex;

namespace Game.Code.Infrastructure
{
	public partial class AppInstaller : RootInstaller
	{
		public override void InstallBindings( DiContainerBuilder builder )
		{
			builder.Register<IRandomService>()
				.To<GodotRandomService>()
				.AsSingleton();

			builder.Register<ITimeService>()
				.To<GodotTimeService>()
				.AsSingleton();

			builder.Register<IInputService>()
			  .To<InputService>()
			  .AsSingleton();
		}

		public override void LaunchGame() { }
	}
}