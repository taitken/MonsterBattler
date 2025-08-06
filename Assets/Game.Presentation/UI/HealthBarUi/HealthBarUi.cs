using UnityEngine;
using UnityEngine.UI;
namespace Game.Presentation.UI.Combat
{
    public class HealthBarUi : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        public int currentHealth { get; set; }

        public void SetHealth(int _currentHealth, int maxHealth)
        {
            this.currentHealth = _currentHealth;
            var percentage = (float)currentHealth / maxHealth;
            fillImage.fillAmount = Mathf.Clamp01(percentage);
        }
    }
}