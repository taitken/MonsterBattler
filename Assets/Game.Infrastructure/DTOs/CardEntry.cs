
using Game.Infrastructure.ScriptableObjects;
using UnityEngine;

namespace Game.Infrastructure.DTOs
{
    [System.Serializable]
    public struct CardEntry
    {
        public AbilityCardData cardData;
        [Range(1, 10)]
        public int quantity;
    }
}
