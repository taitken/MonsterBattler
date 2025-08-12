
namespace Game.Application.Messaging
{
    public readonly struct UnloadSceneCommand : ICommand
    {
        public readonly string Scene;
        public UnloadSceneCommand(string scene) => Scene = scene;
    }
}