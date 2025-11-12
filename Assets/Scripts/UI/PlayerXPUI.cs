using UnityEngine;
using TMPro; // Using TextMeshPro for better text rendering
using Poc4.Player; // To access PlayerStats

namespace Poc4.UI
{
    public class PlayerXPUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private RectTransform xpFillRect; // The RectTransform of the XP bar's fill image
        [SerializeField] private TextMeshProUGUI xpText;

        private void Start()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnLevelUp += UpdateLevelText;
                PlayerStats.Instance.OnXPChanged += UpdateXPBar;
            }
            else
            {
                Debug.LogError("PlayerXPUI: Could not find PlayerStats.Instance at Start. Ensure PlayerStats object is in the scene and active.", this);
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnLevelUp -= UpdateLevelText;
                PlayerStats.Instance.OnXPChanged -= UpdateXPBar;
            }
        }

        private void UpdateLevelText(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"LV. {level}";
            }
        }

        private void UpdateXPBar(int currentXP, int xpToNextLevel)
        {
            if (xpFillRect != null)
            {
                float fillAmount = (xpToNextLevel > 0) ? (float)currentXP / xpToNextLevel : 0;
                xpFillRect.localScale = new Vector3(fillAmount, 1, 1);
            }

            if (xpText != null)
            {
                xpText.text = $"{currentXP} / {xpToNextLevel}";
            }
        }

        // --- Usage in Unity ---
        // 1. Attach this PlayerXPUI.cs script to your "UIManager" GameObject.
        // 2. Create the UI elements in your Canvas:
        //    - A TextMeshProUGUI for the level text.
        //    - An Image for the XP bar background.
        //    - A child Image of the background for the XP bar fill.
        //    - A TextMeshProUGUI for the XP text.
        // 3. In the "UIManager" GameObject's Inspector, drag the corresponding UI elements to the fields in this script.
        // 4. For the XP bar fill image, ensure its RectTransform's pivot is set to (0, 0.5) to scale from left to right.
    }
}
