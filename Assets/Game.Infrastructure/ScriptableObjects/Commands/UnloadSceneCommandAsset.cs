using Game.Application.Messaging;
using UnityEngine;
namespace Game.Infrastructure.ScriptableObjects.Commands
{

    [CreateAssetMenu(menuName = "App/Commands/UnloadScene")]
    public class UnloadSceneCommandAsset : ScriptableObject, ICommandAsset
    {
        public string sceneName;
        public ICommand Create() => new UnloadSceneCommand(sceneName);
    }
}