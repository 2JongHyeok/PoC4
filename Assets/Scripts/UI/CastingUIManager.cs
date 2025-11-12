using UnityEngine;
using TMPro;
using Poc4.Spells;

namespace Poc4.UI
{
    public class CastingUIManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SpellCastingSystem spellCastingSystem;

        [Header("UI Elements")]
        [SerializeField] private GameObject castingUIParent; // The parent object containing all casting UI
        [SerializeField] private RectTransform fillRect; // The RectTransform of the casting bar's "Fill" image

        private void Start()
        {
            if (spellCastingSystem == null)
            {
                // Attempt to find it on the player if not assigned
                spellCastingSystem = FindFirstObjectByType<SpellCastingSystem>();
            }

            SubscribeToEvents();
            
            if (castingUIParent != null)
            {
                castingUIParent.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (spellCastingSystem != null)
            {
                spellCastingSystem.OnCastingStarted += HandleCastingStarted;
                spellCastingSystem.OnCastingProgress += HandleCastingProgress;
                spellCastingSystem.OnCastingFinished += HandleCastingFinished;
            }
            else
            {
                Debug.LogError("CastingUIManager: SpellCastingSystem could not be found. Please assign it in the Inspector.", this);
                this.enabled = false;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (spellCastingSystem != null)
            {
                spellCastingSystem.OnCastingStarted -= HandleCastingStarted;
                spellCastingSystem.OnCastingProgress -= HandleCastingProgress;
                spellCastingSystem.OnCastingFinished -= HandleCastingFinished;
            }
        }

        private void HandleCastingStarted()
        {
            if (castingUIParent != null)
            {
                castingUIParent.SetActive(true);
            }
            UpdateCastingBar(0, 1); // Reset bar at the beginning
        }

        private void HandleCastingProgress(float currentTime, float totalTime)
        {
            UpdateCastingBar(currentTime, totalTime);
        }

        private void HandleCastingFinished()
        {
            if (castingUIParent != null)
            {
                castingUIParent.SetActive(false);
            }
        }

        private void UpdateCastingBar(float currentValue, float maxValue)
        {
            if (fillRect != null)
            {
                float fillAmount = (maxValue > 0) ? currentValue / maxValue : 0;
                fillRect.localScale = new Vector3(fillAmount, 1, 1);
            }
        }

        // --- Usage in Unity ---
        // 1. Attach this script to your "UIManager" GameObject.
        // 2. In the Inspector, assign the SpellCastingSystem component.
        // 3. Create a UI for the casting bar (e.g., a background Image with a child "Fill" Image).
        // 4. Assign the parent GameObject of the bar to 'Casting UI Parent'.
        // 5. Assign the "Fill" Image's RectTransform to the 'Fill Rect' field.
        // 6. Ensure the "Fill" RectTransform's pivot is set to (0, 0.5).
    }
}

