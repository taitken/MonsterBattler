using Game.Application.Interfaces;
using Game.Domain.Enums;
using Game.Infrastructure.ScriptableObjects;
using UnityEngine;

namespace Game.Infrastructure.Providers
{
    /// <summary>
    /// Provides access to resource icon sprites from the ResourceIconDatabase
    /// </summary>
    public class ResourceIconProvider : IResourceIconProvider
    {
        private readonly ResourceIconDatabase _database;

        public ResourceIconProvider(ResourceIconDatabase database)
        {
            _database = database;
        }

        public Sprite GetResourceSprite(ResourceType resourceType)
        {
            if (_database == null)
            {
                Debug.LogError("ResourceIconDatabase is null");
                return null;
            }

            return _database.GetResourceSprite(resourceType);
        }
    }
}