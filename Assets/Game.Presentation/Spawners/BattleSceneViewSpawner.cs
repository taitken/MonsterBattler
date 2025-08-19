using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Game.Presentation.GameObjects;
using Game.Application.DTOs;
using Game.Application.Enums;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Application.Messaging.Events;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Application.Messaging.Events.Spawning;
using Game.Core;
using Game.Domain.Enums;
using Game.Presentation.GameObjects.Card;
using Game.Presentation.GameObjects.Factories;
using Game.Presentation.Services;
using Game.Presentation.Enums;
using UnityEngine;

namespace Game.Presentation.Spawners
{
    public class MonsterViewSpawner : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform enemySpawn;
        private IEventBus _eventQueueService;
        private IInteractionBarrier _waitBarrier;
        private IViewRegistryService _viewRegistry;
        private IDisposable _monsterEventSubscription;
        private IDisposable _battleStartedSubscription;
        private IDisposable _cardPlayedSubscription;
        private IMonsterViewFactory _factory;
        private ICardViewFactory _cardFactory;
        private List<MonsterView> playerMonsters = new();
        private List<MonsterView> enemyMonsters = new();
        private List<MonsterView> allMonsterViews = new(); // Track all views for cleanup
        private int _spawnedMonsterCount;


        void Awake()
        {
            _eventQueueService = ServiceLocator.Get<IEventBus>();
            _waitBarrier = ServiceLocator.Get<IInteractionBarrier>();
            _viewRegistry = ServiceLocator.Get<IViewRegistryService>();
            _factory = ServiceLocator.Get<IMonsterViewFactory>();
            _cardFactory = ServiceLocator.Get<ICardViewFactory>();
            _monsterEventSubscription = _eventQueueService.Subscribe<MonsterSpawnedEvent>(OnMonsterSpawned);
            _battleStartedSubscription = _eventQueueService.Subscribe<BattleEndedEvent>(OnBattleEnded);
            _cardPlayedSubscription = _eventQueueService.Subscribe<CardPlayedEvent>(OnCardPlayed);
        }

        private void OnBattleEnded(BattleEndedEvent evt)
        {
            _spawnedMonsterCount = 0;
            CleanupMonsterViews();
        }

        private void CleanupMonsterViews()
        {
            // Unregister all views from the registry
            foreach (var view in allMonsterViews)
            {
                if (view != null && view.model != null)
                {
                    _viewRegistry.Unregister(view.model.Id);
                    Debug.Log($"Unregistered {view.model.MonsterName} (ID: {view.model.Id}) from ViewRegistry");
                }
            }
            
            // Clear all lists
            allMonsterViews.Clear();
            playerMonsters.Clear();
            enemyMonsters.Clear();
        }

        private void OnMonsterSpawned(MonsterSpawnedEvent evt)
        {
            // Handle barrier synchronization first (count all spawn events, even for dead monsters)
            if (evt.SpawnCompletionToken.HasValue && evt.ExpectedTotalCount.HasValue)
            {
                _spawnedMonsterCount++;
                Debug.Log($"Processed {_spawnedMonsterCount}/{ evt.ExpectedTotalCount} monster spawn events");
            }

            // Don't create views for dead monsters
            if (evt.Monster.IsDead)
            {
                Debug.Log($"Skipping view creation for dead monster: {evt.Monster.MonsterName} (HP: {evt.Monster.CurrentHP})");
                
                // Check if all spawn events are processed (including skipped ones)
                if (evt.SpawnCompletionToken.HasValue && evt.ExpectedTotalCount.HasValue && 
                    _spawnedMonsterCount >= evt.ExpectedTotalCount)
                {
                    Debug.Log("All monster spawn events processed, signaling completion");
                    _waitBarrier.Signal(new BarrierKey((BarrierToken)evt.SpawnCompletionToken));
                }
                return;
            }

            Debug.Log($"Creating view for monster: {evt.Monster.Type} on team {evt.Team}");
            var spawnPosition = DetermineMonsterSpawnPoint(evt);
            var monsterView = _factory.Create(evt.Monster, evt.Team, spawnPosition);
            var team = GetMonstersByTeam(evt.Team);
            team.Add(monsterView);
            allMonsterViews.Add(monsterView);

            // Register the view immediately in the ViewRegistry
            _viewRegistry.Register(evt.Monster.Id, monsterView);
            Debug.Log($"Registered {evt.Monster.MonsterName} (ID: {evt.Monster.Id}) with ViewRegistry in spawner");

            // Check if all spawn events are processed
            if (evt.SpawnCompletionToken.HasValue && evt.ExpectedTotalCount.HasValue && 
                _spawnedMonsterCount >= evt.ExpectedTotalCount)
            {
                Debug.Log("All monster spawn events processed, signaling completion");
                _waitBarrier.Signal(new BarrierKey((BarrierToken)evt.SpawnCompletionToken));
            }
        }

