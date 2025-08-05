using TMPro;
using UnityEngine;
using System.Collections;

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
        Vector3 endPos = startPos + Vector3.up * 1f;

        float elapsed = 0f;
        var cg = GetComponent<CanvasGroup>();

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            cg.alpha = Mathf.Lerp(1, 0, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}