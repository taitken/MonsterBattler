using Game.Application.Interfaces;
using Game.Core;
using Game.Core.Logger;
using UnityEngine;

namespace Game.Presentation.Scenes.Battle.Controllers
{
    /// <summary>
    /// Handles UI visibility during pause states in battle scene
    /// </summary>
    public class BattlePauseUIController : MonoBehaviour
    {
        private IPauseService _pauseService;
        private ILoggerService _loggerService;

        /// <summary>
        /// Initialize the controller with a specific rewards window
        /// </summary>
        public void Initialize(RewardsWindow rewardsWindow)
        {

        }

        void Start()
        {
            _pauseService = ServiceLocator.Get<IPauseService>();
            _loggerService = ServiceLocator.Get<ILoggerService>();

            // Subscribe to pause state changes
            _pauseService.OnPauseStateChanged += OnPauseStateChanged;
        }

        private void OnPauseStateChanged(bool isPaused)
        {
            _loggerService?.Log($"Rewards window {(isPaused ? "shown" : "hidden")} due to pause state change");
        }

        void OnDestroy()
        {
            // Unsubscribe from pause events
            if (_pauseService != null)
                _pauseService.OnPauseStateChanged -= OnPauseStateChanged;
        }
    }
}