using System.Threading.Tasks;

namespace Game.Application.Interfaces
{
    public interface ICardArtProvider
    {
        Task<T> GetCardArtAsync<T>(string cardName) where T : class;
    }
}