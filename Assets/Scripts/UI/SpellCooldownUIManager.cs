using UnityEngine;
using System.Collections.Generic;
using Poc4.Player;
using Poc4.Spells;

namespace Poc4.UI
{
    public class SpellCooldownUIManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private SpellCastingSystem spellCastingSystem;

        [Header("UI Setup")]
        [SerializeField] private List<SpellIconController> spellIcons;

        private void Start()
        {
            // Find dependencies if not assigned
            if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
            if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();
            if (spellCastingSystem == null) spellCastingSystem = FindFirstObjectByType<SpellCastingSystem>();

            InitializeIcons();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeIcons()
        {
            SpellData[] assignedSpells = playerController.GetAssignedSpells();
            for (int i = 0; i < spellIcons.Count; i++)
            {
                if (i < assignedSpells.Length)
                {
                    spellIcons[i].Initialize(assignedSpells[i]);
                }
                else
                {
                    spellIcons[i].Initialize(null); // Handle empty slots
                }
            }
        }

        private void Update()
        {
            // Continuously update the state of each icon
            foreach (var icon in spellIcons)
            {
                if (icon.AssignedSpell != null)
                {
                    bool canCast = spellCastingSystem.CanCast(icon.AssignedSpell);
                    bool hasEnoughMana = playerStats.CurrentMana >= icon.AssignedSpell.manaCost;
                    icon.UpdateState(canCast, spellCastingSystem.IsCasting, hasEnoughMana);
                }
            }
        }

        private void SubscribeToEvents()
        {
            if (spellCastingSystem != null)
            {
                spellCastingSystem.OnCooldownStarted += HandleCooldownStarted;
            }
            // No need to subscribe to PlayerCircleLevelChanged as Update() handles it every frame
        }

        private void UnsubscribeFromEvents()
        {
            if (spellCastingSystem != null)
            {
                spellCastingSystem.OnCooldownStarted -= HandleCooldownStarted;
            }
        }

        private void HandleCooldownStarted(SpellData spell, float duration)
        {
            // Find the icon for this spell and start its cooldown visual
            foreach (var icon in spellIcons)
            {
                if (icon.AssignedSpell == spell)
                {
                    icon.StartCooldown(duration);
                    break;
                }
            }
        }
    }
}
