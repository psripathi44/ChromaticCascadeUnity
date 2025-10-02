using UnityEngine;

namespace ChromaticCascade.Core
{
    /// <summary>
    /// Represents a single cell in the grid
    /// </summary>
    [System.Serializable]
    public class GridCell
    {
        public Vector2Int gridPosition;
        public bool isOccupied;
        public Gameplay.Block occupyingBlock;
        
        // Optional debug visualization
        public SpriteRenderer debugVisual;
        
        public GridCell(Vector2Int position)
        {
            gridPosition = position;
            isOccupied = false;
            occupyingBlock = null;
        }
        
        public void SetOccupied(Gameplay.Block block)
        {
            isOccupied = true;
            occupyingBlock = block;
        }
        
        public void Clear()
        {
            isOccupied = false;
            occupyingBlock = null;
        }
        
        public override string ToString()
        {
            return $"Cell({gridPosition.x},{gridPosition.y}) - {(isOccupied ? "Occupied" : "Empty")}";
        }
    }
}
