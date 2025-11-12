using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Poc4.Combat; // To access Enemy

namespace Poc4.UI
{
    public class EnemyHealthUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private RectTransform fillRect; // The RectTransform of the fill image
        [SerializeField] private Canvas healthCanvas;

        [Header("Settings")]
        [SerializeField] private float hideDelay = 3f;

        private Enemy enemy;
        private Coroutine hideCoroutine;
        private Transform mainCameraTransform;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogError("EnemyHealthUI: Enemy component not found on this GameObject.", this);
                gameObject.SetActive(false);
                return;
            }

            if (healthCanvas != null)
            {
                healthCanvas.gameObject.SetActive(false);
            }

            mainCameraTransform = Camera.main.transform;
        }

        private void Start()
        {
            enemy.OnHealthChanged += UpdateHealthBar;
        }

        private void OnDestroy()
        {
            if (enemy != null)
            {
                enemy.OnHealthChanged -= UpdateHealthBar;
            }
        }

        private void LateUpdate()
        {
            // Make the health bar always face the camera (billboarding)
            if (healthCanvas != null && healthCanvas.gameObject.activeInHierarchy)
            {
                healthCanvas.transform.rotation = mainCameraTransform.rotation;
            }
        }

        private void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            if (fillRect != null)
            {
                float fillAmount = (maxHealth > 0) ? currentHealth / maxHealth : 0;
                fillRect.localScale = new Vector3(fillAmount, 1, 1);
            }

            // Show the health bar and start/reset the hide timer
            if (healthCanvas != null)
            {
                healthCanvas.gameObject.SetActive(true);
                if (hideCoroutine != null)
                {
                    StopCoroutine(hideCoroutine);
                }
                // Don't hide the health bar if the enemy is dead
                if (currentHealth > 0)
                {
                    hideCoroutine = StartCoroutine(HideHealthBarAfterDelay());
                }
            }
        }

        private IEnumerator HideHealthBarAfterDelay()
        {
            yield return new WaitForSeconds(hideDelay);
            if (healthCanvas != null)
            {
                healthCanvas.gameObject.SetActive(false);
            }
        }

        // --- Usage in Unity ---
        // 1. Edit your Enemy prefab.
        // 2. Add a new "Canvas" GameObject as a child of the Enemy.
        //    - In the Canvas component, set Render Mode to "World Space".
        //    - Set the Rect Transform of the Canvas to a small size (e.g., Width 100, Height 20) and position it above the enemy's sprite.
        // 3. Add a UI Image (e.g., "HealthBarBackground") as a child of the Canvas.
        // 4. As a child of "HealthBarBackground", add another UI Image (e.g., "HealthBarFill"). This will be the fillRect.
        // 5. Attach this EnemyHealthUI.cs script to the root of the Enemy prefab.
        // 6. In the Inspector for EnemyHealthUI:
        //    - Drag the child "Canvas" to the 'Health Canvas' field.
        //    - Drag the "HealthBarFill" RectTransform to the 'Fill Rect' field.
        // 7. Ensure the "HealthBarFill" RectTransform's pivot is set to (0, 0.5) so it scales from left to right.
    }
}