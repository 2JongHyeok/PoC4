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

        [Header("Mana")]
        [SerializeField] private float maxMana = 100f;
        private float currentMana;

        [Header("Level & Experience")]
        [SerializeField] private int level = 1;
        [SerializeField] private int currentXP = 0;
        [SerializeField] private int xpToNextLevel = 100;

        [Header("Circle Level")]
        [SerializeField] private int initialCircleLevel = 1;
        public int PlayerCircleLevel { get; private set; }

        public int Level => level;
        public float CurrentMana => currentMana;

        public event Action<int> OnLevelUp;
        public event Action<int, int> OnXPChanged;
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnManaChanged;
        public event Action<int> OnPlayerCircleLevelChanged;

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
            currentMana = maxMana;
            PlayerCircleLevel = initialCircleLevel;
            
            // Initial UI updates
            OnLevelUp?.Invoke(level);
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnPlayerCircleLevelChanged?.Invoke(PlayerCircleLevel);
        }

        // Temporary method for testing
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.X)) AddExperience(25);
            if (Input.GetKeyDown(KeyCode.C)) IncreasePlayerCircleLevel();
            if (Input.GetKeyDown(KeyCode.P)) RestoreFullHealth(); // Debug: Restore Health
            if (Input.GetKeyDown(KeyCode.O)) RestoreFullMana();   // Debug: Restore Mana
        }

        public void IncreasePlayerCircleLevel()
        {
            PlayerCircleLevel++;
            Debug.Log($"Player Circle Level increased to {PlayerCircleLevel}");
            OnPlayerCircleLevelChanged?.Invoke(PlayerCircleLevel);
        }

        public bool UseMana(float amount)
        {
            if (currentMana >= amount)
            {
                currentMana -= amount;
                OnManaChanged?.Invoke(currentMana, maxMana);
                return true;
            }
            return false; // Not enough mana
        }

        public void RestoreFullHealth()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            Debug.Log("DEBUG: Health Restored.");
        }

        public void RestoreFullMana()
        {
            currentMana = maxMana;
            OnManaChanged?.Invoke(currentMana, maxMana);
            Debug.Log("DEBUG: Mana Restored.");
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
            
            Debug.Log($"LEVEL UP! Reached level {level}.");
            OnLevelUp?.Invoke(level);
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
