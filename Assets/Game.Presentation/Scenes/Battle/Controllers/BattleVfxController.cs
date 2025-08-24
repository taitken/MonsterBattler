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
        private VictoryText _victoryText;
        private RectTransform _rootCanvas;
        private IEventBus _bus;
        private IViewRegistryService _viewRegistry;
        private ICombatTextFactory _combatTextFactory;
        private IInteractionBarrier _waitBarrier;
        private IDisposable _subActionSelected;
        private IDisposable _subDamageApplied;
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
            _rootCanvas = GetComponent<RectTransform>();
            _victoryText = GetComponentInChildren<VictoryText>();
            _victoryText.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            _subActionSelected = _bus.Subscribe<ActionSelectedEvent>(OnActionSelected);
            _subDamageApplied = _bus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            _subFainted = _bus.Subscribe<MonsterFaintedEvent>(OnMonsterFainted);
            _subTurnStart = _bus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            _subTurnEnd = _bus.Subscribe<TurnEndedEvent>(OnTurnEnded);
            _subBattleEnded = _bus.Subscribe<BattleEndedEvent>(OnBattleEnded);
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
                
                // Show damage text in red (if any damage was dealt)
                if (e.Amount > 0)
                {
                    ColorUtility.TryParseHtmlString(DAMAGE_COLOUR, out Color redDamageColor);
                    _combatTextFactory.Create(redDamageColor, $"{e.Amount}", _rootCanvas, basePosition);
                }
                
                // Show blocked amount in blue (if any damage was blocked)
                if (e.AmountBlocked > 0)
                {
                    ColorUtility.TryParseHtmlString(BLOCK_COLOUR, out Color blueDamageColor);
                    Vector3 blockPosition = basePosition + new Vector3(0.5f, 0.3f, 0); // Offset slightly
                    _combatTextFactory.Create(blueDamageColor, $"{e.AmountBlocked}", _rootCanvas, blockPosition);
                }
                
                // Play hit reaction based on total attempted damage
                int totalDamage = e.Amount + e.AmountBlocked;
                if (totalDamage > 0)
                {
                    targetView.PlayHitReaction(totalDamage);
                }
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

        private async void OnBattleEnded(BattleEndedEvent e)
        {
            _victoryText.gameObject.SetActive(true);
            var waitKey = BarrierToken.New();
            _victoryText.Play(waitKey);
            await _waitBarrier.WaitAsync(new BarrierKey(waitKey));
            _bus.Publish(new BattleFlowCompleteCommand(e.Result));
        }

        void OnDisable()
        {
            _subActionSelected?.Dispose();
            _subDamageApplied?.Dispose();
            _subFainted?.Dispose();
            _subTurnStart?.Dispose();
            _subTurnEnd?.Dispose();
            _subBattleEnded?.Dispose();
        }
    }
}