using Game.Application.Interfaces;
using Game.Domain.Enums;
using Game.Infrastructure.ScriptableObjects;
using UnityEngine;

namespace Game.Infrastructure.Services
{
    public class StatusEffectIconProvider : IStatusEffectIconProvider
    {
        private readonly StatusEffectIconDatabase _database;

        public StatusEffectIconProvider(StatusEffectIconDatabase database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
        }

        public Sprite GetEffectSprite(EffectType effectType)
        {
            return _database.GetEffectSprite(effectType);
        }

        public bool HasSprite(EffectType effectType)
        {
            return _database.HasSprite(effectType);
        }
    }
}