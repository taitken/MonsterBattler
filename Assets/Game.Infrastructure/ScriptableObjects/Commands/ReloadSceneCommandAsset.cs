using Game.Application.Messaging;
using UnityEngine;
namespace Game.Infrastructure.ScriptableObjects.Commands
{
    [CreateAssetMenu(menuName = "App/Commands/ReloadScene")]
    public class ReloadSceneCommandAsset : ScriptableObject, ICommandAsset
    {
        public bool withFade = true;
        public ICommand Create() => new ReloadCurrentSceneCommand(withFade);
    }
}