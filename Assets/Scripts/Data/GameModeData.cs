using UnityEngine;
using System.Collections.Generic;

namespace ChromaticCascade.Data
{
    /// <summary>
    /// Defines game mode configurations with rules, scoring, and presentation
    /// </summary>
    [CreateAssetMenu(fileName = "GameModeData", menuName = "Chromatic Cascade/Data/Game Mode Data")]
    public class GameModeData : ScriptableObject
    {
        [Header("Mode Identity")]
        public GameMode modeType;
        public string modeName;
        [TextArea(3, 5)]
        public string modeDescription;
        public Sprite modeIcon;
        
        [Header("Spawn Settings")]
        [Tooltip("Base time between block spawns (seconds)")]
        public float baseSpawnInterval = 1.0f;
        
        [Tooltip("Speed curve: Score (X) -> Speed Multiplier (Y)")]
        public AnimationCurve spawnSpeedCurve = AnimationCurve.Linear(0, 1, 10000, 2.5f);
        
        [Tooltip("Allow random color spawning")]
        public bool allowRandomColorSpawn = true;
        
        [Header("Scoring Settings")]
        [Tooltip("Global score multiplier for this mode")]
        public float scoreMultiplier = 1.0f;
        
        [Tooltip("Enable combo system")]
        public bool enableComboSystem = true;
        
        [Tooltip("Maximum combo multiplier (0 = unlimited)")]
        public int maxComboMultiplier = 10;
        
        [Header("Win/Loss Conditions")]
        public WinConditionType winCondition = WinConditionType.None;
        
        [Tooltip("Target score for win (if applicable)")]
        public int targetScore = 0;
        
        [Tooltip("Time limit in seconds (if applicable)")]
        public float timeLimit = 0f;
        
        [Header("Gameplay Modifiers")]
        [Tooltip("Enable power-ups")]
        public bool enablePowerUps = true;
        
        [Tooltip("Enable tier abilities")]
        public bool enableAbilities = true;
        
        [Tooltip("Difficulty multiplier")]
        [Range(0.5f, 3f)]
        public float difficultyMultiplier = 1.0f;
        
        [Header("UI Theme")]
        public Color themeColor = new Color(0.2f, 0.6f, 1f);
        public Sprite backgroundSprite;
        public AudioClip musicTrack;
    }
    
    public enum GameMode
    {
        Classic,      // Endless mode with increasing difficulty
        Challenge,    // Puzzle mode with specific goals
        TimeAttack,   // 3-minute timed mode
        Zen          // Relaxed, slow-paced mode
    }
    
    public enum WinConditionType
    {
        None,         // No win condition (endless or manual)
        Score,        // Reach target score
        Time,         // Survive for X time
        Goal          // Complete specific objectives
    }
}
