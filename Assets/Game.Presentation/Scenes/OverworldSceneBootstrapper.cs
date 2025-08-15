using System.Threading;
using Game.Application.DTOs;
using Game.Application.Interfaces;
using Game.Core;
using Game.Core.Logger;
using Game.Domain.Enums;
using Game.Presentation.GameObjects.OverworldMap;
using UnityEngine;

namespace Assets.Game.Presentation.Scenes
{
    /// <summary>
    /// Bootstrapper for the Overworld Scene.
    /// Initializes services and starts the overworld.
    /// </summary>
    public class OverworldSceneBootstrapper : MonoBehaviour
    {
        public OverworldView overworldView;
        private ILoggerService _loggerService;
        private IOverworldService _overworldService;
        private INavigationService _navigationService;
        void Awake()
        {
            _loggerService = ServiceLocator.Get<ILoggerService>();
            _overworldService = ServiceLocator.Get<IOverworldService>();
            _navigationService = ServiceLocator.Get<INavigationService>();
        }
        void Start()
        {
            // Try to get payload from navigation, but create default if none exists
            if (!_navigationService.TryTakePayload(GameScene.BattleScene, out OverworldPayload payload))
            {
                // Create a default payload when coming from MenuScene
                payload = new OverworldPayload(System.Guid.NewGuid());
            }
            
            _loggerService.Log("OverworldSceneBootstrapper started");
            overworldView.Bind(_overworldService.InitializeOverworld(payload));
            _overworldService.LoadOverworld(payload);
            _loggerService.Log("OverworldSceneBootstrapper finished");
        }
    }
}