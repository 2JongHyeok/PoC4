using UnityEngine;
using System.Collections;

namespace Poc4.Spells
{
    public class AoEVisualizer : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.5f;
        private float maxScale;

        public void Initialize(float radius)
        {
            // Set max scale to be the diameter of the explosion
            this.maxScale = radius * 2f;
            transform.localScale = Vector3.zero;
            StartCoroutine(AnimateCircle());
        }

        private IEnumerator AnimateCircle()
        {
            float halfDuration = animationDuration / 2f;
            float timer = 0f;

            // Scale up
            while (timer < halfDuration)
            {
                float progress = timer / halfDuration;
                float currentScale = Mathf.Lerp(0, maxScale, progress);
                transform.localScale = new Vector3(currentScale, currentScale, 1);
                timer += Time.deltaTime;
                yield return null;
            }

            // Reset timer for scale down
            timer = 0f;

            // Scale down
            while (timer < halfDuration)
            {
                float progress = timer / halfDuration;
                float currentScale = Mathf.Lerp(maxScale, 0, progress);
                transform.localScale = new Vector3(currentScale, currentScale, 1);
                timer += Time.deltaTime;
                yield return null;
            }

            // Ensure it ends at zero scale and destroy
            transform.localScale = Vector3.zero;
            Destroy(gameObject);
        }

        // --- Usage in Unity ---
        // 1. Create a new empty GameObject and name it "ExplosionVisual".
        // 2. Add a SpriteRenderer and assign a simple circle sprite to it.
        // 3. Attach this AoEVisualizer.cs script to the "ExplosionVisual" GameObject.
        // 4. Adjust Animation Duration in the Inspector.
        // 5. Create a prefab from this GameObject.
        // 6. Assign this prefab to the 'Explosion Visual Prefab' field in your DamageSpellEffect assets.
        // 7. The scale is now set dynamically by the projectile that creates it.
    }
}
