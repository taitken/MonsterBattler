namespace Game.Presentation.VfxControllers
{
    using System;
    using Assets.Game.Presentation.UI.TitleUI;
    using Assets.Game.Presentation.UiObjects;
    using Game.Application.DTOs;
    using Game.Application.Interfaces;
    using Game.Application.Messaging;
    using Game.Application.Messaging.Events.BattleFlow;
    using Game.Core;
    using Game.Presentation.Core.Interfaces;
    using Game.Presentation.Shared.Views;
    using UnityEngine;

    public sealed class BattleVfxController : MonoBehaviour
    {
        private RectTransform _rootCanvas;
        private IEventBus _bus;
        private IViewRegistryService _viewRegistry;
        private ICombatTextFactory _combatTextFactory;
        private IInteractionBarrier _waitBarrier;
        private IDisposable _subActionSelected;
        private IDisposable _subDamageApplied;
        private IDisposable _subEffectApplied;
        private IDisposable _subFainted;
        private IDisposable _subTurnStart;
        private IDisposable _subTurnEnd;
        private IDisposable _subBattleEnded;
        private string DAMAGE_COLOUR = "#FF4444";
        private string BLOCK_COLOUR = "#4488FF";

        void Awake()
        {
            _bus = ServiceLocator.Get<IEventBus>();
            _viewRegistry = ServiceLocator.Get<IViewRegistryService>();
            _combatTextFactory = ServiceLocator.Get<ICombatTextFactory>();
            _waitBarrier = ServiceLocator.Get<IInteractionBarrier>();
            _rootCanvas = GetComponentInParent<RectTransform>();
        }

        void OnEnable()
        {
            _subActionSelected = _bus.Subscribe<ActionSelectedEvent>(OnActionSelected);
            _subDamageApplied = _bus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            _subEffectApplied = _bus.Subscribe<EffectAppliedEvent>(OnEffectApplied);
            _subFainted = _bus.Subscribe<MonsterFaintedEvent>(OnMonsterFainted);
            _subTurnStart = _bus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            _subTurnEnd = _bus.Subscribe<TurnEndedEvent>(OnTurnEnded);
        }

        private void OnActionSelected(ActionSelectedEvent e)
        {
            Debug.Log($"ActionSelected: {e.Attacker.MonsterName} attacking {e.Target.MonsterName} with token {e.Token}");
            if (!_viewRegistry.TryGet(e.Attacker.Id, out MonsterView attackerView))
            {
                Debug.LogError($"MonsterView not found for attacker {e.Attacker.MonsterName} (ID: {e.Attacker.Id})");
                return;
            }

            Debug.Log($"Found attacker view, calling PlayAttackAnimation");
            attackerView.PlayAttackAnimation(e.Token);
        }

        private void OnDamageApplied(DamageAppliedEvent e)
        {
            if (_viewRegistry.TryGet(e.Target.Id, out MonsterView targetView))
            {
                Vector3 basePosition = targetView.transform.position + new Vector3(0, 1f, 0);
                
                if (e.Amount > 0)
                {
                    ColorUtility.TryParseHtmlString(DAMAGE_COLOUR, out Color redDamageColor);
                    _combatTextFactory.Create(redDamageColor, $"{e.Amount}", _rootCanvas, basePosition);
                }
                
                if (e.AmountBlocked > 0)
                {
                    ColorUtility.TryParseHtmlString(BLOCK_COLOUR, out Color blueDamageColor);
                    Vector3 blockPosition = basePosition + new Vector3(0.5f, 0.3f, 0);
                    _combatTextFactory.Create(blueDamageColor, $"{e.AmountBlocked}", _rootCanvas, blockPosition);
                }
                
                int totalDamage = e.Amount + e.AmountBlocked;
                if (totalDamage > 0)
                {
                    targetView.PlayHitReaction(totalDamage);
                }
                
                if (e.WaitToken.HasValue)
                {
                    _waitBarrier.SignalAfterDelay(new BarrierKey(e.WaitToken.Value), 0.3f);
                }
            }
        }

        private void OnEffectApplied(EffectAppliedEvent e)
        {
            // Signal completion after delay if there's a wait token
            if (e.WaitToken.HasValue)
            {
                _waitBarrier.SignalAfterDelay(new BarrierKey(e.WaitToken.Value), 0.1f);
            }
            
            // TODO: Add visual effects for different effect types (shield icons, etc.)
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
            _subActionSelected?.Dispose();
            _subDamageApplied?.Dispose();
            _subEffectApplied?.Dispose();
            _subFainted?.Dispose();
            _subTurnStart?.Dispose();
            _subTurnEnd?.Dispose();
            _subBattleEnded?.Dispose();
        }
    }
}