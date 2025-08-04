using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void SetHealth(float percentage)
    {
        fillImage.fillAmount = Mathf.Clamp01(percentage);
    }
}
