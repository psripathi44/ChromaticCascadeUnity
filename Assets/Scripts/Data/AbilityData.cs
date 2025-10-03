using UnityEngine;

namespace ChromaticCascade.Data
{
    /// <summary>
    /// Defines tier-specific abilities and their properties
    /// </summary>
    [CreateAssetMenu(fileName = "AbilityData", menuName = "Chromatic Cascade/Data/Ability Data")]
    public class AbilityData : ScriptableObject
    {
        [Header("Ability Identity")]
        public AbilityType abilityType;
        public string abilityName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Parameters")]
        [Tooltip("Duration in seconds (for timed abilities like Time Freeze)")]
        public float duration = 0f;
        
        [Tooltip("Radius or range of effect (for area abilities)")]
        public int effectRadius = 0;
        
        [Header("Visual Effects")]
        public GameObject vfxPrefab;
        public Color abilityColor = Color.white;
        public Sprite abilityIcon;
        
        [Header("Audio")]
        public AudioClip triggerSound;
        public AudioClip loopSound;
        public AudioClip endSound;
        
        [Header("Scoring")]
        [Tooltip("Base points awarded when ability triggers")]
        public int basePoints = 0;
        
        [Tooltip("Points per block affected")]
        public int pointsPerBlock = 0;
    }
}
