using UnityEngine;
using TMPro;
using Poc4.Player;

namespace Poc4.UI
{
    public class PlayerCircleUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI circleLevelText;

        private void Start()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnPlayerCircleLevelChanged += UpdateCircleLevelText;
                // Set initial value
                UpdateCircleLevelText(PlayerStats.Instance.PlayerCircleLevel);
            }
            else
            {
                Debug.LogError("PlayerCircleUI: Could not find PlayerStats.Instance at Start.", this);
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnPlayerCircleLevelChanged -= UpdateCircleLevelText;
            }
        }

        private void UpdateCircleLevelText(int level)
        {
            if (circleLevelText != null)
            {
                circleLevelText.text = $"Circle: {level}";
            }
        }

        // --- Usage in Unity ---
        // 1. Attach this script to your "UIManager" GameObject.
        // 2. Create a TextMeshProUGUI element in your Canvas for the circle level.
        // 3. In the "UIManager" GameObject's Inspector, drag the TextMeshProUGUI element to the 'Circle Level Text' field.
    }
}
