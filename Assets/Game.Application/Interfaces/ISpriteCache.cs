using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Application.Interfaces
{
    public interface ISpriteCache
    {
        Task<Sprite> GetSpriteAsync<TKey>(TKey key, Func<TKey, string> addressProvider);
        Task<Sprite> GetSpriteAsync(string address);
        void PreloadSprites<TKey>(IEnumerable<TKey> keys, Func<TKey, string> addressProvider);
        void ClearCache();
        bool IsCached<TKey>(TKey key, Func<TKey, string> addressProvider);
    }
}