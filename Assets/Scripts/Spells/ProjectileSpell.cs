using UnityEngine;
using System.Collections.Generic;
using Poc4.Combat;

namespace Poc4.Spells
{
    public class ProjectileSpell : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private LayerMask hitLayer; // Layers the projectile can hit

        private SpellData spellData;
        private Vector2 direction;
        private float currentLifetime;
        private bool hasBeenUsed = false; // Prevents multiple triggers

        public void Initialize(SpellData data, Vector2 targetWorldPosition)
        {
            spellData = data;
            direction = (targetWorldPosition - (Vector2)transform.position).normalized;
            currentLifetime = lifetime;
            hasBeenUsed = false;
            transform.rotation = Quaternion.identity;

            // Apply projectile scale as a multiplier to the prefab's base scale
            if (spellData.projectileScale != 1f) // Optimization: only change if not default
            {
                transform.localScale *= spellData.projectileScale;
            }
        }

        private void Update()
        {
            if (hasBeenUsed) return;

            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0)
            {
                // If it's an AoE spell, explode on lifetime expiry. Otherwise, just disappear.
                if (spellData.aoeRadius > 0)
                {
                    Explode();
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
            }

            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasBeenUsed) return;

            // Check if the collided object is in the hitLayer
            if (((1 << other.gameObject.layer) & hitLayer) != 0)
            {
                // If it's a single-target spell, apply damage directly
                if (spellData.aoeRadius <= 0)
                {
                    hasBeenUsed = true;
                    Debug.Log($"'{spellData.spellName}' hit single target {other.gameObject.name}.");
                    ApplyDamage(other.gameObject);
                    Destroy(gameObject);
                }
                else // Otherwise, it's an AoE spell, so explode on impact
                {
                    Explode();
                }
            }
        }

        private void Explode()
        {
            if (hasBeenUsed) return;
            hasBeenUsed = true;

            Debug.Log($"'{spellData.spellName}' exploding at {transform.position}");

            // Find all colliders within the explosion radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, spellData.aoeRadius, hitLayer);
            
            // Use a HashSet to damage each unique enemy only once
            HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
            foreach (var hit in hits)
            {
                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damagedTargets.Add(damageable);
                }
            }

            // Apply damage to all unique targets found
            foreach (IDamageable target in damagedTargets)
            {
                target.TakeDamage(spellData.damage);
            }

            // For now, we don't have a visual prefab in the new simple SpellData.
            // This can be re-added later if needed.

            Destroy(gameObject);
        }

        private void ApplyDamage(GameObject target)
        {
            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(spellData.damage);
            }
        }
    }
}
