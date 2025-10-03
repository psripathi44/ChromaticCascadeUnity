using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ChromaticCascade.Core;
using ChromaticCascade.Data;

namespace ChromaticCascade.Gameplay
{
    /// <summary>
    /// Resolves evolution matches by merging blocks into higher tiers
    /// </summary>
    public class EvolutionResolver : MonoBehaviour
    {
        public static EvolutionResolver Instance { get; private set; }
        
        [Header("Evolution Settings")]
        [SerializeField] private float evolutionDelay = 0.3f;
        [SerializeField] private bool enableCascades = true;
        [SerializeField] private int maxCascadeDepth = 10;
        
        [Header("Animation")]
        [SerializeField] private float highlightDuration = 0.5f;
        [SerializeField] private float mergeDuration = 0.3f;
        
        private bool isProcessingEvolution = false;
        private int currentCascadeDepth = 0;
        
        public event System.Action<int> OnEvolutionCompleted; // Passes tier evolved to
        public event System.Action<int> OnCascadeDepth; // Passes current cascade depth
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        /// <summary>
        /// Process multiple evolution matches
        /// </summary>
        public void ProcessEvolutions(List<List<Block>> matches)
        {
            if (isProcessingEvolution)
            {
                Debug.LogWarning("Already processing evolutions!");
                return;
            }
            
            StartCoroutine(ProcessEvolutionsCoroutine(matches));
        }
        
        /// <summary>
        /// Coroutine to handle evolution with animations and cascades
        /// </summary>
        private IEnumerator ProcessEvolutionsCoroutine(List<List<Block>> matches)
        {
            isProcessingEvolution = true;
            currentCascadeDepth = 0;
            
            do
            {
                currentCascadeDepth++;
                OnCascadeDepth?.Invoke(currentCascadeDepth);
                
                if (currentCascadeDepth > maxCascadeDepth)
                {
                    Debug.LogWarning("Max cascade depth reached!");
                    break;
                }
                
                // Highlight matches
                yield return new WaitForSeconds(highlightDuration);
                
                // Process each match
                foreach (List<Block> match in matches)
                {
                    if (match.Count >= 4) // Safety check
                    {
                        EvolveMatch(match);
                    }
                }
                
                // Wait for merge animation
                yield return new WaitForSeconds(mergeDuration);
                
                // Check for cascades
                if (enableCascades)
                {
                    // Small delay before checking for cascades
                    yield return new WaitForSeconds(evolutionDelay);
                    matches = FindNewMatches();
                }
                else
                {
                    matches.Clear();
                }
                
            } while (matches.Count > 0);
            
            isProcessingEvolution = false;
            currentCascadeDepth = 0;
            
            Debug.Log("Evolution processing complete");
        }
        
        /// <summary>
        /// Evolve a single match into the next tier
        /// </summary>
        private void EvolveMatch(List<Block> blocks)
        {
            if (blocks.Count == 0) return;
            
            // Get reference block for color/tier info
            Block referenceBlock = blocks[0];
            BlockData currentBlockData = referenceBlock.BlockData;
            
            if (currentBlockData.tierData.tierLevel >= 5)
            {
                Debug.Log("Already at max tier (5), cannot evolve further");
                // TODO: Award bonus points instead
                RemoveBlocks(blocks);
                return;
            }
            
            // Get next tier block data
            BlockData nextTierData = BlockFactory.Instance.GetNextTierBlockData(currentBlockData);
            
            if (nextTierData == null)
            {
                Debug.LogError("Could not find next tier data!");
                RemoveBlocks(blocks);
                return;
            }
            
            // Calculate center position for new block
            Vector2Int centerPos = EvolutionDetector.Instance.GetCenterPosition(blocks);
            
            // Ensure center position is valid and empty
            if (!GridManager.Instance.IsValidPosition(centerPos) || GridManager.Instance.IsCellOccupied(centerPos))
            {
                // Find nearest empty cell
                centerPos = FindNearestEmptyCell(centerPos, blocks);
            }
            
            // Remove old blocks
            RemoveBlocks(blocks);
            
            // Create new evolved block
            Vector3 worldPos = GridManager.Instance.GridToWorld(centerPos);
            Block evolvedBlock = BlockFactory.Instance.CreateBlock(nextTierData, worldPos);
            
            if (evolvedBlock != null)
            {
                // Register in grid
                GridManager.Instance.SetCellOccupied(centerPos, evolvedBlock);
                
                // Play evolution VFX
                evolvedBlock.PlayEvolutionEffect();
                
                // Notify listeners
                OnEvolutionCompleted?.Invoke(nextTierData.tierData.tierLevel);
                
                Debug.Log($"Evolved {blocks.Count} {currentBlockData.GetBlockID()} blocks into {nextTierData.GetBlockID()} at {centerPos}");
            }
        }
        
        /// <summary>
        /// Remove blocks from grid and return to pool
        /// </summary>
        private void RemoveBlocks(List<Block> blocks)
        {
            foreach (Block block in blocks)
            {
                if (block != null)
                {
                    // Clear from grid
                    GridManager.Instance.ClearCell(block.GridPosition);
                    
                    // Return to pool
                    BlockFactory.Instance.ReturnToPool(block);
                }
            }
        }
        
        /// <summary>
        /// Find nearest empty cell to a position (excluding blocks being removed)
        /// </summary>
        private Vector2Int FindNearestEmptyCell(Vector2Int center, List<Block> blocksBeingRemoved)
        {
            // Simple spiral search
            for (int radius = 0; radius <= 3; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                            continue; // Only check perimeter
                        
                        Vector2Int checkPos = center + new Vector2Int(dx, dy);
                        
                        if (GridManager.Instance.IsValidPosition(checkPos))
                        {
                            Block blockAtPos = GridManager.Instance.GetBlockAt(checkPos);
                            
                            // Empty or one of the blocks being removed
                            if (blockAtPos == null || blocksBeingRemoved.Contains(blockAtPos))
                            {
                                return checkPos;
                            }
                        }
                    }
                }
            }
            
            return center; // Fallback to original position
        }
        
        /// <summary>
        /// Check for new matches after evolution (for cascades)
        /// </summary>
        private List<List<Block>> FindNewMatches()
        {
            // Re-run evolution detection
            List<List<Block>> matches = new List<List<Block>>();
            
            if (EvolutionDetector.Instance != null)
            {
                // We need to manually call the detection logic
                // For now, return empty list (will be implemented in detector)
                // TODO: Expose detection method properly
            }
            
            return matches;
        }
    }
}
