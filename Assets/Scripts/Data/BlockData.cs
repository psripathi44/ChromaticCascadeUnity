using UnityEngine;

namespace ChromaticCascade.Data
{
    /// <summary>
    /// Combines ColorData and TierData to define a specific block type
    /// </summary>
    [CreateAssetMenu(fileName = "BlockData", menuName = "Chromatic Cascade/Data/Block Data")]
    public class BlockData : ScriptableObject
    {
        [Header("Block Identity")]
        public ColorData colorData;
        public TierData tierData;
        
        [Header("Visuals")]
        public Sprite blockSprite;
        public GameObject blockPrefab;
        
        [Header("Evolution")]
        [Tooltip("Minimum connected blocks required to evolve (default: 4)")]
        public int minBlocksToEvolve = 4;
        
        [Tooltip("Can form 2x2 square evolution")]
        public bool allows2x2Evolution = true;
        
        [Header("Display")]
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        
        /// <summary>
        /// Returns unique identifier for this block type (e.g., "Red_Tier1")
        /// </summary>
        public string GetBlockID()
        {
            if (colorData == null || tierData == null)
                return "Unknown";
            
            return $"{colorData.colorType}_Tier{tierData.tierLevel}";
        }
        
        /// <summary>
        /// Returns the calculated color for rendering (base color * tier brightness)
        /// </summary>
        public Color GetRenderColor()
        {
            if (colorData == null || tierData == null)
                return Color.white;
            
            Color baseColor = colorData.baseColor;
            float brightness = tierData.brightnessMultiplier;
            
            return new Color(
                Mathf.Min(baseColor.r * brightness, 1f),
                Mathf.Min(baseColor.g * brightness, 1f),
                Mathf.Min(baseColor.b * brightness, 1f),
                baseColor.a
            );
        }
    }
}
