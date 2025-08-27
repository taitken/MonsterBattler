using System;
using Game.Application.Interfaces;
using Game.Core.Logger;
using UnityEngine;

namespace Game.Infrastructure.Services
{
    public class PauseService : IPauseService
    {
        private readonly ILoggerService _logger;
        private bool _isPaused;
        private float _previousTimeScale = 1f;

        public bool IsPaused => _isPaused;
        public event Action<bool> OnPauseStateChanged;

        public PauseService(ILoggerService logger)
        {
            _logger = logger;
            _isPaused = false;
        }

        public void Pause()
        {
            if (_isPaused) return;

            _logger?.Log("Game paused");
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _isPaused = true;
            OnPauseStateChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (!_isPaused) return;

            _logger?.Log("Game resumed");
            Time.timeScale = _previousTimeScale;
            _isPaused = false;
            OnPauseStateChanged?.Invoke(false);
        }

        public void TogglePause()
        {
            if (_isPaused)
                Resume();
            else
                Pause();
        }
    }
}