        private Vector3 DetermineMonsterSpawnPoint(MonsterSpawnedEvent evt)
        {
            var defaultSpawnPosition = GetDefaultSpawnPositionByTeam(evt.Team);
            var existingMonsters = GetMonstersByTeam(evt.Team);
            switch (existingMonsters.Count)
            {
                case 0:
                    return defaultSpawnPosition;
                case 1:
                    return defaultSpawnPosition + new Vector3(-2.5f, -.5f, 0f);
                case 2:
                    return defaultSpawnPosition + new Vector3(2.4f, -.9f, 0f);
                default:
                    return defaultSpawnPosition;
            }
        }

        private List<MonsterView> GetMonstersByTeam(BattleTeam team)
        {
            return team == BattleTeam.Player ? playerMonsters : enemyMonsters;
        }

        private Vector3 GetDefaultSpawnPositionByTeam(BattleTeam team)
        {
            return team == BattleTeam.Player ? playerSpawn.position : enemySpawn.position;
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            // Find the caster's view to position the card above them
            _viewRegistry.TryGet(evt.Caster.Id, out MonsterView casterView);
            if (casterView == null)
            {
                Debug.LogWarning($"Could not find view for caster {evt.Caster.MonsterName}");
                return;
            }

            // Get deck icon position and final card position
            var deckIconPosition = casterView.DeckIconWorldPosition;
            var finalCardPosition = casterView.transform.position + new Vector3(0, 3.6f, 0);
            
            // Debug positions
            Debug.Log($"Monster position: {casterView.transform.position}");
            Debug.Log($"Calculated deck icon position: {deckIconPosition}");
            Debug.Log($"Final card position: {finalCardPosition}");
            
            // Spawn card at deck icon position (initially invisible/tiny)
            var cardView = _cardFactory.Create(evt.Card, deckIconPosition);
            
            // Debug actual card position after spawn
            Debug.Log($"Card actual position after spawn: {cardView.transform.position}");
            
            // Determine animation type and get target position if needed
            var animationType = CardAnimationTypeResolver.GetAnimationType(evt.Card);
            Vector3? targetPosition = null;
            
            if (animationType == CardAnimationType.Attack && evt.PrimaryTarget != null)
            {
                _viewRegistry.TryGet(evt.PrimaryTarget.Id, out MonsterView targetView);
                if (targetView != null)
                {
                    targetPosition = targetView.transform.position;
                }
            }
            
            // Start the stretch animation coroutine
            StartCoroutine(AnimateCardStretch(cardView, deckIconPosition, finalCardPosition, evt.AnimationToken, animationType, targetPosition));
        }

        private IEnumerator AnimateCardStretch(CardView cardView, Vector3 deckIconPosition, Vector3 finalPosition, BarrierToken animationToken, CardAnimationType animationType, Vector3? targetPosition)
        {
            if (cardView == null) yield break;

            var cardTransform = cardView.transform;
            
            // Debug the animation start
            Debug.Log($"AnimateCardStretch starting:");
            Debug.Log($"  Card current position: {cardTransform.position}");
            Debug.Log($"  Deck icon position (start): {deckIconPosition}");
            Debug.Log($"  Final position (end): {finalPosition}");
            
            // Capture the card's original/target scale (set on the prefab)
            var targetScale = cardTransform.localScale;
            
            // Phase 1: Reverse suction stretch animation (0.5 seconds)
            float stretchDuration = 0.5f;
            float elapsed = 0f;
            
            // Start tiny and stretched towards deck icon
            cardTransform.position = deckIconPosition;
            cardTransform.localScale = new Vector3(targetScale.x * 0.1f, targetScale.y * 2f, targetScale.z); // Thin and tall like being "sucked"
            
            while (elapsed < stretchDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / stretchDuration;
                
                // Use ease-out curve for natural reverse suction effect
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                
                // Position: stretch from deck icon to final position
                cardTransform.position = Vector3.Lerp(deckIconPosition, finalPosition, easedT);
                
                // Scale: start stretched thin, end at target scale
                float scaleX = Mathf.Lerp(targetScale.x * 0.1f, targetScale.x, easedT);
                float scaleY = Mathf.Lerp(targetScale.y * 2f, targetScale.y, easedT);
                cardTransform.localScale = new Vector3(scaleX, scaleY, targetScale.z);
                
                yield return null;
            }
            
            // Ensure final position and scale
            cardTransform.position = finalPosition;
            cardTransform.localScale = targetScale;
            
            // Phase 2: Continue with float animation, potentially with special effects
            yield return StartCoroutine(AnimateCardFloat(cardView, animationToken, animationType, targetPosition));
        }

