using System.Collections.Generic;
using Game.Presentation.Shared.Views;
using TMPro;
using UnityEngine;

public class CardSelectWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private RectTransform _cardPosition;
    
    public void Show()
    {
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
