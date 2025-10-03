using UnityEngine;
using System;

namespace ChromaticCascade.Scoring
{
    /// <summary>
    /// Manages score tracking, combo multipliers, and high scores
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Score Settings")]
        [SerializeField] private int basePointsPerBlock = 10;
        [SerializeField] private int[] evolutionBonuses = { 50, 100, 500, 1000, 2000 }; // Tier 1→2 through Tier 4→5

        [Header("Combo Settings")]
        [SerializeField] private float comboDecayTime = 3f;
        [SerializeField] private float[] comboMultipliers = { 1f, 2f, 3f, 5f, 7f, 10f }; // x1 to x10 for combos 0-5+

        private int currentScore = 0;
        private int highScore = 0;
        private int comboCount = 0;
        private float comboTimer = 0f;
        private bool comboActive = false;

        // Events
        public event Action<int, int> OnScoreChanged; // newScore, deltaScore
        public event Action<int, float> OnComboChanged; // comboCount, multiplier
        public event Action OnComboBreak;
        public event Action<int> OnHighScoreBeaten; // newHighScore

        public int CurrentScore => currentScore;
        public int HighScore => highScore;
        public int ComboCount => comboCount;
        public float ComboMultiplier => GetComboMultiplier();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            LoadHighScore();
        }

        private void Update()
        {
            // Handle combo decay
            if (comboActive)
            {
                comboTimer -= Time.deltaTime;
                
                if (comboTimer <= 0f)
                {
                    ResetCombo();
                }
            }
        }

        /// <summary>
        /// Add points for block placement
        /// </summary>
        public void AddBlockPlacementPoints()
        {
            AddScore(basePointsPerBlock, false);
        }

        /// <summary>
        /// Add points for evolution
        /// </summary>
        public void AddEvolutionPoints(int fromTier, int toTier, int blocksEvolved)
        {
            // Base evolution bonus
            int tierIndex = Mathf.Clamp(fromTier - 1, 0, evolutionBonuses.Length - 1);
            int baseBonus = evolutionBonuses[tierIndex];

            // Bonus for number of blocks evolved
            int blockBonus = blocksEvolved * 10;

            // Apply combo multiplier
            int totalPoints = Mathf.RoundToInt((baseBonus + blockBonus) * GetComboMultiplier());

            AddScore(totalPoints, true);

            // Increment combo
            IncrementCombo();

            Debug.Log($"[ScoreManager] Evolution: Tier {fromTier}→{toTier}, {blocksEvolved} blocks, +{totalPoints} points (x{GetComboMultiplier()} combo)");
        }

        /// <summary>
        /// Add points for ability usage
        /// </summary>
        public void AddAbilityPoints(int abilityPoints, int blocksAffected)
        {
            int totalPoints = abilityPoints + (blocksAffected * 10);
            AddScore(totalPoints, false);
            
            Debug.Log($"[ScoreManager] Ability: +{totalPoints} points ({blocksAffected} blocks affected)");
        }

        /// <summary>
        /// Add score with optional combo multiplier
        /// </summary>
        private void AddScore(int points, bool applyCombo)
        {
            int actualPoints = applyCombo ? Mathf.RoundToInt(points * GetComboMultiplier()) : points;
            
            int oldScore = currentScore;
            currentScore += actualPoints;

            OnScoreChanged?.Invoke(currentScore, actualPoints);

            // Check for high score
            if (currentScore > highScore)
            {
                int oldHighScore = highScore;
                highScore = currentScore;
                SaveHighScore();

                if (oldHighScore > 0) // Only fire event if there was a previous high score
                {
                    OnHighScoreBeaten?.Invoke(highScore);
                    Debug.Log($"[ScoreManager] 🎉 NEW HIGH SCORE: {highScore}!");
                }
            }
        }

        /// <summary>
        /// Increment combo counter
        /// </summary>
        private void IncrementCombo()
        {
            comboCount++;
            comboActive = true;
            comboTimer = comboDecayTime;

            OnComboChanged?.Invoke(comboCount, GetComboMultiplier());

            Debug.Log($"[ScoreManager] Combo x{comboCount} (multiplier: x{GetComboMultiplier()})");
        }

        /// <summary>
        /// Reset combo counter
        /// </summary>
        private void ResetCombo()
        {
            if (comboCount > 0)
            {
                Debug.Log($"[ScoreManager] Combo broken at x{comboCount}");
                OnComboBreak?.Invoke();
            }

            comboCount = 0;
            comboActive = false;
            comboTimer = 0f;

            OnComboChanged?.Invoke(0, 1f);
        }

        /// <summary>
        /// Get current combo multiplier based on combo count
        /// </summary>
        private float GetComboMultiplier()
        {
            int index = Mathf.Min(comboCount, comboMultipliers.Length - 1);
            return comboMultipliers[index];
        }

        /// <summary>
        /// Reset score for new game
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            ResetCombo();
            
            OnScoreChanged?.Invoke(0, 0);
            
            Debug.Log("[ScoreManager] Score reset");
        }

        /// <summary>
        /// Load high score from PlayerPrefs
        /// </summary>
        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt("HighScore", 0);
            Debug.Log($"[ScoreManager] High Score loaded: {highScore}");
        }

        /// <summary>
        /// Save high score to PlayerPrefs
        /// </summary>
        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Get score stats for display
        /// </summary>
        public ScoreStats GetScoreStats()
        {
            return new ScoreStats
            {
                currentScore = currentScore,
                highScore = highScore,
                comboCount = comboCount,
                comboMultiplier = GetComboMultiplier()
            };
        }
    }

    /// <summary>
    /// Score statistics data structure
    /// </summary>
    [System.Serializable]
    public struct ScoreStats
    {
        public int currentScore;
        public int highScore;
        public int comboCount;
        public float comboMultiplier;
    }
}
