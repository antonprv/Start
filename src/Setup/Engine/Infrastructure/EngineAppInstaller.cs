// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Engine.Services.Input;
using Framework.Common.Random;
using Framework.Common.Time;
using Zenjex;

namespace Engine.Infrastructure
{
	public partial class EngineAppInstaller : RootInstaller
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