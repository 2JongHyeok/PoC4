using UnityEngine;
using Poc4.Spells; // Add this namespace

[CreateAssetMenu(menuName = "Poc4/Spells/SpellData", fileName = "NewSpellData")] // Updated menu path
public class SpellData : ScriptableObject
{
    [Header("Core Info")]
    public string spellName;
    public Sprite icon;

    [Header("Requirements & Timers")]
    [Tooltip("The player's circle level must be >= this value to cast.")]
    public int requiredCircle = 1;
    [Tooltip("Time in seconds it takes to cast this spell.")]
    public float castingTime = 1.5f;
    [Tooltip("Time in seconds before this spell can be cast again.")]
    public float cooldown = 5f;
    [Tooltip("Mana required to cast this spell.")]
    public float manaCost = 10f;

    [Header("Spell Properties")]
    public float damage = 10f;
    [Tooltip("Set to 0 for single-target spells.")]
    public float aoeRadius = 0f;
    public float projectileScale = 1f;
    public int projectileCount = 1;
    
    [Header("Prefab")]
    public GameObject projectilePrefab;
}

