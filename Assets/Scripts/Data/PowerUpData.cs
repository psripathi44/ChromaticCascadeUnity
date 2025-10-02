using UnityEngine;

namespace ChromaticCascade.Data
{
    /// <summary>
    /// Defines power-up items that players can use during gameplay
    /// </summary>
    [CreateAssetMenu(fileName = "PowerUpData", menuName = "Chromatic Cascade/Data/Power-Up Data")]
    public class PowerUpData : ScriptableObject
    {
        [Header("Power-Up Identity")]
        public PowerUpType powerUpType;
        public string powerUpName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Economy")]
        [Tooltip("Cost in premium currency (Prisms)")]
        public int prismCost = 50;
        
        [Tooltip("Can be purchased with currency")]
        public bool purchasable = true;
        
        [Header("Visuals")]
        public Sprite icon;
        public Color iconTintColor = Color.white;
        
        [Header("Cooldown")]
        [Tooltip("Cooldown in seconds between uses (0 = no cooldown)")]
        public float cooldownSeconds = 0f;
        
        [Header("Audio")]
        public AudioClip useSound;
    }
    
    public enum PowerUpType
    {
        ColorShifter,       // Change block color
        Reversal,           // Undo last move
        BlockBreaker,       // Remove single block
        TimeFreeze,         // Pause falling (10s)
        EvolutionCatalyst   // Upgrade any block by one tier
    }
}
