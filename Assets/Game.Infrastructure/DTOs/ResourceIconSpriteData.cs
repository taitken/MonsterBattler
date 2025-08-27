using Game.Domain.Enums;
using UnityEngine;

namespace Game.Infrastructure.DTOs
{
    [System.Serializable]
    public class ResourceIconSpriteData
    {
        public ResourceType type;
        public Sprite sprite;
    }
}
