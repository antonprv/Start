// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Engine.Infrastructure;
using Zenjex;

namespace Setup.Code.Infrastructure
{
	public partial class AppInstaller : EngineAppInstaller
	{
		public override void InstallBindings( DiContainerBuilder builder )
		{
			base.InstallBindings( builder );

			// Game-specific Bindings
		}
	}
}
