using UnityEngine;
using ChromaticCascade.Core;
using ChromaticCascade.Scoring;
using System.Collections.Generic;

namespace ChromaticCascade.Gameplay.Abilities
{
    /// <summary>
    /// Tier 2 Ability: Clears the entire row when matched
    /// From gameplay.md: "Clears the entire row when matched"
    /// </summary>
    public class RowClearAbility : TierAbility
    {
        public override void Activate(Vector2Int centerPos, List<Block> matchedBlocks)
        {
            Debug.Log($"[Tier 2 Ability] ROW CLEAR at row {centerPos.y}");
            
            int row = centerPos.y;
            List<Block> blocksInRow = new List<Block>();
            
            // Collect all blocks in this row EXCEPT the evolved block at centerPos
            for (int x = 0; x < GridManager.Instance.Width; x++)
            {
                Vector2Int pos = new Vector2Int(x, row);
                
                // Skip the center position (where the evolved block is)
                if (pos == centerPos)
                    continue;
                
                Block block = GridManager.Instance.GetBlockAt(pos);
                
                if (block != null)
                {
                    blocksInRow.Add(block);
                }
            }
            
            // Play VFX
            Vector3 worldPos = GridManager.Instance.GridToWorld(centerPos);
            PlayAbilityVFX(worldPos);
            
            // Clear all blocks in the row
            foreach (Block block in blocksInRow)
            {
                GridManager.Instance.ClearCell(block.GridPosition);
                BlockFactory.Instance.ReturnToPool(block);
            }
            
            // Award points for row clear
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddAbilityPoints(0, blocksInRow.Count);
                Debug.Log($"[Row Clear] Cleared {blocksInRow.Count} blocks, +{blocksInRow.Count * 10} points");
            }
            
            // Apply gravity to all columns after clearing
            ApplyGravityAfterClear();
        }
        
        private void ApplyGravityAfterClear()
        {
            // Make all blocks above cleared row fall down
            HashSet<int> allColumns = new HashSet<int>();
            for (int x = 0; x < GridManager.Instance.Width; x++)
            {
                allColumns.Add(x);
            }
            
            if (EvolutionResolver.Instance != null)
            {
                EvolutionResolver.Instance.ApplyGravityToColumns(allColumns);
            }
        }
    }
}
