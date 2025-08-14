using System.Collections;
using UnityEngine;
using TMPro;
using System.Threading;
using Game.Applcation.DTOs;
using Game.Application.Interfaces;
using Game.Core;

namespace Assets.Game.Presentation.UI.TitleUI
{
    /// <summary>
    /// Controls the victory text animation and effects when the player wins a battle.
    /// </summary>
    [RequireComponent(typeof(TMP_Text), typeof(CanvasGroup))]
    public class VictoryText : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private TMP_Text text;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private ParticleSystem confetti; // optional
        [SerializeField] private AudioSource sfx;         // optional

        [Header("Timings")]
        [SerializeField] private float popInDuration = 0.75f;
        [SerializeField] private float outlineFlashDuration = 0.35f;
        [SerializeField] private float punchDuration = 0.15f;
        [SerializeField] private int punches = 3;
        [SerializeField] private float idleWobbleSeconds = 2.0f; // set 0 to skip

        [Header("Scales")]
        [SerializeField] private float startScale = 0.2f;
        [SerializeField] private float overshootScale = 1.12f;
        [SerializeField] private float finalScale = 1.0f;
        [SerializeField] private float punchScale = 0.06f; // +/- scale per punch

        [Header("Idle")]
        [SerializeField] private float idleScaleAmp = 0.02f;
        [SerializeField] private float idleRotAmp = 3.0f;

        [Header("Exit")]
        [SerializeField] private bool autoFadeOut = false;
        [SerializeField] private float exitDelay = 0.75f;
        [SerializeField] private float exitFadeDuration = 0.4f;

        // Cached TMP material properties (for outline flash)
        private int ID_OutlineWidth;
        private int ID_FaceDilate;

        private Material _runtimeMat;
        private RectTransform _rt;
        private IInteractionBarrier _waitBarrier;

        void Reset()
        {
            text = GetComponent<TMP_Text>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        void Awake()
        {
            _waitBarrier = ServiceLocator.Get<IInteractionBarrier>();
            _rt = (RectTransform)transform;
            ID_FaceDilate = ShaderUtilities.ID_FaceDilate;
            ID_OutlineWidth = ShaderUtilities.ID_OutlineWidth;

            // Use a material instance so we don’t mutate the shared asset
            if (text != null)
                _runtimeMat = Instantiate(text.fontMaterial);

            if (text != null && _runtimeMat != null)
                text.fontMaterial = _runtimeMat;

            // Prepare hidden state
            _rt.localScale = Vector3.one * startScale;
            canvasGroup.alpha = 0f;
        }

        /// <summary>Call this when the battle ends (player victory).</summary>
        public void Play(BarrierToken token)
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(PlayRoutine(token));
        }

        private IEnumerator PlayRoutine(BarrierToken token)
        {
            // 1) POP-IN (fade + squash & stretch + overshoot bounce)
            yield return StartCoroutine(PopIn(popInDuration));

            // 2) OUTLINE FLASH (quick glow)
            if (_runtimeMat != null)
                yield return StartCoroutine(OutlineFlash(outlineFlashDuration));

            // 3) PUNCHES (bouncy emphasis)
            yield return StartCoroutine(Punch(punches, punchDuration, punchScale));

            // Optional: confetti + sfx
            if (confetti) confetti.Play();
            if (sfx) sfx.Play();

            // 4) IDLE WOBBLE (subtle)
            if (idleWobbleSeconds > 0f)
                yield return StartCoroutine(IdleWobble(idleWobbleSeconds));

            // Optional: auto exit
            if (autoFadeOut)
            {
                yield return new WaitForSeconds(exitDelay);
                yield return StartCoroutine(FadeOut(exitFadeDuration));
                gameObject.SetActive(false);
            }

            StartCoroutine(SignalEndAfterDelay(1f, token));
        }

        private IEnumerator SignalEndAfterDelay(float seconds, BarrierToken token)
        {
            yield return new WaitForSeconds(seconds);
            _waitBarrier.Signal(new BarrierKey(token));
        }

        private IEnumerator PopIn(float duration)
        {
            float t = 0f;
            Vector3 from = Vector3.one * startScale;
            Vector3 toOvershoot = Vector3.one * overshootScale;
            Vector3 toFinal = Vector3.one * finalScale;

            // Phase A: fade in + scale to overshoot
            while (t < duration * 0.65f)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / (duration * 0.65f));

                // EaseOutBack-ish curve for the first leg
                float eased = EaseOutCubic(u);

