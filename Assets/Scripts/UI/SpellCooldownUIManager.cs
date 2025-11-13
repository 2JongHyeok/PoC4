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

        private SpellIconController currentSelectedIcon;

        private void Start()
        {
            // Find dependencies if not assigned
            if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
            if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();
            if (spellCastingSystem == null) spellCastingSystem = FindFirstObjectByType<SpellCastingSystem>();

            InitializeIcons();
            SubscribeToEvents();

            // Set initial highlight if a spell is already selected
            if (playerController.CurrentSelectedSpell != null)
            {
                HandleSpellSelected(playerController.CurrentSelectedSpell);
            }
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
            if (playerController != null)
            {
                playerController.OnSpellSelected += HandleSpellSelected;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (spellCastingSystem != null)
            {
                spellCastingSystem.OnCooldownStarted -= HandleCooldownStarted;
            }
            if (playerController != null)
            {
                playerController.OnSpellSelected -= HandleSpellSelected;
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

        private void HandleSpellSelected(SpellData spell)
        {
            // Deactivate previous highlight
            if (currentSelectedIcon != null)
            {
                currentSelectedIcon.SetHighlight(false);
            }

            // Activate new highlight
            foreach (var icon in spellIcons)
            {
                if (icon.AssignedSpell == spell)
                {
                    icon.SetHighlight(true);
                    currentSelectedIcon = icon;
                    break;
                }
            }
        }
    }
}