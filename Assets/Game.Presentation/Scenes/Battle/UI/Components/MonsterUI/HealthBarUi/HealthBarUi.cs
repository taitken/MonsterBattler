using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Game.Presentation.Scenes.Battle.UI.MonsterUI.HealthBarUi
{
    public class HealthBarUi : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI healthText;
        public int currentHealth { get; set; }

        public void SetHealth(int _currentHealth, int maxHealth)
        {
            this.currentHealth = _currentHealth;
            this.healthText.SetText(currentHealth.ToString());
            var percentage = (float)currentHealth / maxHealth;
            fillImage.fillAmount = Mathf.Clamp01(percentage);
            healthText.ForceMeshUpdate();
        }
    }
}