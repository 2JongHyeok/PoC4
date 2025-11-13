using UnityEngine;
using Poc4.Player;

namespace Poc4.UI
{
    public class PlayerManaUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private RectTransform manaFillRect; // The RectTransform of the mana bar's fill image

        private void Start()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnManaChanged += UpdateManaBar;
                // The initial mana value is set by the OnManaChanged event fired from PlayerStats.Awake()
            }
            else
            {
                Debug.LogError("PlayerManaUI: Could not find PlayerStats.Instance at Start. Ensure PlayerStats object is in the scene and active.", this);
                gameObject.SetActive(false); // Disable UI if the player is not found
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnManaChanged -= UpdateManaBar;
            }
        }

        private void UpdateManaBar(float currentMana, float maxMana)
        {
            if (manaFillRect != null)
            {
                float fillAmount = (maxMana > 0) ? currentMana / maxMana : 0;
                manaFillRect.localScale = new Vector3(fillAmount, 1, 1);
            }
        }

        // --- Usage in Unity ---
        // 1. Attach this script to your "UIManager" GameObject.
        // 2. Create a UI for the mana bar (e.g., a background Image with a child "Fill" Image).
        // 3. In the "UIManager" GameObject's Inspector, drag the "Fill" Image's RectTransform to the 'Mana Fill Rect' field.
        // 4. Ensure the "Fill" RectTransform's pivot is set to (0, 0.5) to scale from left to right.
    }
}
