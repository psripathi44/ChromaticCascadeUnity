using UnityEngine;
using System.Collections.Generic;

namespace ChromaticCascade.Core
{
    /// <summary>
    /// Manages the 7x10 game grid, handles cell tracking, and provides spatial queries
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
        
        [Header("Grid Configuration")]
        [SerializeField] private int gridWidth = 7;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        
        [Header("Visual Debug")]
        [SerializeField] private bool showGridGizmos = true;
        [SerializeField] private Color gridLineColor = Color.white;
        [SerializeField] private Color occupiedCellColor = Color.red;
        [SerializeField] private Color emptyCellColor = Color.green;
        
        private GridCell[,] grid;
        
        // Public properties for external access
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public int Width => gridWidth;
        public int Height => gridHeight;
        public float CellSize => cellSize;
        public Vector3 GridOrigin => gridOrigin;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeGrid();
        }
        
        /// <summary>
        /// Initialize the grid array
        /// </summary>
        private void InitializeGrid()
        {
            grid = new GridCell[gridWidth, gridHeight];
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    grid[x, y] = new GridCell(new Vector2Int(x, y));
                }
            }
            
            Debug.Log($"Grid initialized: {gridWidth}x{gridHeight}");
        }
        
        /// <summary>
        /// Convert world position to grid coordinates
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            Vector3 localPos = worldPos - gridOrigin;
            int x = Mathf.FloorToInt(localPos.x / cellSize);
            int y = Mathf.FloorToInt(localPos.y / cellSize);
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Convert grid coordinates to world position (center of cell)
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float worldX = gridOrigin.x + (gridPos.x * cellSize) + (cellSize * 0.5f);
            float worldY = gridOrigin.y + (gridPos.y * cellSize) + (cellSize * 0.5f);
            return new Vector3(worldX, worldY, 0f);
        }
        
        /// <summary>
        /// Check if grid position is within bounds
        /// </summary>
        public bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
        }
        
        /// <summary>
        /// Check if cell is occupied by a block
        /// </summary>
        public bool IsCellOccupied(Vector2Int pos)
        {
            if (!IsValidPosition(pos)) return true; // Out of bounds = occupied
            return grid[pos.x, pos.y].isOccupied;
        }
        
        /// <summary>
        /// Set cell as occupied by a block
        /// </summary>
        public void SetCellOccupied(Vector2Int pos, Gameplay.Block block)
        {
            if (!IsValidPosition(pos))
            {
                Debug.LogWarning($"Attempted to occupy invalid position: {pos}");
                return;
            }
            
            grid[pos.x, pos.y].SetOccupied(block);
        }
        
        /// <summary>
        /// Clear a cell
        /// </summary>
        public void ClearCell(Vector2Int pos)
        {
            if (!IsValidPosition(pos)) return;
            grid[pos.x, pos.y].Clear();
        }
        
        /// <summary>
        /// Get the block at a grid position
        /// </summary>
        public Gameplay.Block GetBlockAt(Vector2Int pos)
        {
            if (!IsValidPosition(pos)) return null;
            return grid[pos.x, pos.y].occupyingBlock;
        }
        
        /// <summary>
        /// Get all neighboring cells (4-directional by default)
        /// </summary>
        public List<GridCell> GetNeighbors(Vector2Int pos, bool includeDiagonal = false)
        {
            List<GridCell> neighbors = new List<GridCell>();
            
            // Cardinal directions
            Vector2Int[] cardinalDirections = {
                new Vector2Int(0, 1),   // Up
                new Vector2Int(0, -1),  // Down
                new Vector2Int(-1, 0),  // Left
                new Vector2Int(1, 0)    // Right
            };
            
            foreach (Vector2Int dir in cardinalDirections)
            {
                Vector2Int neighborPos = pos + dir;
                if (IsValidPosition(neighborPos))
                {
                    neighbors.Add(grid[neighborPos.x, neighborPos.y]);
                }
            }
            
            // Diagonal directions (optional)
            if (includeDiagonal)
            {
                Vector2Int[] diagonalDirections = {
                    new Vector2Int(-1, 1),  // Top-Left
                    new Vector2Int(1, 1),   // Top-Right
                    new Vector2Int(-1, -1), // Bottom-Left
                    new Vector2Int(1, -1)   // Bottom-Right
                };
                
                foreach (Vector2Int dir in diagonalDirections)
                {
                    Vector2Int neighborPos = pos + dir;
                    if (IsValidPosition(neighborPos))
                    {
                        neighbors.Add(grid[neighborPos.x, neighborPos.y]);
                    }
                }
            }
            
            return neighbors;
        }
        
        /// <summary>
        /// Get all cells in a specific row
        /// </summary>
        public List<GridCell> GetRow(int y)
        {
            List<GridCell> row = new List<GridCell>();
            if (y < 0 || y >= gridHeight) return row;
            
            for (int x = 0; x < gridWidth; x++)
            {
                row.Add(grid[x, y]);
            }
            return row;
        }
        
        /// <summary>
        /// Get all cells in a specific column
        /// </summary>
        public List<GridCell> GetColumn(int x)
        {
            List<GridCell> column = new List<GridCell>();
            if (x < 0 || x >= gridWidth) return column;
            
            for (int y = 0; y < gridHeight; y++)
            {
                column.Add(grid[x, y]);
            }
            return column;
        }
        
        /// <summary>
        /// Clear the entire grid
        /// </summary>
        public void ClearGrid()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    grid[x, y].Clear();
                }
            }
        }
        
        /// <summary>
        /// Get spawn position (center top of grid)
        /// </summary>
        public Vector2Int GetSpawnPosition()
        {
            return new Vector2Int(gridWidth / 2, gridHeight - 1);
        }
        
        /// <summary>
        /// Check if spawn position is blocked (game over condition)
        /// </summary>
        public bool IsSpawnBlocked()
        {
            Vector2Int spawnPos = GetSpawnPosition();
            return IsCellOccupied(spawnPos);
        }
        
        // Debug visualization
        private void OnDrawGizmos()
        {
            if (!showGridGizmos) return;
            
            // Draw grid lines
            Gizmos.color = gridLineColor;
            
            // Vertical lines
            for (int x = 0; x <= gridWidth; x++)
            {
                Vector3 start = gridOrigin + new Vector3(x * cellSize, 0, 0);
                Vector3 end = start + new Vector3(0, gridHeight * cellSize, 0);
                Gizmos.DrawLine(start, end);
            }
            
            // Horizontal lines
            for (int y = 0; y <= gridHeight; y++)
            {
                Vector3 start = gridOrigin + new Vector3(0, y * cellSize, 0);
                Vector3 end = start + new Vector3(gridWidth * cellSize, 0, 0);
                Gizmos.DrawLine(start, end);
            }
            
            // Draw cell occupancy (if grid is initialized)
            if (grid != null && Application.isPlaying)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        Vector3 cellCenter = GridToWorld(new Vector2Int(x, y));
                        Gizmos.color = grid[x, y].isOccupied ? occupiedCellColor : emptyCellColor;
                        Gizmos.DrawWireCube(cellCenter, Vector3.one * cellSize * 0.8f);
                    }
                }
            }
        }
    }
}
