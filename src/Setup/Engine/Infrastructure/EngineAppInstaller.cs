// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Engine.Services.Input;

using Framework.Common.Extensions;
using Framework.Common.Random;
using Framework.Common.Time;

using Godot;
using Physics;

using Zenjex;

using GEngine = Godot.Engine;

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

			builder
				.Register<IPhysicsWorld>()
				.FromFactory( () => FindPhysicsWorld()  )
				.AsSingleton();
		}

		private IPhysicsWorld FindPhysicsWorld() =>
			FindPhysicsWorld( GEngine.GetMainLoop().As<SceneTree>().Root );

		private IPhysicsWorld FindPhysicsWorld( Node node )
		{
			if ( node is IPhysicsWorld found )
				return found;

			foreach ( var child in node.GetChildren() )
			{
				var result = FindPhysicsWorld( child );
				if ( result != null )
					return result;
			}

			return null;
		}

		public override void LaunchGame() { }
	}
}