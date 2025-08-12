using Game.Application.Messaging;
using Game.Domain.Enums;
using UnityEngine;
namespace Game.Infrastructure.ScriptableObjects.Commands
{
    [CreateAssetMenu(menuName = "App/Commands/LoadScene")]
    public class LoadSceneCommandAsset : ScriptableObject, ICommandAsset
    {
        [Tooltip("Unity scene name as in Build Settings")]
        public GameScene scene;

        public bool withFade = true;

        public ICommand Create() => new LoadSceneCommand(scene, withFade);
    }
}

