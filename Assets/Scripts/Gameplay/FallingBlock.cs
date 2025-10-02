using UnityEngine;
using ChromaticCascade.Core;

namespace ChromaticCascade.Gameplay
{
    /// <summary>
    /// Handles automatic falling behavior and lock delay for blocks
    /// </summary>
    public class FallingBlock : MonoBehaviour
    {
        [Header("Fall Settings")]
        [SerializeField] private float fallSpeed = 1f; // Cells per second
        [SerializeField] private float lockDelay = 0.5f; // Time before locking when on ground
        
        private Block block;
        private float fallTimer = 0f;
        private float lockTimer = 0f;
        private bool isGrounded = false;
        private bool isLocked = false;
        
        public bool IsLocked => isLocked;
        
        public void Initialize(Block blockComponent)
        {
            block = blockComponent;
            fallSpeed = 1f; // Default fall speed
        }
        
        private void Awake()
        {
            // Try to get Block component if not initialized
            if (block == null)
            {
                block = GetComponent<Block>();
            }
        }
        
        private void Update()
        {
            if (isLocked || block == null) return;
            
            // Check if being controlled by player
            if (PlayerController.Instance != null && PlayerController.Instance.IsControllingBlock(block))
            {
                // Player is controlling, don't auto-fall yet (will be handled by soft drop)
                return;
            }
            
            AutoFall();
        }
        
        /// <summary>
        /// Automatic falling behavior
        /// </summary>
        private void AutoFall()
        {
            fallTimer += Time.deltaTime;
            
            float fallInterval = 1f / fallSpeed;
            
            if (fallTimer >= fallInterval)
            {
                fallTimer = 0f;
                
                if (!TryMoveDown())
                {
                    // Can't move down - start lock timer
                    HandleGroundContact();
                }
                else
                {
                    // Successfully moved - reset lock timer
                    isGrounded = false;
                    lockTimer = 0f;
                }
            }
        }
        
        /// <summary>
        /// Attempt to move block down one cell
        /// </summary>
        public bool TryMoveDown()
        {
            if (block == null)
            {
                Debug.LogWarning("FallingBlock: block is null in TryMoveDown");
                return false;
            }
            
            Vector2Int currentPos = block.GridPosition;
            Vector2Int newPos = currentPos + Vector2Int.down;
            
            if (CanMoveTo(newPos))
            {
                MoveToPosition(newPos);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Handle contact with ground or other blocks
        /// </summary>
        private void HandleGroundContact()
        {
            if (!isGrounded)
            {
                isGrounded = true;
                lockTimer = 0f;
            }
            
            lockTimer += Time.deltaTime;
            
            if (lockTimer >= lockDelay)
            {
                LockBlock();
            }
        }
        
        /// <summary>
        /// Check if block can move to a position
        /// </summary>
        public bool CanMoveTo(Vector2Int gridPos)
        {
            return GridManager.Instance.IsValidPosition(gridPos) &&
                   !GridManager.Instance.IsCellOccupied(gridPos);
        }
        
        /// <summary>
        /// Move block to a new grid position
        /// </summary>
        public void MoveToPosition(Vector2Int newGridPos)
        {
            block.SetGridPosition(newGridPos);
            transform.position = GridManager.Instance.GridToWorld(newGridPos);
        }
        
        /// <summary>
        /// Lock the block in place
        /// </summary>
        private void LockBlock()
        {
            if (isLocked) return;
            
            isLocked = true;
            block.Lock();
            
            // Register with grid
            GridManager.Instance.SetCellOccupied(block.GridPosition, block);
            
            // Notify spawner
            BlockSpawner.Instance.OnBlockLocked(block);
            
            // Trigger evolution check
            if (EvolutionDetector.Instance != null)
            {
                EvolutionDetector.Instance.CheckForEvolutions();
            }
            
            // Remove this component
            Destroy(this);
            
            Debug.Log($"Block locked at {block.GridPosition}");
        }
        
        /// <summary>
        /// Force immediate drop (hard drop)
        /// </summary>
        public void HardDrop()
        {
            while (TryMoveDown())
            {
                // Keep moving down until we can't
            }
            
            LockBlock();
        }
        
        /// <summary>
        /// Accelerate fall speed (soft drop)
        /// </summary>
        public void SetFallSpeed(float speed)
        {
            fallSpeed = speed;
        }
        
        /// <summary>
        /// Reset lock delay (called when player moves block away from ground)
        /// </summary>
        public void ResetLockDelay()
        {
            if (!CanMoveTo(block.GridPosition + Vector2Int.down))
            {
                lockTimer = 0f;
            }
            else
            {
                isGrounded = false;
                lockTimer = 0f;
            }
        }
    }
}
