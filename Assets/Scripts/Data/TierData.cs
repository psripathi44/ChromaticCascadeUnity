using UnityEngine;

namespace ChromaticCascade.Data
{
    /// <summary>
    /// Defines tier progression levels (1-5) with visual and gameplay properties
    /// </summary>
    [CreateAssetMenu(fileName = "TierData", menuName = "Chromatic Cascade/Data/Tier Data")]
    public class TierData : ScriptableObject
    {
        [Header("Tier Identity")]
        [Range(1, 5)]
        public int tierLevel = 1;
        
        [Header("Visual Properties")]
        [Tooltip("Brightness multiplier for this tier (higher = brighter)")]
        [Range(1.0f, 2.5f)]
        public float brightnessMultiplier = 1.0f;
        
        [Tooltip("Glow intensity for this tier")]
        [Range(0f, 2f)]
        public float glowIntensity = 0f;
        
        [Header("Effects")]
        [Tooltip("Particle effect prefab for this tier (optional)")]
        public GameObject particleEffectPrefab;
        
        [Tooltip("Evolution particle effect when creating this tier")]
        public GameObject evolutionVFXPrefab;
        
        [Header("Ability")]
        [Tooltip("Ability type triggered when this tier evolves (Tier 2-5)")]
        public AbilityType abilityType = AbilityType.None;
        
        [Header("Scoring")]
        [Tooltip("Score multiplier for evolutions involving this tier")]
        public float scoreMultiplier = 1.0f;
        
        [Tooltip("Base evolution bonus when creating this tier")]
        public int evolutionBonus = 0;
    }
    
    public enum AbilityType
    {
        None,           // Tier 1 - no ability
        RowClear,       // Tier 2
        ColorBomb,      // Tier 3
        Cascade,        // Tier 4
        TimeFreeze      // Tier 5
    }
}
