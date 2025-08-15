using System.Threading;
using Game.Application.DTOs;
using Game.Application.Interfaces;
using Game.Core;
using Game.Core.Logger;
using Game.Domain.Enums;
using UnityEngine;

namespace Assets.Game.Presentation.Scenes
{
    /// <summary>
    /// Bootstrapper for the Battle Scene.
    /// Initializes services and starts the battle.
    /// </summary>
    public class BattleSceneBootstrapper : MonoBehaviour
    {
        private ILoggerService _loggerService;
        private IBattleService _battleService;
        private INavigationService _navigationService;
        void Awake()
        {
            _loggerService = ServiceLocator.Get<ILoggerService>();
            _battleService = ServiceLocator.Get<IBattleService>();
            _navigationService = ServiceLocator.Get<INavigationService>();
        }
        void Start()
        {
            _navigationService.TryTakePayload(GameScene.OverworldScene, out BattlePayload payload);
            var ct = new CancellationToken();
            _loggerService.Log("BattleSceneBootstrapper started");
            _battleService.RunBattleAsync(payload.RoomId, ct);
            _loggerService.Log("BattleSceneBootstrapper finished starting battle");
        }
    }
}