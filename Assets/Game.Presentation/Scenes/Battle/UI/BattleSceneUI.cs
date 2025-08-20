using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core;
using TMPro;
using UnityEngine;

namespace Assets.Game.Presentation.UI.BattleScene
{
    public class BattleSceneUI : MonoBehaviour
    {
        IEventBus _eventBus;
        [SerializeField] private TMP_Text victoryText;

        void Awake()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
        }

        private void OnBattleEnded(BattleEndedEvent @event)
        {
            victoryText.gameObject.SetActive(true);
        }

        void Start()
        {
            // Hide it at the start
            victoryText.gameObject.SetActive(false);
        }
    }
}
