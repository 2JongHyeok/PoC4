using UnityEngine;
using Poc4.Combat; // For IDamageable

namespace Poc4.Spells
{
    [CreateAssetMenu(menuName = "Poc4/Spells/Effects/Damage Effect", fileName = "NewDamageSpellEffect")]
    public class DamageSpellEffect : SpellEffect
    {
        [Header("Damage Effect Settings")]
        public float damageMultiplier = 1f; // Additional multiplier for this specific damage effect

        [Header("AoE Settings")]
        public float explosionRadius = 3f;
        public GameObject explosionVisualPrefab;

        public override void ApplyEffect(GameObject target, SpellData spellData, int circleCount)
        {
            Debug.Log("[DamageSpellEffect] Inside ApplyEffect. Checking for IDamageable...");
            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                Debug.Log("[DamageSpellEffect] IDamageable found. Applying damage...");
                float finalDamage = 0f;

                if (spellData != null && spellData.damages != null && spellData.damages.Length > 0)
                {
                    // circleCount is 1-based, array is 0-based
                    int damageIndex = circleCount - 1;
                    if (damageIndex >= 0 && damageIndex < spellData.damages.Length)
                    {
                        finalDamage = spellData.damages[damageIndex] * damageMultiplier;
                    }
                    else
                    {
                        Debug.LogWarning($"DamageSpellEffect: circleCount ({circleCount}) is out of range for {spellData.name}'s damages array. Applying 0 damage.", this);
                    }
                }
                else
                {
                    Debug.LogWarning($"DamageSpellEffect: SpellData or its damages array is missing for {effectName}. Applying 0 damage.", this);
                }

                damageable.TakeDamage(finalDamage);
                Debug.Log($"{target.name} took {finalDamage} damage from {effectName}.");
            }
            else
            {
                Debug.Log($"{target.name} is not IDamageable. Cannot apply damage effect.");
            }
        }
    }
}