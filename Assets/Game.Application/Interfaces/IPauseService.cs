using System;

namespace Game.Application.Interfaces
{
    public interface IPauseService
    {
        bool IsPaused { get; }
        event Action<bool> OnPauseStateChanged;
        void Pause();
        void Resume();
        void TogglePause();
    }
}