using Game.Application.Interfaces;
using Game.Domain.Enums;
using Game.Infrastructure.ScriptableObjects;
using System.Threading.Tasks;

namespace Game.Infrastructure.Services
{
    public class MonsterSpriteAdapter : IMonsterSpriteProvider
    {
        private readonly MonsterDatabase _database;

        public MonsterSpriteAdapter(MonsterDatabase database)
        {
            _database = database;
        }

        public Task<T> GetMonsterSpriteAsync<T>(MonsterType type) where T : class
        {
            var sprite = _database.GetMonsterSprite(type);
            return Task.FromResult(sprite as T);
        }
    }
}