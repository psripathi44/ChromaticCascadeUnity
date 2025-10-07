using UnityEngine;
using ChromaticCascade.Core;
using ChromaticCascade.Data;
using ChromaticCascade.Scoring;
using System.Collections.Generic;

namespace ChromaticCascade.Gameplay.Abilities
{
    /// <summary>
    /// Tier 4 Ability: Initiates a cascade that evolves all adjacent blocks by one tier
    /// From gameplay.md: "Initiates a cascade that evolves all adjacent blocks by one tier"
    /// </summary>
    public class CascadeAbility : TierAbility
    {
        public override void Activate(Vector2Int centerPos, List<Block> matchedBlocks)
        {
            Debug.Log($"[Tier 4 Ability] CASCADE at {centerPos}");
            
            List<Block> adjacentBlocks = new List<Block>();
            
            // Get all adjacent blocks (8 directions - including diagonals)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // Skip center
                    
                    Vector2Int checkPos = centerPos + new Vector2Int(dx, dy);
                    
                    if (GridManager.Instance.IsValidPosition(checkPos))
                    {
                        Block block = GridManager.Instance.GetBlockAt(checkPos);
                        if (block != null && !block.IsEvolved) // Don't cascade evolved blocks
                        {
                            adjacentBlocks.Add(block);
                        }
                    }
                }
            }
            
            // Play VFX
            Vector3 worldPos = GridManager.Instance.GridToWorld(centerPos);
            PlayAbilityVFX(worldPos);
            
            // Evolve each adjacent block by one tier
            int evolvedCount = 0;
            foreach (Block block in adjacentBlocks)
            {
                if (block.TierData.tierLevel < 5) // Can't evolve beyond Tier 5
                {
                    // Get next tier data
                    BlockData nextTierData = BlockFactory.Instance.GetNextTierBlockData(block.BlockData);
                    
                    if (nextTierData != null)
                    {
                        Vector2Int blockPos = block.GridPosition;
                        Vector3 blockWorldPos = block.transform.position;
                        
                        // Remove old block
                        GridManager.Instance.ClearCell(blockPos);
                        BlockFactory.Instance.ReturnToPool(block);
                        
                        // Create evolved block
                        Block evolvedBlock = BlockFactory.Instance.CreateBlock(nextTierData, blockWorldPos);
                        if (evolvedBlock != null)
                        {
                            // Remove FallingBlock component
                            FallingBlock fallingComp = evolvedBlock.GetComponent<FallingBlock>();
                            if (fallingComp != null)
                                Object.DestroyImmediate(fallingComp);
                            
                            // Mark as evolved and register
                            evolvedBlock.MarkAsEvolved();
                            GridManager.Instance.SetCellOccupied(blockPos, evolvedBlock);
                            
                            evolvedCount++;
                        }
                    }
                }
            }
            
            // Award points
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddAbilityPoints(200, evolvedCount);
                Debug.Log($"[Cascade] Evolved {evolvedCount} adjacent blocks, +{200 + (evolvedCount * 10)} points");
            }
            
            // Check for new evolutions from the cascade
            if (EvolutionDetector.Instance != null)
            {
                EvolutionDetector.Instance.CheckForEvolutions();
            }
        }
    }
}
