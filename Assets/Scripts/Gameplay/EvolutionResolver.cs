using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ChromaticCascade.Core;
using ChromaticCascade.Data;
using ChromaticCascade.Scoring;

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
            
            // Clear "justEvolved" flag on all blocks so they can fall later
            ClearAllJustEvolvedFlags();
            
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
            
            // Calculate center position (used for ability trigger location and evolved block placement)
            Vector2Int centerPos = EvolutionDetector.Instance.GetCenterPosition(blocks);
            
            // Get tier info
            int fromTier = currentBlockData.tierData.tierLevel;
            int toTier = nextTierData.tierData.tierLevel;
            
            Debug.Log($"[Evolution] {blocks.Count} {currentBlockData.GetBlockID()} blocks → {nextTierData.GetBlockID()} at {centerPos}");
            
            // FIRST: Remove the matched blocks (but save the center position for the evolved block)
            RemoveBlocks(blocks);
            
            // SECOND: For Tier 1 blocks evolving to Tier 2, spawn the evolved block from the top
            if (fromTier == 1 && toTier == 2)
            {
                // Spawn the evolved block from the top in a random column
                int randomColumn = Random.Range(0, GridManager.Instance.GridWidth);
                SpawnEvolvedBlock(nextTierData, randomColumn);
                
                Debug.Log($"[Evolution] Spawned Tier 2 block at top (col {randomColumn})");
                
                // THIRD: Trigger ability for the evolved block (at the center of the match)
                if (Abilities.AbilityManager.Instance != null)
                {
                    Debug.Log($"[Evolution] Triggering Tier {toTier} ability at {centerPos}");
                    Abilities.AbilityManager.Instance.TriggerAbility(toTier, centerPos, blocks);
                }
            }
            else
            {
                // For higher tier evolutions (Tier 2->3, 3->4, 4->5), place at the match center
                Vector3 worldPos = GridManager.Instance.GridToWorld(centerPos);
                Block evolvedBlock = BlockFactory.Instance.CreateBlock(nextTierData, worldPos);
                
                if (evolvedBlock != null)
                {
                    // Place the evolved block at the center position
                    evolvedBlock.SetGridPosition(centerPos);
                    GridManager.Instance.SetCellOccupied(centerPos, evolvedBlock);
                    evolvedBlock.MarkAsEvolved();
                    evolvedBlock.PlayEvolutionEffect();
                    
                    Debug.Log($"[Evolution] Created {nextTierData.GetBlockID()} at match center {centerPos}");
                    
                    // THIRD: Trigger ability for the evolved block
                    if (Abilities.AbilityManager.Instance != null && toTier >= 3 && toTier <= 5)
                    {
                        Debug.Log($"[Evolution] Triggering Tier {toTier} ability at {centerPos}");
                        Abilities.AbilityManager.Instance.TriggerAbility(toTier, centerPos, blocks);
                    }
                }
            }
            
            // Award evolution points
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddEvolutionPoints(fromTier, toTier, blocks.Count);
            }
            
            // Notify listeners
            OnEvolutionCompleted?.Invoke(toTier);
        }
        
        /// <summary>
        /// Spawn an evolved block from the top of the grid
        /// </summary>
        private void SpawnEvolvedBlock(BlockData blockData, int column)
        {
            // Instead of directly spawning, queue the block in the BlockSpawner
            // This ensures it follows the normal spawning rules and player can control it
            if (BlockSpawner.Instance != null)
            {
                // Queue the evolved block as the next block to spawn
                BlockSpawner.Instance.QueueSpecificBlock(blockData);
                Debug.Log($"[Evolution] Queued {blockData.GetBlockID()} as next block to spawn");
                return;
            }
            
            // Fallback if BlockSpawner is not available (shouldn't happen)
            Debug.LogWarning("[Evolution] BlockSpawner not available, using direct spawn method");
            
            // Get spawn position at top of grid
            Vector2Int spawnGridPos = new Vector2Int(column, GridManager.Instance.GridHeight - 1);
            
            // Check if blocked, try adjacent columns
            if (GridManager.Instance.IsCellOccupied(spawnGridPos))
            {
                Debug.LogWarning($"[Evolution] Column {column} blocked, trying adjacent");
                for (int offset = 1; offset <= 3; offset++)
                {
                    int rightCol = column + offset;
                    if (rightCol < GridManager.Instance.GridWidth)
                    {
                        Vector2Int rightPos = new Vector2Int(rightCol, GridManager.Instance.GridHeight - 1);
                        if (!GridManager.Instance.IsCellOccupied(rightPos))
                        {
                            spawnGridPos = rightPos;
                            break;
                        }
                    }
                    
                    int leftCol = column - offset;
                    if (leftCol >= 0)
                    {
                        Vector2Int leftPos = new Vector2Int(leftCol, GridManager.Instance.GridHeight - 1);
                        if (!GridManager.Instance.IsCellOccupied(leftPos))
                        {
                            spawnGridPos = leftPos;
                            break;
                        }
                    }
                }
                
                if (GridManager.Instance.IsCellOccupied(spawnGridPos))
                {
                    Debug.LogError($"[Evolution] All spawn blocked! Block lost.");
                    return;
                }
            }
            
            // Create evolved block at spawn position
            Vector3 spawnWorldPos = GridManager.Instance.GridToWorld(spawnGridPos);
            Block evolvedBlock = BlockFactory.Instance.CreateBlock(blockData, spawnWorldPos);
            
            if (evolvedBlock != null)
            {
                // Add FallingBlock so it falls normally
                FallingBlock falling = evolvedBlock.gameObject.GetComponent<FallingBlock>();
                if (falling == null)
                {
                    falling = evolvedBlock.gameObject.AddComponent<FallingBlock>();
                }
                falling.Initialize(evolvedBlock);
                
                evolvedBlock.MarkAsEvolved();
                
                Debug.Log($"[Evolution] Spawned {blockData.GetBlockID()} at top (col {spawnGridPos.x})");
                evolvedBlock.PlayEvolutionEffect();
            }
        }
        
        /// <summary>
        /// Remove blocks from grid and return to pool
        /// </summary>
        private void RemoveBlocks(List<Block> blocks)
        {
            // Track which columns were affected for gravity update
            HashSet<int> affectedColumns = new HashSet<int>();
            
            // Debug log to track which blocks are being removed
            Debug.Log($"[Evolution] Removing {blocks.Count} blocks:");
            
            foreach (Block block in blocks)
            {
                if (block != null)
                {
                    Vector2Int pos = block.GridPosition;
                    affectedColumns.Add(pos.x);
                    
                    Debug.Log($"[Evolution] Removing block at {pos} - {block.BlockData.GetBlockID()}");
                    
                    // Clear from grid
                    GridManager.Instance.ClearCell(pos);
                    
                    // Return to pool
                    BlockFactory.Instance.ReturnToPool(block);
                }
            }
            
            // Make blocks above fall down in affected columns
            ApplyGravityToColumns(affectedColumns);
        }
        
        /// <summary>
        /// Apply gravity to blocks in affected columns after evolution
        /// Made public so abilities can trigger gravity (e.g., after Row Clear)
        /// </summary>
        public void ApplyGravityToColumns(HashSet<int> columns)
        {
            foreach (int column in columns)
            {
                // Start from TOP and work downwards to process highest blocks first
                for (int y = GridManager.Instance.Height - 1; y >= 0; y--)
                {
                    Vector2Int pos = new Vector2Int(column, y);
                    Block block = GridManager.Instance.GetBlockAt(pos);
                    
                    // Skip empty cells and blocks that just evolved THIS frame (let them settle first)
                    if (block == null || block.JustEvolved)
                    {
                        continue;
                    }
                    
                    // Only apply gravity to locked blocks
                    if (block.IsLocked)
                    {
                        // Check if block can fall
                        Vector2Int below = pos + Vector2Int.down;
                        
                        if (GridManager.Instance.IsValidPosition(below) && !GridManager.Instance.IsCellOccupied(below))
                        {
                            // Block has empty space below, make it fall
                            FallingBlock fallingComponent = block.gameObject.GetComponent<FallingBlock>();
                            if (fallingComponent == null)
                            {
                                // Unlock the block and add falling behavior
                                block.Unlock();
                                GridManager.Instance.ClearCell(pos);
                                
                                fallingComponent = block.gameObject.AddComponent<FallingBlock>();
                                fallingComponent.Initialize(block);
                                
                                Debug.Log($"[Gravity] Block at {pos} falling to fill empty space below");
                            }
                        }
                    }
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
            List<List<Block>> matches = new List<List<Block>>();
            
            if (EvolutionDetector.Instance != null)
            {
                // Clear previous checks and find all new matches
                matches = EvolutionDetector.Instance.FindAllMatchesPublic();
                
                if (matches.Count > 0)
                {
                    Debug.Log($"[Cascade] Found {matches.Count} new matches for cascade!");
                }
            }
            
            return matches;
        }
        
        /// <summary>
        /// Clear "justEvolved" flag on all blocks after evolution completes
        /// This allows evolved blocks to fall later when space opens beneath them
        /// </summary>
        private void ClearAllJustEvolvedFlags()
        {
            for (int x = 0; x < GridManager.Instance.Width; x++)
            {
                for (int y = 0; y < GridManager.Instance.Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Block block = GridManager.Instance.GetBlockAt(pos);
                    
                    if (block != null && block.JustEvolved)
                    {
                        block.ClearJustEvolved();
                    }
                }
            }
        }
    }
}
