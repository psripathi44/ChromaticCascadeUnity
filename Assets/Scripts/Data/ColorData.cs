using UnityEngine;

namespace ChromaticCascade.Data
{
    /// <summary>
    /// Defines a color used in the game with its visual and audio properties
    /// </summary>
    [CreateAssetMenu(fileName = "ColorData", menuName = "Chromatic Cascade/Data/Color Data")]
    public class ColorData : ScriptableObject
    {
        [Header("Color Identity")]
        public ColorType colorType;
        public Color32 baseColor = Color.white;
        
        [Header("Accessibility")]
        [Tooltip("Pattern overlay for color-blind mode")]
        public AccessibilityPattern pattern = AccessibilityPattern.None;
        
        [Header("Audio")]
        [Tooltip("Sound played when this color is matched/placed")]
        public AudioClip colorSound;
        
        [Header("Visual")]
        public Sprite colorIcon;
    }
    
    public enum ColorType
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple
    }
    
    public enum AccessibilityPattern
    {
        None,
        Stripes,
        Dots,
        Chevrons,
        Diamonds,
        Waves
    }
}
