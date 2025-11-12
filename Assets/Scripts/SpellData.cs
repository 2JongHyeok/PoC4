using UnityEngine;
using Poc4.Spells; // Add this namespace

[CreateAssetMenu(menuName = "Poc4/Spells/SpellData", fileName = "NewSpellData")] // Updated menu path
public class SpellData : ScriptableObject
{
    public string spellName;
    public Sprite icon;
    public float[] castingTimes; // e.g., [3, 5, 7, ...]

    [Header("Spell Effects & Scaling")]
    public SpellEffect spellEffect; // Reference to the base spell effect
    public float[] damages; // Damage per circle level (index 0 = 1st circle)

    [Tooltip("Optional: Area of Effect radius per circle. Used by explosion logic.")]
    public float[] aoeRadiuses; 

    [Tooltip("Optional: Scale of the projectile prefab per circle. 1 is default size.")]
    public float[] projectileScales;

    [Tooltip("Optional: Number of projectiles to fire per circle.")]
    public int[] projectileCounts;

    public GameObject projectilePrefab;
}
