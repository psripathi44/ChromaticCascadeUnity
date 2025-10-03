using System.Collections.Generic;
using UnityEngine;

namespace ChromaticCascade.Evolution
{
    /// <summary>
    /// Represents a detected evolution pattern (4+ connected or 2x2 square)
    /// </summary>
    public class EvolutionPattern
    {
        public enum PatternType
        {
            Linear4Plus,    // 4+ connected blocks (horizontal/vertical)
            Square2x2       // 2x2 square pattern
        }

        public PatternType Type { get; private set; }
        public List<Gameplay.Block> Blocks { get; private set; }
        public Vector2Int CenterPosition { get; private set; }
        public Data.ColorData Color { get; private set; }
        public Data.TierData Tier { get; private set; }

        public EvolutionPattern(PatternType type, List<Gameplay.Block> blocks, Data.ColorData color, Data.TierData tier)
        {
            Type = type;
            Blocks = new List<Gameplay.Block>(blocks);
            Color = color;
            Tier = tier;
            CenterPosition = CalculateCenter();
        }

        /// <summary>
        /// Calculate the center position for spawning the evolved block
        /// </summary>
        private Vector2Int CalculateCenter()
        {
            if (Blocks == null || Blocks.Count == 0)
                return Vector2Int.zero;

            // Calculate average position
            Vector2 sum = Vector2.zero;
            foreach (var block in Blocks)
            {
                sum += new Vector2(block.GridPosition.x, block.GridPosition.y);
            }

            Vector2 average = sum / Blocks.Count;
            return new Vector2Int(Mathf.RoundToInt(average.x), Mathf.RoundToInt(average.y));
        }

        /// <summary>
        /// Get the score value for this pattern
        /// </summary>
        public int GetPatternScore()
        {
            // Base score per block
            int baseScore = Blocks.Count * 10;

            // Bonus for pattern type
            int patternBonus = Type == PatternType.Square2x2 ? 50 : 25;

            // Tier multiplier (higher tiers worth more)
            int tierLevel = Tier != null ? Tier.tierLevel : 1;
            float tierMultiplier = 1f + (tierLevel * 0.5f);

            return Mathf.RoundToInt((baseScore + patternBonus) * tierMultiplier);
        }
    }
}
