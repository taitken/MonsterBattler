
using Game.Domain.Enums;

namespace Game.Application.Messaging
{

    public readonly struct LoadAdditiveSceneCommand : ICommand
    {
        public readonly GameScene Scene;
        public readonly bool ActivateOnLoad;
        public LoadAdditiveSceneCommand(GameScene scene, bool activateOnLoad = true)
        {
            Scene = scene;
            ActivateOnLoad = activateOnLoad;
        }
    }
}