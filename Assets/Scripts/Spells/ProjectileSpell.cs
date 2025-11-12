using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private int circleCount;
        private Vector2 direction;
        private float currentLifetime;
        private bool hasExploded = false;

        public void Initialize(SpellData data, int circles, Vector2 targetWorldPosition)
        {
            spellData = data;
            circleCount = circles;
            direction = (targetWorldPosition - (Vector2)transform.position).normalized; // Calculate direction from spawn to target
            currentLifetime = lifetime;
            hasExploded = false;
            transform.rotation = Quaternion.identity;

            // Apply projectile scale based on circle count as a multiplier to the prefab's base scale
            int circleIndex = circleCount - 1;
            if (spellData.projectileScales != null && circleIndex < spellData.projectileScales.Length)
            {
                float scaleMultiplier = spellData.projectileScales[circleIndex];
                Vector3 baseScale = transform.localScale; // Get the prefab's base scale
                transform.localScale = baseScale * scaleMultiplier;
            }
        }

        private void Update()
        {
            if (hasExploded) return;

            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0)
            {
                // If it's a single-target spell (explosionRadius 0), just destroy it on lifetime expiry
                if (spellData?.spellEffect is DamageSpellEffect damageEffect && damageEffect.explosionRadius == 0)
                {
                    Destroy(gameObject);
                }
                else // Otherwise, explode
                {
                    Explode();
                }
                return;
            }

            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasExploded) return;

            // Check if the collided object is in the hitLayer
            if (((1 << other.gameObject.layer) & hitLayer) != 0)
            {
                // If it's a single-target spell (explosionRadius 0), apply damage directly
                if (spellData?.spellEffect is DamageSpellEffect damageEffect && damageEffect.explosionRadius == 0)
                {
                    Debug.Log($"'{spellData.spellName}' hit single target {other.gameObject.name}.");
                    damageEffect.ApplyEffect(other.gameObject, spellData, circleCount);
                    Destroy(gameObject);
                }
                else // Otherwise, explode
                {
                    Explode();
                }
            }
        }

        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;

            Debug.Log($"'{spellData.spellName}' exploding at {transform.position}");

            if (spellData?.spellEffect is DamageSpellEffect damageEffect)
            {
                int circleIndex = circleCount - 1;

                // Get AoE radius for the current circle, with a fallback to the effect's default radius
                float radius = damageEffect.explosionRadius;
                if (spellData.aoeRadiuses != null && circleIndex < spellData.aoeRadiuses.Length)
                {
                    radius = spellData.aoeRadiuses[circleIndex];
                }

                // Instantiate and initialize visual effect
                if (damageEffect.explosionVisualPrefab != null)
                {
                    GameObject visualizerObject = Instantiate(damageEffect.explosionVisualPrefab, transform.position, Quaternion.identity);
                    AoEVisualizer visualizer = visualizerObject.GetComponent<AoEVisualizer>();
                    if (visualizer != null)
                    {
                        visualizer.Initialize(radius);
                    }
                }

                // Find all colliders within the explosion radius
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, hitLayer);
                
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
                    Component targetComponent = (Component)target;
                    damageEffect.ApplyEffect(targetComponent.gameObject, spellData, circleCount);
                }
            }
            else
            {
                Debug.LogWarning($"Spell '{spellData.spellName}' has no DamageSpellEffect with AoE data.");
            }

            Destroy(gameObject);
        }

        // --- Usage in Unity ---
        // 1. This script is attached to a projectile prefab.
        // 2. The projectile explodes on contact with an object on the 'Hit Layer' or when its lifetime expires.
        // 3. The explosion damage and radius are defined in the 'DamageSpellEffect' asset, which is referenced by the 'SpellData' asset.
    }
}