        private IEnumerator AnimateCardFloat(CardView cardView, BarrierToken animationToken, CardAnimationType animationType, Vector3? targetPosition)
        {
            if (cardView == null) yield break;

            var cardTransform = cardView.transform;
            var startPosition = cardTransform.position;
            var originalRotation = cardTransform.rotation;
            var floatTarget = startPosition + new Vector3(0, 0.5f, 0);

            // Phase 1: Float and potentially perform special animation
            if (animationType == CardAnimationType.Attack && targetPosition.HasValue)
            {
                yield return StartCoroutine(AnimateAttack(cardView, startPosition, floatTarget, targetPosition.Value, animationToken));
            }
            else
            {
                // Default floating motion for 2 seconds
                float elapsed = 0f;
                while (elapsed < 2f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / 2f;
                    
                    // Gentle floating motion
                    float floatOffset = Mathf.Sin(t * 3.14159f * 2) * 0.2f;
                    cardTransform.position = Vector3.Lerp(startPosition, floatTarget, t * 0.5f) + new Vector3(0, floatOffset, 0);
                    
                    yield return null;
                }
                
                // Signal hit point for non-attack cards
                _waitBarrier.Signal(new BarrierKey(animationToken, (int)AttackPhase.Hit));
            }

            // Phase 2: Wait while damage/effects are resolved
            yield return new WaitForSeconds(1f);

            // Signal animation end
            _waitBarrier.Signal(new BarrierKey(animationToken, (int)AttackPhase.End));

            // Phase 3: Fade out and destroy the card
            if (cardView != null)
            {
                var canvasGroup = cardView.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = cardView.gameObject.AddComponent<CanvasGroup>();

                var elapsed = 0f;
                while (elapsed < 0.5f && cardView != null)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = 1f - (elapsed / 0.5f);
                    yield return null;
                }

                if (cardView != null)
                    Destroy(cardView.gameObject);
            }
        }

        private IEnumerator AnimateAttack(CardView cardView, Vector3 originalStartPosition, Vector3 floatTarget, Vector3 targetPosition, BarrierToken animationToken)
        {
            if (cardView == null) yield break;

            var cardTransform = cardView.transform;
            var originalRotation = cardTransform.rotation;
            
            // Calculate direction to target and determine if attacking left or right
            Vector3 attackDirection = (targetPosition - originalStartPosition).normalized;
            bool attackingLeft = attackDirection.x < 0;
            
            // Phase 1: Wind up (0.3 seconds) - tilt back
            float windUpDuration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < windUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / windUpDuration;
                
                // Gentle float motion
                float floatOffset = Mathf.Sin(t * 3.14159f) * 0.1f;
                cardTransform.position = Vector3.Lerp(originalStartPosition, floatTarget, t * 0.3f) + new Vector3(0, floatOffset, 0);
                
                // Wind up rotation - tilt back away from target
                float windUpRotation = Mathf.Lerp(0f, attackingLeft ? 20f : -20f, Mathf.Sin(t * 3.14159f * 0.5f));
                cardTransform.rotation = originalRotation * Quaternion.Euler(0, 0, windUpRotation);
                
                yield return null;
            }
            
            // Phase 2: Strike forward (0.2 seconds) - quick rotation and slide toward enemy
            float strikeDuration = 0.2f;
            elapsed = 0f;
            Vector3 strikeStart = cardTransform.position;
            Vector3 strikeEnd = strikeStart + attackDirection * 0.5f; // Slide forward slightly
            
            while (elapsed < strikeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / strikeDuration;
                
                // Ease-out for snappy strike
                float easedT = 1f - Mathf.Pow(1f - t, 2f);
                
                // Strike position - slide toward target
                cardTransform.position = Vector3.Lerp(strikeStart, strikeEnd, easedT);
                
                // Strike rotation - swing toward target
                float startRotation = attackingLeft ? 20f : -20f;
                float endRotation = attackingLeft ? -25f : 25f;
                float strikeRotation = Mathf.Lerp(startRotation, endRotation, easedT);
                cardTransform.rotation = originalRotation * Quaternion.Euler(0, 0, strikeRotation);
                
                yield return null;
            }
            
            // Signal hit point - damage should be resolved now
            _waitBarrier.Signal(new BarrierKey(animationToken, (int)AttackPhase.Hit));
            
            // Phase 3: Return to position (0.4 seconds) - settle back to baseline position
            float returnDuration = 0.4f;
            elapsed = 0f;
            Vector3 returnStart = cardTransform.position;
            
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / returnDuration;
                
                // Ease-in for smooth return
                float easedT = Mathf.Pow(t, 0.5f);
                
                // Return position - dampen the bounce over time to settle at baseline (originalStartPosition)
                float bounceIntensity = Mathf.Lerp(0.2f, 0f, easedT); // Fade out bounce
                float floatOffset = Mathf.Sin(t * 3.14159f * 3) * bounceIntensity; // More bounces that fade out
                cardTransform.position = Vector3.Lerp(returnStart, originalStartPosition, easedT) + new Vector3(0, floatOffset, 0);
                
                // Return rotation - gradually return to original rotation
                cardTransform.rotation = Quaternion.Lerp(cardTransform.rotation, originalRotation, easedT);
                
                yield return null;
            }
            
            // Ensure final state - card should be at original position
            cardTransform.position = originalStartPosition;
            cardTransform.rotation = originalRotation;
        }

        void OnDestroy()
        {
            CleanupMonsterViews();
            _monsterEventSubscription?.Dispose();
            _battleStartedSubscription?.Dispose();
            _cardPlayedSubscription?.Dispose();
        }
    }
}
