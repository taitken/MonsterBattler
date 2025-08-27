using Game.Domain.Enums;
using UnityEngine;

namespace Game.Application.Interfaces
{
    /// <summary>
    /// Provides access to resource icon sprites
    /// </summary>
    public interface IResourceIconProvider
    {
        /// <summary>
        /// Gets the sprite for a specific resource type
        /// </summary>
        /// <param name="resourceType">The type of resource</param>
        /// <returns>The sprite for the resource, or null if not found</returns>
        Sprite GetResourceSprite(ResourceType resourceType);
    }
}