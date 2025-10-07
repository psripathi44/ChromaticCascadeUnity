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
            
            // FIRST: Remove ALL old blocks (clears the center position too)
            RemoveBlocks(blocks);
            
            // SECOND: Create new evolved block at the now-empty center position
            Vector3 worldPos = GridManager.Instance.GridToWorld(centerPos);
            Block evolvedBlock = BlockFactory.Instance.CreateBlock(nextTierData, worldPos);
            
            if (evolvedBlock != null)
            {
                // CRITICAL: Remove any FallingBlock component (factory might have added it)
                FallingBlock fallingComp = evolvedBlock.GetComponent<FallingBlock>();
                if (fallingComp != null)
                {
                    Object.DestroyImmediate(fallingComp);
                }
                
                // Mark as evolved so it NEVER falls, even during gravity updates
                evolvedBlock.MarkAsEvolved();
                
                // Register in grid
                GridManager.Instance.SetCellOccupied(centerPos, evolvedBlock);
                
                // Play evolution VFX
                evolvedBlock.PlayEvolutionEffect();
                
                // Award evolution points
                int fromTier = currentBlockData.tierData.tierLevel;
                int toTier = nextTierData.tierData.tierLevel;
                
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddEvolutionPoints(fromTier, toTier, blocks.Count);
                }
                
                // Notify listeners
                OnEvolutionCompleted?.Invoke(nextTierData.tierData.tierLevel);
                
                Debug.Log($"Evolved {blocks.Count} {currentBlockData.GetBlockID()} blocks into {nextTierData.GetBlockID()} at {centerPos}");
                
                // TRIGGER TIER ABILITY (gameplay.md: "Automatically activate when their triggering match occurs")
                // Tier 2+: Row Clear, Color Bomb, Cascade, Time Freeze
                if (Abilities.AbilityManager.Instance != null && toTier >= 2)
                {
                    Abilities.AbilityManager.Instance.TriggerAbility(toTier, centerPos, blocks);
                }
            }
        }
        
        /// <summary>
        /// Remove blocks from grid and return to pool
        /// </summary>
        private void RemoveBlocks(List<Block> blocks)
        {
            // Track which columns were affected for gravity update
            HashSet<int> affectedColumns = new HashSet<int>();
            
            foreach (Block block in blocks)
            {
                if (block != null)
                {
                    affectedColumns.Add(block.GridPosition.x);
                    
                    // Clear from grid
                    GridManager.Instance.ClearCell(block.GridPosition);
                    
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
                    
                    // Skip blocks that just evolved THIS frame (let them settle first)
                    if (block != null && block.IsLocked && !block.JustEvolved)
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
