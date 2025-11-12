using UnityEngine;
using Poc4.Player;
using System.Collections;

namespace Poc4.Combat
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        private enum EnemyState { Idle, Chasing, Attacking, Returning }

        [Header("Data & Dependencies")]
        [SerializeField] private EnemyData enemyData;
        [SerializeField] private Transform spawnPoint; // Assign a SpawnPoint object with a CircleCollider2D for the leash range

        [Header("Colliders")]
        [SerializeField] private Collider2D bodyCollider; // Collider for taking damage
        [SerializeField] private Collider2D attackRangeCollider; // Collider for attack range checks

        [Header("Debug")]
        [SerializeField] private EnemyState currentState;

        private float currentHealth;
        private Transform playerTarget;
        private Vector3 initialPosition;
        private Rigidbody2D rb;
        private Coroutine attackCoroutine;
        private Coroutine regenerationCoroutine;
        private float leashRadius;

        public event System.Action<float, float> OnHealthChanged; // currentHealth, maxHealth

        private void Awake()
        {
            if (enemyData == null)
            {
                Debug.LogError("EnemyData is not assigned.", this);
                Destroy(gameObject);
                return;
            }
            if (spawnPoint == null)
            {
                Debug.LogError("SpawnPoint is not assigned.", this);
                Destroy(gameObject);
                return;
            }

            rb = GetComponent<Rigidbody2D>();
            initialPosition = transform.position;
            currentHealth = enemyData.maxHealth;
            OnHealthChanged?.Invoke(currentHealth, enemyData.maxHealth); // Initial health update

            CircleCollider2D leashCollider = spawnPoint.GetComponent<CircleCollider2D>();
            if (leashCollider == null)
            {
                Debug.LogError("SpawnPoint does not have a CircleCollider2D for its leash range.", this);
                leashRadius = 20f; // Fallback radius
            }
            else
            {
                leashRadius = leashCollider.radius;
            }
            
            // Find player using the PlayerStats singleton
            if (PlayerStats.Instance != null)
            {
                playerTarget = PlayerStats.Instance.transform;
            }

            SetState(EnemyState.Idle);
        }

        private void Update()
        {
            if (playerTarget == null)
            {
                // If player is defeated or not present, return to spawn
                if (currentState != EnemyState.Returning && currentState != EnemyState.Idle)
                {
                    SetState(EnemyState.Returning);
                }
            }

            switch (currentState)
            {
                case EnemyState.Idle:
                    UpdateIdleState();
                    break;
                case EnemyState.Chasing:
                    UpdateChasingState();
                    break;
                case EnemyState.Attacking:
                    UpdateAttackingState();
                    break;
                case EnemyState.Returning:
                    UpdateReturningState();
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (currentState == EnemyState.Chasing)
            {
                MoveTowards(playerTarget.position);
            }
            else if (currentState == EnemyState.Returning)
            {
                MoveTowards(initialPosition);
            }
            else
            {
                rb.linearVelocity = Vector2.zero; // Stop movement in Idle or Attacking states
            }
        }

        #region State Logic
        private void SetState(EnemyState newState)
        {
            if (currentState == newState) return;

            // --- EXIT LOGIC for old state ---
            // Stop any pending health regeneration when leaving Idle state
            if (currentState == EnemyState.Idle)
            {
                if (regenerationCoroutine != null)
                {
                    StopCoroutine(regenerationCoroutine);
                    regenerationCoroutine = null;
                }
            }
            // Stop attacking when leaving Attacking state
            if (currentState == EnemyState.Attacking)
            {
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            }

            currentState = newState;
            Debug.Log($"{enemyData.enemyName} entering state: {newState}");

            // --- ENTER LOGIC for new state ---
            if (newState == EnemyState.Attacking)
            {
                attackCoroutine = StartCoroutine(AttackLoop());
            }
            else if (newState == EnemyState.Returning)
            {
                bodyCollider.enabled = false;
                attackRangeCollider.enabled = false;
            }
            else if (newState == EnemyState.Idle)
            {
                bodyCollider.enabled = true;
                attackRangeCollider.enabled = true;
                // Start health regeneration process if not at full health
                if (currentHealth < enemyData.maxHealth)
                {
                    regenerationCoroutine = StartCoroutine(RegenerateHealthAfterDelay());
                }
            }
        }

        private IEnumerator RegenerateHealthAfterDelay()
        {
            Debug.Log($"{enemyData.enemyName} will regenerate health in 5 seconds.");
            yield return new WaitForSeconds(5f);

            Debug.Log($"{enemyData.enemyName} has regenerated to full health.");
            currentHealth = enemyData.maxHealth;
            OnHealthChanged?.Invoke(currentHealth, enemyData.maxHealth);
            regenerationCoroutine = null;
        }

        private void UpdateIdleState()
        {
            if (playerTarget == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            if (distanceToPlayer <= enemyData.perceptionRange)
            {
                SetState(EnemyState.Chasing);
            }
        }

        private void UpdateChasingState()
        {
            if (playerTarget == null)
            {
                SetState(EnemyState.Returning);
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            float distanceFromSpawn = Vector2.Distance(transform.position, spawnPoint.position);

            if (distanceFromSpawn > leashRadius)
            {
                SetState(EnemyState.Returning);
            }
            else if (distanceToPlayer <= enemyData.attackRange)
            {
                SetState(EnemyState.Attacking);
            }
        }

        private void UpdateAttackingState()
        {
            if (playerTarget == null)
            {
                SetState(EnemyState.Returning);
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer > enemyData.attackRange)
            {
                SetState(EnemyState.Chasing);
            }
        }

        private void UpdateReturningState()
        {
            float distanceToHome = Vector2.Distance(transform.position, initialPosition);
            if (distanceToHome <= 0.1f)
            {
                SetState(EnemyState.Idle);
            }
        }

        private void MoveTowards(Vector3 targetPosition)
        {
            Vector2 direction = (targetPosition - transform.position).normalized;
            rb.linearVelocity = direction * enemyData.moveSpeed;
        }

        private IEnumerator AttackLoop()
        {
            while (true)
            {
                float attackDelay = 1f / enemyData.attackSpeed;
                yield return new WaitForSeconds(attackDelay);

                if (playerTarget != null)
                {
                    IDamageable playerDamageable = playerTarget.GetComponent<IDamageable>();
                    if (playerDamageable != null)
                    {
                        Debug.Log($"{enemyData.enemyName} is attacking the player for {enemyData.attackPower} damage.");
                        playerDamageable.TakeDamage(enemyData.attackPower);
                    }
                }
            }
        }
        #endregion

        #region Damage & Death
        public void TakeDamage(float amount)
        {
            if (currentState == EnemyState.Returning) return; // Invulnerable while returning

            currentHealth -= amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, enemyData.maxHealth);
            OnHealthChanged?.Invoke(currentHealth, enemyData.maxHealth); // Update health UI
            Debug.Log($"{enemyData.enemyName} took {amount} damage. Current health: {currentHealth}/{enemyData.maxHealth}");

            // If the enemy was idle, it should become hostile and start chasing.
            if (currentState == EnemyState.Idle)
            {
                SetState(EnemyState.Chasing);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"{enemyData.enemyName} has been defeated!");

            // Grant experience to the player using the singleton instance
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.AddExperience(enemyData.xpValue);
            }
            else
            {
                Debug.LogWarning("Could not find PlayerStats.Instance in the scene to grant XP.");
            }

            Destroy(gameObject);
        }
        #endregion

        // --- Usage in Unity ---
        // 1. Create an EnemyData asset and configure its stats (health, xp, speed, ranges).
        // 2. Create a SpawnPoint object in your scene (e.g., an empty GameObject).
        // 3. Add a CircleCollider2D to the SpawnPoint and set its radius to define the enemy's max activity range (leash).
        // 4. Set up the Enemy GameObject with a SpriteRenderer, Rigidbody2D (Dynamic, Gravity Scale 0), and this script.
        // 5. Add TWO CircleCollider2D components to the Enemy:
        //    - One for the body (for taking damage). Assign this to the 'Body Collider' field.
        //    - One for the attack range. Assign this to the 'Attack Range Collider' field.
        // 6. In the Inspector for the Enemy, assign the EnemyData asset and the SpawnPoint object.
        // 7. The perception range is now handled by code, not a collider. The attack range is also code-driven, but the collider is used for disabling/enabling.
        // 8. Ensure the Player GameObject has the "PlayerStats" script and a Collider2D.
    }
}
