using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Poc4.Player;

namespace Poc4.Spells
{
    public class SpellCastingSystem : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerStats playerStats;

        private bool isCasting = false;
        private Dictionary<SpellData, float> spellCooldowns = new Dictionary<SpellData, float>();
        private Coroutine castingCoroutine;

        public bool IsCasting => isCasting;

        // Events for UI
        public event Action<float, float> OnCastingProgress; // currentTime, totalTime
        public event Action OnCastingStarted;
        public event Action OnCastingFinished; // Success, interrupt, or fail
        public event Action<SpellData, float> OnCooldownStarted; // Spell, cooldown duration

        private void Awake()
        {
            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
            }
        }

        public bool CanCast(SpellData spell)
        {
            if (spell == null) return false;
            if (isCasting)
            {
                // Already casting, so can't cast another.
                return false;
            }
            if (playerStats.PlayerCircleLevel < spell.requiredCircle)
            {
                Debug.Log($"Cannot cast: Player circle level ({playerStats.PlayerCircleLevel}) is too low for {spell.spellName} (requires {spell.requiredCircle}).");
                return false;
            }
            if (playerStats.CurrentMana < spell.manaCost)
            {
                Debug.Log($"Cannot cast: Not enough mana for {spell.spellName}.");
                return false;
            }
            if (spellCooldowns.TryGetValue(spell, out float cooldownEndTime) && Time.time < cooldownEndTime)
            {
                Debug.Log($"Cannot cast: {spell.spellName} is on cooldown.");
                return false;
            }
            
            return true;
        }

        public void AttemptToCast(SpellData spell, Vector2 targetPosition)
        {
            if (!CanCast(spell))
            {
                OnCastingFinished?.Invoke();
                return;
            }

            castingCoroutine = StartCoroutine(CastingCoroutine(spell, targetPosition));
        }

        private IEnumerator CastingCoroutine(SpellData spell, Vector2 targetPosition)
        {
            isCasting = true;
            OnCastingStarted?.Invoke();
            Debug.Log($"Casting {spell.spellName}...");

            float castTimer = 0f;
            while (castTimer < spell.castingTime)
            {
                castTimer += Time.deltaTime;
                OnCastingProgress?.Invoke(castTimer, spell.castingTime);
                yield return null;
            }

            // Final check for mana right before casting, and consume it.
            if (playerStats.UseMana(spell.manaCost))
            {
                // --- Fire Spell Logic ---
                if (spell.projectilePrefab != null)
                {
                    StartCoroutine(FireSequentialProjectiles(spell, spell.projectileCount, targetPosition));
                }
                else
                {
                    Debug.LogWarning($"Spell {spell.spellName} has no projectile prefab assigned.");
                }
                // -----------------------
                
                // Set cooldown
                spellCooldowns[spell] = Time.time + spell.cooldown;
                OnCooldownStarted?.Invoke(spell, spell.cooldown);
                Debug.Log($"{spell.spellName} cast successfully. Cooldown started.");
            }
            else
            {
                Debug.LogWarning($"Failed to cast {spell.spellName}: Not enough mana at the last moment.");
            }

            isCasting = false;
            OnCastingFinished?.Invoke();
            castingCoroutine = null;
        }

        private IEnumerator FireSequentialProjectiles(SpellData spell, int count, Vector2 targetPosition)
        {
            if (count <= 0) count = 1;
            
            // For single projectiles, fire instantly. For multiple, spread over 1 second.
            float delay = (count > 1) ? 1.0f / count : 0;

            for (int i = 0; i < count; i++)
            {
                ProjectileSpell projectile = Instantiate(spell.projectilePrefab, transform.position, Quaternion.identity).GetComponent<ProjectileSpell>();
                projectile.Initialize(spell, targetPosition);

                if (delay > 0)
                {
                    yield return new WaitForSeconds(delay);
                }
            }
        }

        public void InterruptCasting()
        {
            if (!isCasting || castingCoroutine == null) return;

            StopCoroutine(castingCoroutine);
            isCasting = false;
            OnCastingFinished?.Invoke();
            castingCoroutine = null;
            Debug.Log("Casting interrupted.");
        }
    }
}