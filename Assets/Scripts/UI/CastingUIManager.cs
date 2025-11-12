using UnityEngine;
using UnityEngine.UI;
using TMPro; // Using TextMeshPro for better text rendering

public class CastingUIManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SpellCastingSystem spellCastingSystem;

    [Header("UI Elements")]
    [SerializeField] private GameObject castingUIParent; // The parent object containing all casting UI
    [SerializeField] private RectTransform fillRect; // The RectTransform of the slider's "Fill" image
    [SerializeField] private TextMeshProUGUI circleText; // Text to display "Circle X"

    private void Start()
    {
        // Ensure SpellCastingSystem is assigned in the Inspector.
        // If not assigned, a NullReferenceException will occur, indicating a setup error.
        SubscribeToEvents();
        
        // Initially hide the UI
        if (castingUIParent != null)
        {
            castingUIParent.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        // Ensure spellCastingSystem is not null before subscribing
        if (spellCastingSystem != null)
        {
            spellCastingSystem.OnCastingStarted += HandleCastingStarted;
            spellCastingSystem.OnCastingProgress += HandleCastingProgress;
            spellCastingSystem.OnCircleCompleted += HandleCircleCompleted;
            spellCastingSystem.OnCastingInterrupted += HandleCastingFinished;
            spellCastingSystem.OnSpellFired += (firedCircle) => HandleCastingFinished();
        }
        else
        {
            Debug.LogError("CastingUIManager: SpellCastingSystem is not assigned. Please assign it in the Inspector.", this);
            this.enabled = false; // Disable this component if essential dependency is missing
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (spellCastingSystem != null)
        {
            spellCastingSystem.OnCastingStarted -= HandleCastingStarted;
            spellCastingSystem.OnCastingProgress -= HandleCastingProgress;
            spellCastingSystem.OnCircleCompleted -= HandleCircleCompleted;
            spellCastingSystem.OnCastingInterrupted -= HandleCastingFinished;
            spellCastingSystem.OnSpellFired -= (firedCircle) => HandleCastingFinished();
        }
    }

    private void HandleCastingStarted()
    {
        if (castingUIParent != null)
        {
            castingUIParent.SetActive(true);
        }
        UpdateCircleText(0); // Show 0 for completed circles at the start
        UpdateSlider(0, 1); // Reset slider at the beginning
    }

    private void HandleCastingProgress(int currentCircle, float currentCircleTime, float totalCircleTime)
    {
        UpdateSlider(currentCircleTime, totalCircleTime);
    }

    private void HandleCircleCompleted(int completedCircle)
    {
        // Show the number of the circle that was just completed
        UpdateCircleText(completedCircle);
        UpdateSlider(0, 1); // Reset for the next circle
    }

    private void HandleCastingFinished()
    {
        if (castingUIParent != null)
        {
            castingUIParent.SetActive(false);
        }
        UpdateSlider(0, 1);
    }

    private void UpdateCircleText(int circleNumber)
    {
        if (circleText != null)
        {
            circleText.text = $"{circleNumber}";
        }
    }

    private void UpdateSlider(float currentValue, float maxValue)
    {
        if (fillRect != null)
        {
            float fillAmount = (maxValue > 0) ? currentValue / maxValue : 0;
            fillRect.localScale = new Vector3(fillAmount, 1, 1);
        }
    }

    // --- Usage in Unity ---
    // 1. Create a new empty GameObject in your scene and name it "UIManager".
    // 2. Attach this CastingUIManager.cs script to the "UIManager" GameObject.
    // 3. Create a Canvas in your scene (if you don't have one).
    // 4. Inside the Canvas, create a UI structure:
    //    - An empty GameObject named "CastingUIParent".
    //    - Inside "CastingUIParent", add a "Slider" UI element.
    //    - Inside "CastingUIParent", add a "Text - TextMeshPro" UI element.
    // 5. In the "UIManager" GameObject's Inspector:
    //    - IMPORTANT: Drag the "Player" GameObject (or wherever SpellCastingSystem is) to the 'Spell Casting System' field. This field MUST be assigned.
    //    - Drag the "CastingUIParent" GameObject to the 'Casting UI Parent' field.
    //    - In the Hierarchy, expand the "Slider" to find its "Fill Area" -> "Fill" child object. Drag the "Fill" object to the 'Fill Rect' field.
    //    - Drag the "Text - TextMeshPro" to the 'Circle Text' field.
    // 6. In the Hierarchy, select the "Fill" object and in its RectTransform, set the pivot to (0, 0.5). This makes it scale from the left.
    // 7. Ensure the scene is running to test the UI updates during casting.
}
