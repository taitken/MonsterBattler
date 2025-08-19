using Game.Application.Interfaces;
using Game.Infrastructure.ScriptableObjects;
using System.Threading.Tasks;

namespace Game.Infrastructure.Services
{
    public class CardArtAdapter : ICardArtProvider
    {
        private readonly AbilityCardDatabase _database;

        public CardArtAdapter(AbilityCardDatabase database)
        {
            _database = database;
        }

        public Task<T> GetCardArtAsync<T>(string cardName) where T : class
        {
            var cardData = _database.GetCard(cardName);
            var cardArt = cardData?.cardArt;
            return Task.FromResult(cardArt as T);
        }
    }
}