using Game.Application.Interfaces;
using Game.Domain.Enums;
using Game.Infrastructure.ScriptableObjects;
using UnityEngine;

namespace Game.Infrastructure.Providers
{
    public class RuneIconProvider : IRuneIconProvider
    {
        private readonly RuneIconDatabase _database;

        public RuneIconProvider(RuneIconDatabase database)
        {
            _database = database;
        }

        public Sprite GetRuneSprite(RuneType runeType)
        {
            return _database.GetRuneSprite(runeType);
        }
    }
}