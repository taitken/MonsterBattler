using UnityEngine;
using System.Collections;
using TMPro;
namespace Game.Presentation.UI.Combat
{
    public class CombatTextUi : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float duration = .5f;
        [SerializeField] private float arcHeight = 150f;
        [SerializeField] private float horizontalDistance = 120f;
        [SerializeField] private float maxRotation = 30f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1.2f, 1f, 0.8f);

        public void Show(string combatText, Color color)
        {
            text.text = combatText;
            text.color = color;
            StartCoroutine(FloatingArcAnimation());
        }

        private IEnumerator FloatingArcAnimation()
        {
            Vector3 startPos = transform.position;
            var cg = GetComponent<CanvasGroup>();
            var originalScale = transform.localScale;
            
            // Determine random arc direction (left or right)
            float direction = Random.value > 0.5f ? 1f : -1f;
            Vector3 endPos = startPos + new Vector3(horizontalDistance * direction, arcHeight * 0.3f, 0);
            
            // Calculate arc peak position
            Vector3 peakPos = startPos + new Vector3((horizontalDistance * direction) * 0.5f, arcHeight, 0);
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                
                // Calculate parabolic arc position using quadratic Bezier curve
                Vector3 currentPos = CalculateParabolicPosition(startPos, peakPos, endPos, t);
                transform.position = currentPos;
                
                // Apply rotation during flight
                float rotationT = Mathf.Sin(t * Mathf.PI); // Peak rotation at middle of arc
                float currentRotation = maxRotation * rotationT * direction;
                transform.rotation = Quaternion.Euler(0, 0, currentRotation);
                
                // Apply scale curve for dynamic sizing
                float currentScale = scaleCurve.Evaluate(t);
                transform.localScale = originalScale * currentScale;
                
                // Fade out towards the end
                float fadeStartT = 0.6f;
                if (t > fadeStartT)
                {
                    float fadeT = (t - fadeStartT) / (1f - fadeStartT);
                    cg.alpha = Mathf.Lerp(1f, 0f, fadeT);
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
        
        private Vector3 CalculateParabolicPosition(Vector3 start, Vector3 peak, Vector3 end, float t)
        {
            // Quadratic Bezier curve: B(t) = (1-t)²P₀ + 2(1-t)tP₁ + t²P₂
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * start + 
                   2f * oneMinusT * t * peak + 
                   t * t * end;
        }
    }
}