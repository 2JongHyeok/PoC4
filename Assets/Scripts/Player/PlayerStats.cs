using UnityEngine;
using System;
using Poc4.Combat; // For IDamageable

namespace Poc4.Player
{
    public class PlayerStats : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;

        [Header("Level & Experience")]
        [SerializeField] private int level = 1;
        [SerializeField] private int currentXP = 0;
        [SerializeField] private int xpToNextLevel = 100;

        [Header("Casting Time Scaling")]
        [Tooltip("The casting time multiplier at level 1. Should be 1.")]
        [SerializeField] private float initialCastingMultiplier = 1.0f;
        [Tooltip("How much the multiplier decreases per level. E.g., 0.05 means 5% faster casting per level.")]
        [SerializeField] private float castingMultiplierReductionPerLevel = 0.05f;

        public int Level => level;
        public float CastingTimeMultiplier { get; private set; }

        public event Action<int> OnLevelUp;
        public event Action<int, int> OnXPChanged;
        public event Action<float, float> OnHealthChanged;

        public static PlayerStats Instance { get; private set; }

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            currentHealth = maxHealth;
            CalculateCastingMultiplier();
            OnLevelUp?.Invoke(level);
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        // Temporary method for testing
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                AddExperience(25);
            }
        }

        public void AddExperience(int amount)
        {
            currentXP += amount;
            Debug.Log($"Gained {amount} XP. Total XP: {currentXP}/{xpToNextLevel}");

            while (currentXP >= xpToNextLevel)
            {
                currentXP -= xpToNextLevel;
                LevelUp();
            }
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);
        }

        private void LevelUp()
        {
            level++;
            // Simple formula for next level's XP requirement
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);
            
            CalculateCastingMultiplier();

            Debug.Log($"LEVEL UP! Reached level {level}. Casting time multiplier is now {CastingTimeMultiplier:P0}.");
            OnLevelUp?.Invoke(level);
        }

        private void CalculateCastingMultiplier()
        {
            // Multiplier decreases as level increases
            CastingTimeMultiplier = initialCastingMultiplier - (castingMultiplierReductionPerLevel * (level - 1));
            // Ensure the multiplier doesn't go below a certain threshold (e.g., 10% of original time)
            CastingTimeMultiplier = Mathf.Max(0.1f, CastingTimeMultiplier);
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            Debug.Log($"Player took {amount} damage. Current health: {currentHealth}/{maxHealth}");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Player has been defeated! Game Over.");
            // For now, just disable the player controller to stop actions
            GetComponent<PlayerController>().enabled = false;
            this.enabled = false;
        }

        // --- Usage in Unity ---
        // 1. Attach this script to the "Player" GameObject.
        // 2. Adjust health, level, and scaling values in the Inspector.
        // 3. While the game is running, press the 'X' key to add 25 XP and test the level-up mechanic.
        // 4. Ensure the Player's layer is set correctly so enemies can detect it.
    }
}
