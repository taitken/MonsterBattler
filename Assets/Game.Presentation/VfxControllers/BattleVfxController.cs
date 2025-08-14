namespace Game.Presentation.VfxControllers
{
    using System;
    using Assets.Game.Presentation.GameObjects;
    using Assets.Game.Presentation.UiObjects;
    using Game.Application.Messaging;
    using Game.Application.Messaging.Events.BattleFlow;
    using Game.Core;
    using Game.Presentation.Services;
    using UnityEngine;

    public sealed class BattleVfxController : MonoBehaviour
    {
        private IEventBus _bus;
        private IViewRegistryService _viewRegistry;
        private ICombatTextFactory _combatTextFactory;
        private IDisposable _subActionSelected;
        private IDisposable _subDamageApplied;
        private IDisposable _subFainted;
        private IDisposable _subTurnStart;
        private IDisposable _subTurnEnd;
        private string DAMAGE_COLOUR = "#B22222";

        void Awake()
        {
            _bus = ServiceLocator.Get<IEventBus>();
            _viewRegistry = ServiceLocator.Get<IViewRegistryService>();
            _combatTextFactory = ServiceLocator.Get<ICombatTextFactory>();
        }

        void OnEnable()
        {
            _subActionSelected = _bus.Subscribe<ActionSelectedEvent>(OnActionSelected);
            _subDamageApplied = _bus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            _subFainted = _bus.Subscribe<MonsterFaintedEvent>(OnMonsterFainted);
            _subTurnStart = _bus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            _subTurnEnd = _bus.Subscribe<TurnEndedEvent>(OnTurnEnded);
        }

        private void OnActionSelected(ActionSelectedEvent e)
        {
            if (!_viewRegistry.TryGet(e.Attacker.Id, out MonsterView attackerView))
                return;

            attackerView.PlayAttackAnimation(e.Token);
        }

        private void OnDamageApplied(DamageAppliedEvent e)
        {
            if (_viewRegistry.TryGet(e.Target.Id, out MonsterView targetView))
            {
                ColorUtility.TryParseHtmlString(DAMAGE_COLOUR, out Color redDamageColor);
                _combatTextFactory.Create(redDamageColor, $"{e.Amount}", targetView.transform.position + new Vector3(0, 1f, 0));
                targetView.PlayHitReaction(e.Amount);
            }
        }

        private void OnMonsterFainted(MonsterFaintedEvent e)
        {
            if (_viewRegistry.TryGet(e.Monster.Id, out MonsterView view))
            {
                view.PlayDie();
            }
        }

        private void OnTurnStarted(TurnStartedEvent e)
        {
            // Optional: tint team, show TURN banner, play whoosh SFX, etc.
            // UiTurnBanner.Show(e.Team);
        }

        private void OnTurnEnded(TurnEndedEvent e)
        {
            // Optional: clear tints/banners
            // UiTurnBanner.Hide();
        }

        void OnDisable()
        {
            _subActionSelected?.Dispose(); _subActionSelected = null;
            _subDamageApplied?.Dispose(); _subDamageApplied = null;
            _subFainted?.Dispose(); _subFainted = null;
            _subTurnStart?.Dispose(); _subTurnStart = null;
            _subTurnEnd?.Dispose(); _subTurnEnd = null;
        }
    }
}