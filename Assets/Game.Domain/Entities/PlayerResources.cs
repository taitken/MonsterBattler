using System;
using System.Collections.Generic;
using Game.Domain.Enums;

namespace Game.Domain.Entities
{
    /// <summary>
    /// Domain entity representing player resources (gold, gems, etc.)
    /// </summary>
    public class PlayerResources : BaseEntity
    {
        private readonly Dictionary<ResourceType, int> _resources;

        public event Action<ResourceType, int> OnResourceChanged;

        public PlayerResources()
        {
            _resources = new Dictionary<ResourceType, int>
            {
                { ResourceType.Gold, 100 }, 
                { ResourceType.Experience, 0 },
                { ResourceType.Health, 100 },
                { ResourceType.Mana, 50 }
            };
        }

        public PlayerResources(Dictionary<ResourceType, int> initialResources)
        {
            _resources = new Dictionary<ResourceType, int>(initialResources);
        }

        /// <summary>
        /// Gets the current amount of a specific resource
        /// </summary>
        public int GetResource(ResourceType resourceType)
        {
            return _resources.TryGetValue(resourceType, out var amount) ? amount : 0;
        }

        /// <summary>
        /// Sets the amount of a specific resource
        /// </summary>
        public void SetResource(ResourceType resourceType, int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Resource amount cannot be negative", nameof(amount));

            var oldAmount = GetResource(resourceType);
            _resources[resourceType] = amount;
            
            if (oldAmount != amount)
                OnResourceChanged?.Invoke(resourceType, amount);
        }

        /// <summary>
        /// Adds to a specific resource
        /// </summary>
        public void AddResource(ResourceType resourceType, int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount to add must be positive", nameof(amount));

            var currentAmount = GetResource(resourceType);
            SetResource(resourceType, currentAmount + amount);
        }

        /// <summary>
        /// Removes from a specific resource
        /// </summary>
        /// <returns>True if successful, false if insufficient resources</returns>
        public bool RemoveResource(ResourceType resourceType, int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount to remove must be positive", nameof(amount));

            var currentAmount = GetResource(resourceType);
            if (currentAmount < amount)
                return false;

            SetResource(resourceType, currentAmount - amount);
            return true;
        }

        /// <summary>
        /// Checks if player has enough of a specific resource
        /// </summary>
        public bool HasEnough(ResourceType resourceType, int requiredAmount)
        {
            return GetResource(resourceType) >= requiredAmount;
        }

        /// <summary>
        /// Gets a copy of all current resources
        /// </summary>
        public Dictionary<ResourceType, int> GetAllResources()
        {
            return new Dictionary<ResourceType, int>(_resources);
        }

        // Convenience properties for common resources
        public int Gold => GetResource(ResourceType.Gold);
        public int Experience => GetResource(ResourceType.Experience);
        public int Health => GetResource(ResourceType.Health);
        public int Mana => GetResource(ResourceType.Mana);
    }
}