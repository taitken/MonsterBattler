using Game.Domain.Enums;
using UnityEngine;

namespace Game.Infrastructure.DTOs
{
    [System.Serializable]
    public class RuneIconSpriteData
    {
        public RuneType type;
        public Sprite sprite;
        public Color glowColor = Color.red;
    }
}