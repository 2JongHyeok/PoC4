using UnityEngine;

namespace Poc4.Spells
{
    public abstract class SpellEffect : ScriptableObject
    {
        [Header("Base Spell Effect Settings")]
        public string effectName = "New Spell Effect";
        public float baseDamage = 10f;
        public float baseKnockbackForce = 0f;
        public float baseDuration = 0f; // For status effects or DoT

        // Abstract method to apply the effect to a target
        public abstract void ApplyEffect(GameObject target, SpellData spellData, int circleCount);
    }
}
