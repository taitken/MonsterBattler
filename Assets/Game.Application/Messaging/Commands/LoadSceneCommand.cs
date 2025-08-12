
using Game.Domain.Enums;

namespace Game.Application.Messaging
{
    public readonly struct LoadSceneCommand : ICommand
    {
        public readonly GameScene Scene;
        public readonly bool WithFade;
        public LoadSceneCommand(GameScene scene, bool withFade = true)
        {
            Scene = scene;
            WithFade = withFade;
        }
    }
}