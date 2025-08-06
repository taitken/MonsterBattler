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
    public class BattleSceneBootstrapper : MonoBehaviour
    {
        private ILoggerService _loggerService;
        private IBattleService _battleService;
        void Awake()
        {
            _loggerService = ServiceLocator.Get<ILoggerService>();
            _battleService = ServiceLocator.Get<IBattleService>();
        }
        void Start()
        {
            _loggerService.Log("BattleSceneBootstrapper started");
            _battleService.RunBattleAsync();
            _loggerService.Log("BattleSceneBootstrapper finished starting battle");
        }
    }
}