using UnityEngine;

namespace Poc4.Combat
{
    [CreateAssetMenu(menuName = "Poc4/Combat/Enemy Data", fileName = "NewEnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("Info")]
        public string enemyName;

        [Header("Stats")]
        public float maxHealth = 100f;
        public float moveSpeed = 3f;
        public float attackPower = 10f;
        public float attackSpeed = 1.0f; // Attacks per second
        public float attackRange = 2f;
        public float perceptionRange = 10f;
        public int xpValue = 50; // XP granted on death

        [Header("Visuals")]
        public Sprite sprite;
        // public GameObject enemyPrefab; // Optional: If different enemy types have different base prefabs
    }
}
