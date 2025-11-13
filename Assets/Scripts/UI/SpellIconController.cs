using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Poc4.UI
{
    public class SpellIconController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay; // Black, semi-transparent image for cooldown wipe
        [SerializeField] private Image unavailableOverlay; // Red, semi-transparent image for level requirement
        [SerializeField] private GameObject selectionHighlight; // Image behind the icon to show selection

        private SpellData assignedSpell;
        private float cooldownEndTime;
        private float cooldownDuration;

        public SpellData AssignedSpell => assignedSpell;

        public void Initialize(SpellData spell)
        {
            assignedSpell = spell;
            if (assignedSpell != null)
            {
                iconImage.sprite = assignedSpell.icon;
                gameObject.name = $"SpellIcon_{assignedSpell.spellName}";
            }
            else
            {
                iconImage.enabled = false;
            }
            SetHighlight(false); // Ensure highlight is off initially
        }

        public void UpdateState(bool canCast, bool isCastingOther, bool hasEnoughMana)
        {
            if (assignedSpell == null) return;

            bool isOnCooldown = Time.time < cooldownEndTime;
            bool levelRequirementMet = Player.PlayerStats.Instance.PlayerCircleLevel >= assignedSpell.requiredCircle;

            if (!levelRequirementMet || !hasEnoughMana)
            {
                // State 1: Cannot Cast (Level or Mana)
                iconImage.color = Color.white; // Normal icon color
                cooldownOverlay.gameObject.SetActive(false);
                unavailableOverlay.gameObject.SetActive(true);
            }
            else if (isOnCooldown)
            {
                // State 2: On Cooldown
                iconImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Dimmed
                unavailableOverlay.gameObject.SetActive(false);
                cooldownOverlay.gameObject.SetActive(true);

                float remainingTime = cooldownEndTime - Time.time;
                cooldownOverlay.rectTransform.localScale = new Vector3(1, remainingTime / cooldownDuration, 1);
            }
            else
            {
                // State 3: Ready to Cast (or currently casting another spell)
                unavailableOverlay.gameObject.SetActive(false);
                cooldownOverlay.gameObject.SetActive(false);
                // Dim the icon if player is busy casting something else or another condition in CanCast fails
                iconImage.color = (isCastingOther || !canCast) ? new Color(0.5f, 0.5f, 0.5f, 0.8f) : Color.white;
            }
        }

        public void StartCooldown(float duration)
        {
            cooldownDuration = duration;
            cooldownEndTime = Time.time + duration;
        }

        public void SetHighlight(bool active)
        {
            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(active);
            }
        }
    }
}