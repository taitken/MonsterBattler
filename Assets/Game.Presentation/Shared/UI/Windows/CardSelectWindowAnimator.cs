using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Presentation.Shared.Views;

namespace Game.Presentation.Shared.UI.Windows
{
    public class CardSelectWindowAnimator : MonoBehaviour
    {
        private const float CARD_SELECTION_DURATION = 0.6f;
        private const float CARD_FADE_DURATION = 0.3f;
        private const float MONSTER_SELECTION_DURATION = 0.5f;
        
        public IEnumerator AnimateCardSelection(CardView selectedCard, RectTransform selectedCardPosition, List<CardView> allCards)
        {
            var selectedCardRect = selectedCard.GetComponent<RectTransform>();
            if (selectedCardRect == null) yield break;
            
            var originalPosition = selectedCardRect.anchoredPosition;
            var targetPosition = selectedCardPosition.anchoredPosition;
            var originalScale = selectedCardRect.localScale;
            
            var fadeCoroutines = new List<Coroutine>();
            foreach (var cardView in allCards)
            {
                if (cardView != selectedCard)
                {
                    fadeCoroutines.Add(StartCoroutine(FadeOutCard(cardView, CARD_FADE_DURATION)));
                }
            }
            
            float elapsed = 0f;
            while (elapsed < CARD_SELECTION_DURATION)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / CARD_SELECTION_DURATION;
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                
                selectedCardRect.anchoredPosition = Vector2.Lerp(originalPosition, targetPosition, easedT);
                
                yield return null;
            }
            
            selectedCardRect.anchoredPosition = targetPosition;
            
            foreach (var coroutine in fadeCoroutines)
            {
                yield return coroutine;
            }
        }
        
        private IEnumerator FadeOutCard(CardView cardView, float duration)
        {
            if (cardView == null) yield break;
            
            var canvasGroup = cardView.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = cardView.gameObject.AddComponent<CanvasGroup>();
            
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            cardView.gameObject.SetActive(false);
        }
        
        public IEnumerator AnimateMonsterSelection(Image selectedMonster, RectTransform topRightPosition, List<Image> allMonsters)
        {
            var selectedMonsterRect = selectedMonster.GetComponent<RectTransform>();
            if (selectedMonsterRect == null) yield break;
            
            var originalPosition = selectedMonsterRect.anchoredPosition;
            var targetPosition = topRightPosition.anchoredPosition;
            var originalScale = selectedMonsterRect.localScale;
            var targetScale = originalScale * 0.8f; // Make it slightly smaller in top right
            
            // Fade out other monsters
            var fadeCoroutines = new List<Coroutine>();
            foreach (var monster in allMonsters)
            {
                if (monster != selectedMonster && monster.gameObject.activeInHierarchy)
                {
                    fadeCoroutines.Add(StartCoroutine(FadeOutMonster(monster, CARD_FADE_DURATION)));
                }
            }
            
            // Animate selected monster to top right
            float elapsed = 0f;
            while (elapsed < MONSTER_SELECTION_DURATION)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / MONSTER_SELECTION_DURATION;
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                
                selectedMonsterRect.anchoredPosition = Vector2.Lerp(originalPosition, targetPosition, easedT);
                selectedMonsterRect.localScale = Vector3.Lerp(originalScale, targetScale, easedT);
                
                yield return null;
            }
            
            selectedMonsterRect.anchoredPosition = targetPosition;
            selectedMonsterRect.localScale = targetScale;
            
            // Wait for fade animations to complete
            foreach (var coroutine in fadeCoroutines)
            {
                yield return coroutine;
            }
        }
        
        private IEnumerator FadeOutMonster(Image monster, float duration)
        {
            if (monster == null) yield break;
            
            var canvasGroup = monster.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = monster.gameObject.AddComponent<CanvasGroup>();
            
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            monster.gameObject.SetActive(false);
        }
    }
}