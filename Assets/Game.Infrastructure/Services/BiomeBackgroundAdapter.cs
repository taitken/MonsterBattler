using Game.Application.Interfaces;
using Game.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Infrastructure.Services
{
    public class BiomeBackgroundAdapter : IBiomeBackgroundProvider
    {
        private readonly Dictionary<Biome, AsyncOperationHandle> _loadedBackgrounds = new();

        public async Task<T> GetBackgroundAsync<T>(Biome biome) where T : class
        {
            // Check if already loaded
            if (_loadedBackgrounds.TryGetValue(biome, out var existingHandle) && existingHandle.IsValid())
            {
                return existingHandle.Result as T;
            }

            // Load from Addressables
            var address = $"Backgrounds/{biome}";
            var handle = Addressables.LoadAssetAsync<Sprite>(address);
            
            // Store handle for later release
            _loadedBackgrounds[biome] = handle;
            
            try
            {
                var sprite = await handle.Task;
                return sprite as T;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load background for biome {biome} at address '{address}': {ex.Message}");
                
                // Clean up failed handle
                if (_loadedBackgrounds.ContainsKey(biome))
                {
                    _loadedBackgrounds.Remove(biome);
                }
                
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                
                throw;
            }
        }

        public void ReleaseBackground(Biome biome)
        {
            if (_loadedBackgrounds.TryGetValue(biome, out var handle))
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                _loadedBackgrounds.Remove(biome);
            }
        }

        public void ReleaseAllBackgrounds()
        {
            foreach (var kvp in _loadedBackgrounds)
            {
                if (kvp.Value.IsValid())
                {
                    Addressables.Release(kvp.Value);
                }
            }
            _loadedBackgrounds.Clear();
        }
    }
}