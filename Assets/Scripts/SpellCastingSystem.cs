using UnityEngine;
using System;
using System.Collections;
using Poc4.Spells; // For ProjectileSpell
using Poc4.Player; // For PlayerStats

public class SpellCastingSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerController playerController; // For movement input detection
    private PlayerStats playerStats; // Reference to the player's stats

    [Header("Casting Settings")]
    [SerializeField] private float castingLockDuration = 0.5f; // Input lock duration at the start of casting

    private SpellData currentSpellData; // The spell currently being cast
    private bool isCasting = false;
    private int currentCircle = 0;
    private float currentCircleCastTime = 0f;
    private float totalCastingTime = 0f;

    public event Action<int, float, float> OnCastingProgress; // currentCircle, currentCircleTime, totalCircleTime
    public event Action<int> OnCircleCompleted; // circleNumber
    public event Action OnCastingStarted;
    public event Action OnCastingInterrupted;
    public event Action<int> OnSpellFired; // firedCircleCount

    private Coroutine castingCoroutine;
    private Coroutine inputLockCoroutine;

    public bool IsCasting => isCasting;
    public bool IsInputLocked { get; private set; } = false;
    public int CurrentCircle => currentCircle;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("SpellCastingSystem: PlayerController not found on this GameObject or assigned.", this);
            }
        }

        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("SpellCastingSystem: PlayerStats not found on this GameObject.", this);
        }
    }

    public void StartCasting(SpellData spellData)
    {
        if (isCasting || IsInputLocked) return;

        currentSpellData = spellData;
        if (currentSpellData == null || currentSpellData.castingTimes == null || currentSpellData.castingTimes.Length == 0)
        {
            Debug.LogWarning("SpellCastingSystem: No spell data or casting times defined for casting.", this);
            return;
        }

        isCasting = true;
        currentCircle = 0;
        currentCircleCastTime = 0f;
        totalCastingTime = 0f;

        OnCastingStarted?.Invoke();
        inputLockCoroutine = StartCoroutine(HandleCastingLock());
        castingCoroutine = StartCoroutine(CastingProcess());
        Debug.Log($"Casting started for {currentSpellData.spellName}");
    }

    private IEnumerator HandleCastingLock()
    {
        IsInputLocked = true;
        yield return new WaitForSeconds(castingLockDuration);
        IsInputLocked = false;
    }

    private IEnumerator CastingProcess()
    {
        // Wait for the initial input lock to finish before starting the actual casting timer
        yield return new WaitUntil(() => !IsInputLocked);

        while (currentCircle < currentSpellData.castingTimes.Length && isCasting)
        {
            float baseCastTime = currentSpellData.castingTimes[currentCircle];
            // Apply the casting time multiplier from PlayerStats
            float targetCircleTime = baseCastTime * (playerStats != null ? playerStats.CastingTimeMultiplier : 1f);
            
            currentCircleCastTime = 0f;

            while (currentCircleCastTime < targetCircleTime && isCasting)
            {
                currentCircleCastTime += Time.deltaTime;
                totalCastingTime += Time.deltaTime;
                OnCastingProgress?.Invoke(currentCircle + 1, currentCircleCastTime, targetCircleTime);
                yield return null;
            }

            if (!isCasting) // Interrupted during the current circle
            {
                yield break;
            }

            currentCircle++;
            OnCircleCompleted?.Invoke(currentCircle);
            Debug.Log($"Circle {currentCircle} completed for {currentSpellData.spellName}");

            if (currentCircle >= currentSpellData.castingTimes.Length)
            {
                Debug.Log($"All circles completed for {currentSpellData.spellName}. Ready to fire.");
            }
        }
    }

    public void FireSpell()
    {
        if (!isCasting || IsInputLocked) return;

        if (currentCircle > 0)
        {
            if (currentSpellData.projectilePrefab != null)
            {
                // Capture data before resetting state, so the coroutine can use it
                SpellData spellToFire = currentSpellData;
                int circlesOfFire = currentCircle;

                int circleIndex = circlesOfFire - 1;
                int projectileCount = 1;
                if (spellToFire.projectileCounts != null && circleIndex < spellToFire.projectileCounts.Length)
                {
                    projectileCount = spellToFire.projectileCounts[circleIndex];
                }

                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // Pass mousePos as targetWorldPosition
                StartCoroutine(FireSequentialProjectiles(spellToFire, circlesOfFire, projectileCount, mousePos));

                OnSpellFired?.Invoke(circlesOfFire);
                Debug.Log($"Spell {spellToFire.spellName} firing {projectileCount} projectile(s) sequentially...");
            }
            else
            {
                Debug.LogWarning($"Spell {currentSpellData.spellName} has no projectile prefab assigned.");
            }
            
            // Reset casting state immediately so the player can move while projectiles are firing.
            ResetCastingState();
        }
    }

    private IEnumerator FireSequentialProjectiles(SpellData spell, int circles, int count, Vector2 targetWorldPosition)
    {
        float totalDuration = 1.0f;
        // If only one projectile, fire it instantly (delay = 0)
        float delay = (count > 1) ? totalDuration / count : 0;

        for (int i = 0; i < count; i++)
        {
            ProjectileSpell projectile = Instantiate(spell.projectilePrefab, transform.position, Quaternion.identity).GetComponent<ProjectileSpell>();
            projectile.Initialize(spell, circles, targetWorldPosition); // Pass targetWorldPosition

            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
        }
    }

    public void InterruptCasting()
    {
        if (!isCasting) return;

        Debug.Log("Casting interrupted.");
        OnCastingInterrupted?.Invoke();
        ResetCastingState();
    }

    private void ResetCastingState()
    {
        if (castingCoroutine != null)
        {
            StopCoroutine(castingCoroutine);
            castingCoroutine = null;
        }
        if (inputLockCoroutine != null)
        {
            StopCoroutine(inputLockCoroutine);
            inputLockCoroutine = null;
        }

        IsInputLocked = false;
        isCasting = false;
        currentCircle = 0;
        currentCircleCastTime = 0f;
        totalCastingTime = 0f;
        currentSpellData = null;
    }

    // --- Usage in Unity ---
    // 1. Attach this script to the Player GameObject.
    // 2. Ensure the Player GameObject also has the PlayerStats and PlayerController scripts.
    // 3. Create SpellData ScriptableObjects (Assets -> Create -> Poc4/Spells/SpellData).
    // 4. In each SpellData asset, assign a Projectile Prefab.
    // 5. PlayerController is responsible for selecting a spell and calling StartCasting.
    // 6. PlayerController calls FireSpell or InterruptCasting based on player input.
}

