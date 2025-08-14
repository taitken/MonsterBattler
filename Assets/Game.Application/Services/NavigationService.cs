using System.Collections.Generic;
using Game.Application.Interfaces;
using Game.Domain.Enums;

public sealed class NavigationService : INavigationService
{
    private readonly Dictionary<GameScene, object> _payloads = new();

    public void SetPayload<T>(GameScene scene, T payload)
        => _payloads[scene] = payload;

    public bool TryTakePayload<T>(GameScene scene, out T payload)
    {
        if (_payloads.TryGetValue(scene, out var obj) && obj is T t)
        {
            _payloads.Remove(scene);
            payload = t;
            return true;
        }
        payload = default;
        return false;
    }
}