                canvasGroup.alpha = eased;
                // squash/stretch: scale X slightly more for style
                float squash = Mathf.Lerp(from.x, toOvershoot.x, eased);
                _rt.localScale = new Vector3(squash * 1.05f, squash * 0.95f, 1f);
                yield return null;
            }

            // Phase B: settle to final
            t = 0f;
            while (t < duration * 0.35f)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / (duration * 0.35f));
                float eased = EaseOutCubic(u);
                _rt.localScale = Vector3.Lerp(toOvershoot, toFinal, eased);
                yield return null;
            }
            _rt.localScale = toFinal;
            canvasGroup.alpha = 1f;
        }

        private IEnumerator OutlineFlash(float duration)
        {
            // Safeguard
            if (_runtimeMat == null) yield break;

            // Capture current
            float baseOutline = _runtimeMat.HasProperty(ID_OutlineWidth) ? _runtimeMat.GetFloat(ID_OutlineWidth) : 0f;
            float baseDilate = _runtimeMat.HasProperty(ID_FaceDilate) ? _runtimeMat.GetFloat(ID_FaceDilate) : 0f;

            float peakOutline = baseOutline + 0.2f;
            float peakDilate = baseDilate + 0.1f;

            float half = duration * 0.5f;
            float t = 0f;

            // Up
            while (t < half)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / half);
                float e = EaseOutCubic(u);
                _runtimeMat.SetFloat(ID_OutlineWidth, Mathf.Lerp(baseOutline, peakOutline, e));
                _runtimeMat.SetFloat(ID_FaceDilate, Mathf.Lerp(baseDilate, peakDilate, e));
                yield return null;
            }

            // Down
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / half);
                float e = EaseInCubic(u);
                _runtimeMat.SetFloat(ID_OutlineWidth, Mathf.Lerp(peakOutline, baseOutline, e));
                _runtimeMat.SetFloat(ID_FaceDilate, Mathf.Lerp(peakDilate, baseDilate, e));
                yield return null;
            }
            _runtimeMat.SetFloat(ID_OutlineWidth, baseOutline);
            _runtimeMat.SetFloat(ID_FaceDilate, baseDilate);
        }

        private IEnumerator Punch(int count, float eachDuration, float magnitude)
        {
            Vector3 baseScale = Vector3.one * finalScale;
            for (int i = 0; i < count; i++)
            {
                // up
                float t = 0f;
                while (t < eachDuration * 0.45f)
                {
                    t += Time.deltaTime;
                    float u = Mathf.Clamp01(t / (eachDuration * 0.45f));
                    float s = Mathf.Lerp(0f, magnitude, EaseOutCubic(u));
                    _rt.localScale = baseScale * (1f + s);
                    yield return null;
                }

                // down (slightly below base for bounce feel)
                t = 0f;
                while (t < eachDuration * 0.55f)
                {
                    t += Time.deltaTime;
                    float u = Mathf.Clamp01(t / (eachDuration * 0.55f));
                    float s = Mathf.Lerp(magnitude, -magnitude * 0.35f, EaseInOutCubic(u));
                    _rt.localScale = baseScale * (1f + s);
                    yield return null;
                }

                _rt.localScale = baseScale;
            }
        }

        private IEnumerator IdleWobble(float seconds)
        {
            float t = 0f;
            Vector3 baseScale = Vector3.one * finalScale;
            float baseRot = 0f;

            while (t < seconds)
            {
                t += Time.deltaTime;
                float s = Mathf.Sin(t * 2.2f) * idleScaleAmp;
                float r = Mathf.Sin(t * 1.6f) * idleRotAmp;

                _rt.localScale = baseScale * (1f + s);
                _rt.localRotation = Quaternion.Euler(0f, 0f, baseRot + r);
                yield return null;
            }

            _rt.localScale = baseScale;
            _rt.localRotation = Quaternion.identity;
        }

        private IEnumerator FadeOut(float duration)
        {
            float t = 0f;
            float startAlpha = canvasGroup.alpha;
            Vector3 startScale = _rt.localScale;
            Vector3 endScale = startScale * 0.95f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / duration);
                float e = EaseInCubic(u);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, e);
                _rt.localScale = Vector3.Lerp(startScale, endScale, e);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            _rt.localScale = endScale;
        }

        // --- EASING ---
        private static float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
        private static float EaseInCubic(float x) => x * x * x;
        private static float EaseInOutCubic(float x)
        {
            return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
        }
    }
}