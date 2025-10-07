using UnityEngine;
using ChromaticCascade.Core;
using ChromaticCascade.Data;
using ChromaticCascade.Scoring;
using System.Collections.Generic;

namespace ChromaticCascade.Gameplay.Abilities
{
    /// <summary>
    /// Tier 3 Ability: Spawns a color bomb that the player can detonate to remove any chosen color
    /// From gameplay.md: "Spawns a color bomb that the player can detonate to remove any chosen color"
    /// 
    /// NOTE: For MVP, we'll auto-detonate on the most common color on the board
    /// Full implementation would show UI for player to choose color
    /// </summary>
    public class ColorBombAbility : TierAbility
    {
        public override void Activate(Vector2Int centerPos, List<Block> matchedBlocks)
        {
            Debug.Log($"[Tier 3 Ability] COLOR BOMB at {centerPos}");
            
            // For MVP: Auto-detonate on the most common color
            ColorData targetColor = FindMostCommonColor();
            
            if (targetColor == null)
            {
                Debug.LogWarning("No color found to bomb!");
                return;
            }
            
            Debug.Log($"[Color Bomb] Targeting color: {targetColor.colorType}");
            
            List<Block> blocksToRemove = new List<Block>();
            HashSet<int> affectedColumns = new HashSet<int>();
            
            // Find all blocks of the target color EXCEPT the evolved block at centerPos
            for (int x = 0; x < GridManager.Instance.Width; x++)
            {
                for (int y = 0; y < GridManager.Instance.Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    
                    // Skip the center position (where the evolved block is)
                    if (pos == centerPos)
                        continue;
                    
                    Block block = GridManager.Instance.GetBlockAt(pos);
                    
                    if (block != null && block.ColorData == targetColor)
                    {
                        blocksToRemove.Add(block);
                        affectedColumns.Add(x);
                    }
                }
            }
            
            // Play VFX
            Vector3 worldPos = GridManager.Instance.GridToWorld(centerPos);
            PlayAbilityVFX(worldPos);
            
            // Remove all blocks of that color
            foreach (Block block in blocksToRemove)
            {
                GridManager.Instance.ClearCell(block.GridPosition);
                BlockFactory.Instance.ReturnToPool(block);
            }
            
            // Award points
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddAbilityPoints(100, blocksToRemove.Count);
                Debug.Log($"[Color Bomb] Removed {blocksToRemove.Count} {targetColor.colorType} blocks, +{100 + (blocksToRemove.Count * 10)} points");
            }
            
            // Apply gravity
            if (EvolutionResolver.Instance != null && affectedColumns.Count > 0)
            {
                EvolutionResolver.Instance.ApplyGravityToColumns(affectedColumns);
            }
        }
        
        private ColorData FindMostCommonColor()
        {
            Dictionary<ColorData, int> colorCounts = new Dictionary<ColorData, int>();
            
            // Count blocks of each color
            for (int x = 0; x < GridManager.Instance.Width; x++)
            {
                for (int y = 0; y < GridManager.Instance.Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Block block = GridManager.Instance.GetBlockAt(pos);
                    
                    if (block != null && block.ColorData != null)
                    {
                        if (!colorCounts.ContainsKey(block.ColorData))
                            colorCounts[block.ColorData] = 0;
                        
                        colorCounts[block.ColorData]++;
                    }
                }
            }
            
            // Find most common
            ColorData mostCommon = null;
            int maxCount = 0;
            
            foreach (var kvp in colorCounts)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    mostCommon = kvp.Key;
                }
            }
            
            return mostCommon;
        }
    }
}
