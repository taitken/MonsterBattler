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
        void Awake()
        {
            _loggerService = ServiceLocator.Get<ILoggerService>();
            _overworldService = ServiceLocator.Get<IOverworldService>();
        }
        void Start()
        {
            _loggerService.Log("OverworldSceneBootstrapper started");
            overworldView.Bind(_overworldService.InitializeOverworld());
            _loggerService.Log("OverworldSceneBootstrapper finished");
        }
    }
}