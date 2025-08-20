using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckIconPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _drawPileText;
    [SerializeField] private TextMeshProUGUI _discardPileText;
    [SerializeField] private Image _deckIcon;

    public Vector3 DeckIconPosition => _deckIcon != null ? _deckIcon.transform.position : transform.position;

    public void SetDeckIconText(int drawCards, int discardCards)
    {
        Debug.Log($"Draw cards: {drawCards}");
        _drawPileText.SetText(drawCards > 0 ? drawCards.ToString() : "0");
        _discardPileText.SetText(discardCards > 0 ? discardCards.ToString() : "0");
        _drawPileText.ForceMeshUpdate();
        _discardPileText.ForceMeshUpdate();
    }
}
