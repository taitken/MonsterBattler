using System.Threading;
using Game.Application.Interfaces;
using Game.Core;
using Game.Core.Logger;
using UnityEngine;

namespace Assets.Game.Presentation.Scenes
{
    /// <summary>
    /// Bootstrapper for the Battle Scene.
    /// Initializes services and starts the battle.
    /// </summary>
    public class OverworldSceneBootstrapper : MonoBehaviour
    {
        private ILoggerService _loggerService;
        private INavigationService _navigationService;
        private IBattleHistoryService _battleHistoryService;
        void Awake()
        {
            _loggerService = ServiceLocator.Get<ILoggerService>();
            _navigationService = ServiceLocator.Get<INavigationService>();
            _battleHistoryService = ServiceLocator.Get<IBattleHistoryService>();
        }
        void Start()
        {
            var ct = new CancellationToken();
            _loggerService.Log("OverworldSceneBootstrapper started");
            _loggerService.Log("OverworldSceneBootstrapper finished starting battle");
        }
    }
}