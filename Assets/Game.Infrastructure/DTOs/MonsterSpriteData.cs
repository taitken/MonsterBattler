using Game.Domain.Enums;
using UnityEngine;

namespace Game.Infrastructure.DTOs
{
    [System.Serializable]
    public class MonsterSpriteData
    {
        public MonsterType type;
        public Sprite sprite;
    }
}
