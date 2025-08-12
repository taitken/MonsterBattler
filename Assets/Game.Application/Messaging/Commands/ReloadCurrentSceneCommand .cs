
namespace Game.Application.Messaging
{
    public readonly struct ReloadCurrentSceneCommand : ICommand
    {
        public readonly bool WithFade;
        public ReloadCurrentSceneCommand(bool withFade = true) => WithFade = withFade;
    }
}