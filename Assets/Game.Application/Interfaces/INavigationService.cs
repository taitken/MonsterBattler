using Game.Domain.Enums;

namespace Game.Application.Interfaces
{
    /// <summary>
    /// Interface for navigation service to handle scene transitions and payloads.
    /// </summary>
    public interface INavigationService
    {
        void SetPayload<T>(GameScene scene, T payload);
        bool TryTakePayload<T>(GameScene scene, out T payload);
    }
}