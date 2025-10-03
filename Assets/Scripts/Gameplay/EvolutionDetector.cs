using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ChromaticCascade.Core;

namespace ChromaticCascade.Gameplay
{
    /// <summary>
    /// Detects matching groups of 4+ connected blocks and 2x2 squares for evolution
    /// </summary>
    public class EvolutionDetector : MonoBehaviour
    {
        public static EvolutionDetector Instance { get; private set; }
        
        [Header("Detection Settings")]
        [SerializeField] private int minBlocksForEvolution = 4;
        [SerializeField] private bool allow2x2Detection = true;
        
        [Header("Debug")]
        [SerializeField] private bool debugVisualization = false;
        
        private HashSet<Block> checkedBlocks = new HashSet<Block>();
        
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
        /// Check entire grid for evolutions
        /// </summary>
        public void CheckForEvolutions()
        {
            checkedBlocks.Clear();
            
            List<List<Block>> matches = FindAllMatches();
            
            if (matches.Count > 0)
            {
                Debug.Log($"Found {matches.Count} matches!");
                
                // Send to EvolutionResolver
                if (EvolutionResolver.Instance != null)
                {
                    EvolutionResolver.Instance.ProcessEvolutions(matches);
                }
            }
        }
        
        /// <summary>
        /// Find all matching groups in the grid
        /// </summary>
        private List<List<Block>> FindAllMatches()
        {
            List<List<Block>> allMatches = new List<List<Block>>();
            
            // Check for 4+ connected blocks (flood fill)
            for (int x = 0; x < GridManager.Instance.Width; x++)
            {
                for (int y = 0; y < GridManager.Instance.Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Block block = GridManager.Instance.GetBlockAt(pos);
                    
                    if (block != null && !checkedBlocks.Contains(block))
                    {
                        List<Block> connectedGroup = FindConnectedBlocks(block);
                        
                        if (connectedGroup.Count >= minBlocksForEvolution)
                        {
                            allMatches.Add(connectedGroup);
                            
                            // Mark all blocks in this group as checked
                            foreach (Block b in connectedGroup)
                            {
                                checkedBlocks.Add(b);
                            }
                        }
                    }
                }
            }
            
            // Check for 2x2 squares (if enabled)
            if (allow2x2Detection)
            {
                List<List<Block>> squares = Find2x2Squares();
                allMatches.AddRange(squares);
            }
            
            return allMatches;
        }
        
        /// <summary>
        /// Find all connected blocks of the same color and tier using flood fill (BFS)
        /// </summary>
        private List<Block> FindConnectedBlocks(Block startBlock)
        {
            List<Block> connected = new List<Block>();
            Queue<Block> toCheck = new Queue<Block>();
            HashSet<Block> visited = new HashSet<Block>();
            
            toCheck.Enqueue(startBlock);
            visited.Add(startBlock);
            
            while (toCheck.Count > 0)
            {
                Block current = toCheck.Dequeue();
                connected.Add(current);
                
                // Check all 4 neighbors (cardinal directions only)
                List<GridCell> neighbors = GridManager.Instance.GetNeighbors(current.GridPosition, false);
                
                foreach (GridCell neighborCell in neighbors)
                {
                    if (neighborCell.isOccupied && neighborCell.occupyingBlock != null)
                    {
                        Block neighborBlock = neighborCell.occupyingBlock;
                        
                        // Check if matches and hasn't been visited
                        if (!visited.Contains(neighborBlock) && current.Matches(neighborBlock))
                        {
                            visited.Add(neighborBlock);
                            toCheck.Enqueue(neighborBlock);
                        }
                    }
                }
            }
            
            return connected;
        }
        
        /// <summary>
        /// Find all 2x2 square matches in the grid
        /// </summary>
        private List<List<Block>> Find2x2Squares()
        {
            List<List<Block>> squares = new List<List<Block>>();
            
            // Check every possible 2x2 area
            for (int x = 0; x < GridManager.Instance.Width - 1; x++)
            {
                for (int y = 0; y < GridManager.Instance.Height - 1; y++)
                {
                    // Get 4 blocks in 2x2 area
                    Block bl = GridManager.Instance.GetBlockAt(new Vector2Int(x, y));         // Bottom-left
                    Block br = GridManager.Instance.GetBlockAt(new Vector2Int(x + 1, y));     // Bottom-right
                    Block tl = GridManager.Instance.GetBlockAt(new Vector2Int(x, y + 1));     // Top-left
                    Block tr = GridManager.Instance.GetBlockAt(new Vector2Int(x + 1, y + 1)); // Top-right
                    
                    // Check if all 4 exist and match
                    if (bl != null && br != null && tl != null && tr != null)
                    {
                        if (bl.Matches(br) && bl.Matches(tl) && bl.Matches(tr))
                        {
                            // Check if this square wasn't already found in flood fill
                            if (!checkedBlocks.Contains(bl))
                            {
                                List<Block> square = new List<Block> { bl, br, tl, tr };
                                squares.Add(square);
                                
                                // Mark as checked
                                foreach (Block b in square)
                                {
                                    checkedBlocks.Add(b);
                                }
                            }
                        }
                    }
                }
            }
            
            return squares;
        }
        
        /// <summary>
        /// Get center position of a group of blocks (for spawning evolved block)
        /// </summary>
        public Vector2Int GetCenterPosition(List<Block> blocks)
        {
            if (blocks.Count == 0) return Vector2Int.zero;
            
            float avgX = (float)blocks.Average(b => b.GridPosition.x);
            float avgY = (float)blocks.Average(b => b.GridPosition.y);
            
            return new Vector2Int(Mathf.RoundToInt(avgX), Mathf.RoundToInt(avgY));
        }
        
        private void OnDrawGizmos()
        {
            if (!debugVisualization || !Application.isPlaying) return;
            
            // Visualize checked blocks
            foreach (Block block in checkedBlocks)
            {
                if (block != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(block.transform.position, Vector3.one * 0.9f);
                }
            }
        }
    }
}
