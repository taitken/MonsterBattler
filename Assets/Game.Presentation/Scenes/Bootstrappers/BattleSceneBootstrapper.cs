using System.Threading;
using Game.Application.DTOs;
using Game.Application.Interfaces;
using Game.Application.Repositories;
using Game.Core;
using Game.Core.Logger;
using Game.Domain.Enums;
using Game.Presentation.UI;
using Game.Presentation.Scenes.Battle.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game.Presentation.Scenes
{
    /// <summary>
    /// Bootstrapper for the Battle Scene.
    /// Initializes services and starts the battle.
    /// </summary>
    public class BattleSceneBootstrapper : MonoBehaviour
    {
        [SerializeField] private BackgroundUI _backgroundImage;
        private ILoggerService _loggerService;
        private IBattleService _battleService;
        private INavigationService _navigationService;
        private IBiomeBackgroundProvider _backgroundProvider;
        private IOverworldRepository _overworldService;
        void Awake()
        {
            _loggerService = ServiceLocator.Get<ILoggerService>();
            _battleService = ServiceLocator.Get<IBattleService>();
            _navigationService = ServiceLocator.Get<INavigationService>();
            _backgroundProvider = ServiceLocator.Get<IBiomeBackgroundProvider>();
            _overworldService = ServiceLocator.Get<IOverworldRepository>();
            
            // Add controllers for pause functionality
            gameObject.AddComponent<BattleInputController>();
        }
        async void Start()
        {
            try
            {
                _loggerService.Log("BattleSceneBootstrapper started");
                
                if (!_navigationService.TryTakePayload(GameScene.BattleScene, out BattlePayload payload))
                {
                    _loggerService.LogError("No battle payload found - cannot start battle");
                    return;
                }
                
                if (payload.RoomId == System.Guid.Empty)
                {
                    _loggerService.LogError("Invalid room ID in battle payload - cannot start battle");
                    return;
                }
                
                _loggerService.Log($"Starting battle for room: {payload.RoomId}");
                
                // Load background based on room's biome
                await LoadBackgroundForRoom(payload.RoomId);
                
                var ct = new CancellationToken();
                await _battleService.RunBattleAsync(payload.RoomId, ct);
                _loggerService.Log("BattleSceneBootstrapper finished battle");
            }
            catch (System.Exception ex)
            {
                _loggerService.LogError($"Battle failed with exception: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadBackgroundForRoom(System.Guid roomId)
        {
            try
            {
                var room = _overworldService.GetRoomById(roomId);
                if (room == null)
                {
                    _loggerService.LogError($"Room with ID {roomId} not found - using default biome");
                    return;
                }

                _loggerService.Log($"Loading background for biome: {room.Biome}");
                var backgroundSprite = await _backgroundProvider.GetBackgroundAsync<Sprite>(room.Biome);
                
                if (backgroundSprite != null && _backgroundImage != null)
                {
                    _backgroundImage.SetImage(backgroundSprite);
                    _loggerService.Log($"Successfully loaded {room.Biome} background");
                }
                else
                {
                    _loggerService.LogError($"Failed to load background for biome {room.Biome}");
                }
            }
            catch (System.Exception ex)
            {
                _loggerService.LogError($"Error loading background: {ex.Message}");
            }
        }

        void OnDestroy()
        {
            // Clean up background when leaving battle scene
            _backgroundProvider?.ReleaseAllBackgrounds();
        }
    }
}