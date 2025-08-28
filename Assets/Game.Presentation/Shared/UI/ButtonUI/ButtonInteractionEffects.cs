using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Presentation.UI.ButtonUI
{
    /// <summary>
    /// Handles hover and click effects for ButtonUI
    /// </summary>
    public class ButtonInteractionEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Hover Effects")]
        public Color hoverColor = new Color(0.6f, 0.2f, 0.2f, 1f);
        [Range(1.0f, 1.3f)]
        public float hoverScale = 1.1f;
        [Range(0.05f, 0.5f)]
        public float hoverTransitionSpeed = 0.1f;

        private Image _backgroundImage;
        private Color _originalColor;
        private Vector3 _originalScale;
        private Coroutine _hoverTransition;

        public void Initialize(Image backgroundImage)
        {
            _backgroundImage = backgroundImage;
            if (_backgroundImage != null)
            {
                _originalColor = _backgroundImage.color;
                _originalScale = transform.localScale;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_backgroundImage == null) return;
            
            if (_hoverTransition != null)
                StopCoroutine(_hoverTransition);
                
            _hoverTransition = StartCoroutine(TransitionToHover(true));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_backgroundImage == null) return;
            
            if (_hoverTransition != null)
                StopCoroutine(_hoverTransition);
                
            _hoverTransition = StartCoroutine(TransitionToHover(false));
        }

        public void PlayClickEffect()
        {
            if (_backgroundImage != null)
            {
                StopAllCoroutines();
                StartCoroutine(FlashColor(_backgroundImage, hoverColor, 0.05f));
            }
        }

        private IEnumerator TransitionToHover(bool isHovering)
        {
            Color targetColor = isHovering ? hoverColor : _originalColor;
            Vector3 targetScale = isHovering ? _originalScale * hoverScale : _originalScale;
            
            Color startColor = _backgroundImage.color;
            Vector3 startScale = transform.localScale;
            
            float elapsed = 0f;
            
            while (elapsed < hoverTransitionSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / hoverTransitionSpeed;
                t = Mathf.SmoothStep(0f, 1f, t);
                
                _backgroundImage.color = Color.Lerp(startColor, targetColor, t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                
                yield return null;
            }
            
            _backgroundImage.color = targetColor;
            transform.localScale = targetScale;
            _hoverTransition = null;
        }

        private IEnumerator FlashColor(Image img, Color flashColor, float duration)
        {
            var original = img.color;
            img.color = flashColor;
            yield return new WaitForSeconds(duration);
            img.color = original;
        }
    }
}