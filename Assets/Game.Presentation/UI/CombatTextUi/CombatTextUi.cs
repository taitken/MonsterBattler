using UnityEngine;
using System.Collections;
using TMPro;

public class CombatTextUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float duration = 1f;

    public void Show(string combatText, Color color)
    {
        text.text = combatText;
        text.color = color;
        StartCoroutine(FloatUpAndFade());
    }

    private IEnumerator FloatUpAndFade()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * 2f;

        float elapsed = 0f;
        var cg = GetComponent<CanvasGroup>();

        // Display at full strength initially
        while (elapsed < duration)
        {
            float t = elapsed / duration / 2;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        // Smoothly float up and fade out
        while (elapsed < duration)
        {
            float t = elapsed / duration / 2 + 0.5f;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            cg.alpha = Mathf.Lerp(1, 0, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}