using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Game.Application.Interfaces;

namespace Game.Infrastructure.Services
{
    public class SpriteCache : ISpriteCache
    {
        private readonly Dictionary<string, Sprite> _cache = new();
        private readonly Dictionary<string, Task<Sprite>> _loadingTasks = new();

        public async Task<Sprite> GetSpriteAsync<TKey>(TKey key, Func<TKey, string> addressProvider)
        {
            var address = addressProvider(key);
            return await GetSpriteAsync(address);
        }


        public async Task<Sprite> GetSpriteAsync(string address)
        {
            if (_cache.TryGetValue(address, out var cachedSprite))
                return cachedSprite;

            if (_loadingTasks.TryGetValue(address, out var loadingTask))
                return await loadingTask;

            var task = LoadSpriteAsync(address);
            _loadingTasks[address] = task;

            try
            {
                var sprite = await task;
                if (sprite != null)
                {
                    _cache[address] = sprite;
                }
                return sprite;
            }
            finally
            {
                _loadingTasks.Remove(address);
            }
        }

        private async Task<Sprite> LoadSpriteAsync(string address)
        {
            try
            {
                var handle = Addressables.LoadAssetAsync<Sprite>(address);
                var sprite = await handle.Task;
                
                if (sprite == null)
                {
                    Debug.LogWarning($"Failed to load sprite at address: {address}");
                    return null;
                }
                
                return sprite;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading sprite at address '{address}': {ex.Message}");
                return null;
            }
        }

        public async void PreloadSprites<TKey>(IEnumerable<TKey> keys, Func<TKey, string> addressProvider)
        {
            var loadTasks = new List<Task>();

            foreach (var key in keys)
            {
                loadTasks.Add(GetSpriteAsync(key, addressProvider));
            }

            await Task.WhenAll(loadTasks);
            Debug.Log($"Preloaded {_cache.Count} sprites");
        }

        public bool IsCached<TKey>(TKey key, Func<TKey, string> addressProvider)
        {
            var address = addressProvider(key);
            return _cache.ContainsKey(address);
        }

        public void ClearCache()
        {
            foreach (var sprite in _cache.Values)
            {
                if (sprite != null)
                {
                    Addressables.Release(sprite);
                }
            }
            _cache.Clear();
            _loadingTasks.Clear();
        }
    }
}