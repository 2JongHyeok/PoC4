using UnityEngine;
using UnityEngine.UI;
using Poc4.Player; // To access PlayerStats

namespace Poc4.UI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private RectTransform fillRect; // The RectTransform of the fill image

        private void Start()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnHealthChanged += UpdateHealthBar;
            }
            else
            {
                Debug.LogError("PlayerHealthUI: Could not find PlayerStats.Instance at Start. Ensure PlayerStats object is in the scene and active.", this);
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnHealthChanged -= UpdateHealthBar;
            }
        }

        private void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            if (fillRect != null)
            {
                float fillAmount = (maxHealth > 0) ? currentHealth / maxHealth : 0;
                fillRect.localScale = new Vector3(fillAmount, 1, 1);
            }
        }

        // --- Usage in Unity ---
        // 1. Create a new empty GameObject in your scene and name it "UIManager" (or use an existing one).
        // 2. Attach this PlayerHealthUI.cs script to it.
        // 3. Create a Canvas in your scene.
        // 4. Inside the Canvas, create a UI Image (e.g., named "HealthBarBackground").
        // 5. As a child of "HealthBarBackground", create another UI Image (e.g., named "HealthBarFill"). This will be the fillRect.
        // 6. In the "UIManager" GameObject's Inspector, drag the "HealthBarFill" RectTransform to the 'Fill Rect' field.
        // 7. Ensure the "HealthBarFill" RectTransform's pivot is set to (0, 0.5) so it scales from left to right.
        // 8. Ensure the Player GameObject with the PlayerStats script is present in the scene.
    }
}
