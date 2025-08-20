using TMPro;
using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(TextMeshProUGUI))]
public class TmpWidowFix : MonoBehaviour
{
    [Min(2)] public int minLastLineWords = 2;
    [Tooltip("If off, only applies in play mode.")]
    public bool applyInEditMode = true;

    private TextMeshProUGUI _tmp;
    private string _originalText;

    const char NBSP = '\u00A0';

    void OnEnable()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        CacheOriginal();
        Apply();
    }

    void OnValidate()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        CacheOriginal();
        Apply();
    }

    void OnRectTransformDimensionsChange() => Apply();

    void CacheOriginal()
    {
        if (_tmp == null) return;
        _originalText = _tmp.text;
    }

    public void Apply()
    {
        if (_tmp == null) return;
        if (!Application.isPlaying && !applyInEditMode) return;
        if (string.IsNullOrWhiteSpace(_originalText)) return;

        string t = _originalText;

        for (int attempt = 0; attempt < 8; attempt++)
        {
            _tmp.text = t;
            _tmp.ForceMeshUpdate();
            var ti = _tmp.textInfo;
            if (ti.lineCount == 0) break;

            var last = ti.lineInfo[ti.lineCount - 1];
            int start = last.firstCharacterIndex;
            int end   = last.lastVisibleCharacterIndex;
            if (start < 0 || end < start) break;

            int words = 1;
            for (int i = start; i <= end && i < t.Length; i++)
                if (t[i] == ' ') words++;

            if (words >= minLastLineWords) break;

            // find the last space in the last two lines and glue it
            int spaceToGlue = -1;
            for (int i = end; i >= 0; i--)
            {
                if (t[i] == ' ') { spaceToGlue = i; break; }
                if (i == start) break;
            }
            if (spaceToGlue == -1) break;

            t = t.Remove(spaceToGlue, 1).Insert(spaceToGlue, NBSP.ToString());
        }

        _tmp.text = t;
    }

    void Update()
    {
        if (!Application.isPlaying && !applyInEditMode) return;
        if (_tmp != null && _tmp.text != _originalText)
            CacheOriginal(); // keep sync if you edit the text in inspector
        Apply();
    }
}