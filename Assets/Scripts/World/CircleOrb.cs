using UnityEngine;
using Poc4.Player;

namespace Poc4.World
{
    [RequireComponent(typeof(Collider2D))]
    public class CircleOrb : MonoBehaviour
    {
        [Header("Effects")]
        [SerializeField] private GameObject pickupEffectPrefab; // e.g., a particle system
        [SerializeField] private bool isSingleUse = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if the orb collided with the player
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                Debug.Log("Circle Orb collected by player.");
                
                // Increase the player's circle level
                playerStats.IncreasePlayerCircleLevel();

                // Play pickup effect if assigned
                if (pickupEffectPrefab != null)
                {
                    Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                }

                // Destroy the orb if it's single-use
                if (isSingleUse)
                {
                    Destroy(gameObject);
                }
            }
        }

        // --- Usage in Unity ---
        // 1. Create a new GameObject for the orb (e.g., a sprite).
        // 2. Add a Collider2D component to it and set 'Is Trigger' to true.
        // 3. Attach this CircleOrb.cs script to it.
        // 4. Optionally, create a particle effect prefab and assign it to the 'Pickup Effect Prefab' field.
        // 5. Place the orb in the scene for the player to collect.
    }
}
