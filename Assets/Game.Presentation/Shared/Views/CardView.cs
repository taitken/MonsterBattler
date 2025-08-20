using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Domain.Entities.Abilities;
using Game.Application.Interfaces;
using Game.Core;
using System.Threading.Tasks;

namespace Game.Presentation.Shared.Views
{
    public class CardView : MonoObject<AbilityCard>
    {
        [SerializeField] private TextMeshProUGUI _cardTitle;
        [SerializeField] private TextMeshProUGUI _bodyText;
        [SerializeField] private Image _cardArt;
        private ICardArtProvider _spriteProvider;

        void Awake()
        {
            _spriteProvider = ServiceLocator.Get<ICardArtProvider>();
        }

        protected override async void OnModelBound()
        {
            if (model == null)
            {
                throw new System.InvalidOperationException("Cannot bind null model to CardView");
            }

            await UpdateCardVisuals();
        }

        private async Task UpdateCardVisuals()
        {
            if (_cardTitle != null)
                _cardTitle.text = model.Name;

            if (_bodyText != null)
                _bodyText.text = model.GeneratedDescription;

            if (_cardArt != null)
                _cardArt.sprite = await _spriteProvider.GetCardArtAsync<Sprite>(model.Name);
        }

        private void OnDestroy()
        {
            if (_viewRegistry != null && model != null)
                _viewRegistry.Unregister(model.Id);
        }
    }
}
