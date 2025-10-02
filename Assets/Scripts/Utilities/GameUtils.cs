using UnityEngine;

namespace ChromaticCascade.Utilities
{
    /// <summary>
    /// Utility class for common helper functions
    /// </summary>
    public static class GameUtils
    {
        /// <summary>
        /// Remap a value from one range to another
        /// </summary>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }
        
        /// <summary>
        /// Get a random color from the game's color palette
        /// </summary>
        public static Color GetRandomPaletteColor()
        {
            Color[] palette = {
                new Color32(255, 75, 75, 255),   // Red
                new Color32(75, 138, 255, 255),  // Blue
                new Color32(68, 208, 122, 255),  // Green
                new Color32(247, 201, 72, 255),  // Yellow
                new Color32(160, 91, 255, 255)   // Purple
            };
            
            return palette[Random.Range(0, palette.Length)];
        }
        
        /// <summary>
        /// Format time in MM:SS format
        /// </summary>
        public static string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
        
        /// <summary>
        /// Format large numbers with abbreviations (K, M, B)
        /// </summary>
        public static string FormatNumber(long number)
        {
            if (number < 1000)
                return number.ToString();
            
            if (number < 1000000)
                return $"{number / 1000f:F1}K";
            
            if (number < 1000000000)
                return $"{number / 1000000f:F1}M";
            
            return $"{number / 1000000000f:F1}B";
        }
        
        /// <summary>
        /// Smooth damp a float value
        /// </summary>
        public static float SmoothDamp(float current, float target, ref float velocity, float smoothTime)
        {
            return Mathf.SmoothDamp(current, target, ref velocity, smoothTime);
        }
        
        /// <summary>
        /// Check if device is low-end for performance adjustments
        /// </summary>
        public static bool IsLowEndDevice()
        {
            // Simple heuristic based on system memory
            return SystemInfo.systemMemorySize < 4096; // Less than 4GB RAM
        }
        
        /// <summary>
        /// Get safe area insets for notched displays
        /// </summary>
        public static Rect GetSafeArea()
        {
            return Screen.safeArea;
        }
    }
}
