using Code.Services.Input;
using ZenjexGodot;

namespace Code.Infrastructure
{
    public partial class AppInstaller : RootInstaller
    {
        public override void InstallBindings(DiContainerBuilder builder)
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
