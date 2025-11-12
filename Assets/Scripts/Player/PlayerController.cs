using UnityEngine;
using Poc4.Spells;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Spell Casting")]
    [SerializeField] private SpellCastingSystem spellCastingSystem;
    [SerializeField] private SpellData[] assignedSpells = new SpellData[4]; // Assign spells for keys 1-4 in the Inspector

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isMoving = false;
    private SpellData currentSelectedSpell;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerController: Rigidbody2D not found on this GameObject.", this);
        }

        if (spellCastingSystem == null)
        {
            spellCastingSystem = GetComponent<SpellCastingSystem>();
            if (spellCastingSystem == null)
            {
                Debug.LogError("PlayerController: SpellCastingSystem not found on this GameObject or assigned.", this);
            }
        }

        // Set the first spell as the default selected one
        if (assignedSpells.Length > 0 && assignedSpells[0] != null)
        {
            currentSelectedSpell = assignedSpells[0];
            Debug.Log($"Spell '{currentSelectedSpell.spellName}' selected by default.");
        }
    }

    private void Update()
    {
        // The IsCasting flag now prevents most actions
        if (spellCastingSystem.IsCasting)
        {
            moveInput = Vector2.zero; // Ensure no movement input is processed while casting
        }
        else
        {
            // Only handle selection and movement if not casting
            HandleSpellSelection();
            HandleMovementInput();
        }

        HandleCastingInput();

        // Interrupt casting if player moves while casting
        if (spellCastingSystem.IsCasting && isMoving)
        {
            spellCastingSystem.InterruptCasting();
        }
    }

    private void HandleSpellSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSpell(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSpell(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSpell(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSpell(3);
    }

    private void SelectSpell(int index)
    {
        // Check if the selected spell is valid first
        if (index >= assignedSpells.Length || assignedSpells[index] == null)
        {
            Debug.LogWarning($"No spell assigned to key {index + 1}.");
            return;
        }

        // If casting, interrupt the current cast before selecting the new one.
        if (spellCastingSystem.IsCasting)
        {
            spellCastingSystem.InterruptCasting();
        }

        // Select the new spell.
        currentSelectedSpell = assignedSpells[index];
        Debug.Log($"Spell '{currentSelectedSpell.spellName}' selected with key {index + 1}.");
    }

    private void HandleMovementInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        isMoving = moveInput.magnitude > 0.1f;
    }

    private void HandleCastingInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            if (currentSelectedSpell != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                spellCastingSystem.AttemptToCast(currentSelectedSpell, mousePos);
            }
            else
            {
                Debug.LogWarning("No spell selected to cast.");
            }
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Only allow movement if not casting
        if (!spellCastingSystem.IsCasting)
        {
            rb.linearVelocity = moveInput.normalized * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // Stop movement if casting
        }
    }

    // --- Usage in Unity ---
    // 1. Attach this script and SpellCastingSystem.cs to the "Player" GameObject.
    // 2. Ensure the Player has a Rigidbody2D and a Collider2D.
    // 3. In the Inspector for PlayerController:
    //    - Assign the SpellCastingSystem component to its field.
    //    - Create and assign up to 4 SpellData assets to the 'Assigned Spells' array.
    // 4. Press keys 1-4 to select a spell, and left-click to cast/fire.
}