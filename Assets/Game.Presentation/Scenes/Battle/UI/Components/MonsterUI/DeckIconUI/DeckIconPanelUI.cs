using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckIconPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _drawPileText;
    [SerializeField] private TextMeshProUGUI _discardPileText;
    [SerializeField] private Image _deckIcon;

    public Vector3 DeckIconPosition 
    {
        get
        {
            if (_deckIcon != null)
            {
                // Convert UI position to world space using main camera
                var canvas = _deckIcon.GetComponentInParent<Canvas>();
                var rectTransform = _deckIcon.GetComponent<RectTransform>();
                
                if (canvas != null && rectTransform != null)
                {
                    Vector3 worldPos;
                    
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        // For screen space overlay, convert UI position to screen then to world
                        var camera = Camera.main;
                        if (camera != null)
                        {
                            // Get screen position from RectTransform
                            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
                            worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
                            Debug.Log($"DeckIcon UI pos: {rectTransform.position}, screen pos: {screenPos}, world pos: {worldPos}");
                            return worldPos;
                        }
                    }
                    else if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera != null)
                    {
                        // For camera space canvas, use the canvas camera
                        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rectTransform.position);
                        worldPos = canvas.worldCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
                        Debug.Log($"DeckIcon UI pos: {rectTransform.position}, screen pos: {screenPos}, world pos: {worldPos}");
                        return worldPos;
                    }
                    else if (canvas.renderMode == RenderMode.WorldSpace)
                    {
                        // For world space canvas, the position is already in world space
                        worldPos = rectTransform.position;
                        Debug.Log($"DeckIcon world space pos: {worldPos}");
                        return worldPos;
                    }
                }
            }
            Debug.Log($"DeckIcon fallback to transform pos: {transform.position}");
            return transform.position;
        }
    }

    public void SetDeckIconText(int drawCards, int discardCards)
    {
        Debug.Log($"Draw cards: {drawCards}");
        _drawPileText.SetText(drawCards > 0 ? drawCards.ToString() : "0");
        _discardPileText.SetText(discardCards > 0 ? discardCards.ToString() : "0");
        _drawPileText.ForceMeshUpdate();
        _discardPileText.ForceMeshUpdate();
    }
}
