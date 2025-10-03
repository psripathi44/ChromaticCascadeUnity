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
        [SerializeField] private float lockDelay = 0.05f; // Time before locking when on ground (DEBUG: very fast for testing)
        
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
            
            // Always auto-fall regardless of player control
            // Player control only affects horizontal movement
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
                Debug.Log($"[AutoFall] Attempting move from {block.GridPosition}");
                
                if (!TryMoveDown())
                {
                    // Can't move down - start lock timer
                    Debug.Log($"[AutoFall] Cannot move down from {block.GridPosition}. Calling HandleGroundContact().");
                    HandleGroundContact();
                }
                else
                {
                    // Successfully moved - reset lock timer
                    Debug.Log($"[AutoFall] Successfully moved to {block.GridPosition}");
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
            
            bool canMove = CanMoveTo(newPos);
            Debug.Log($"TryMoveDown: Current={currentPos}, New={newPos}, CanMove={canMove}");
            
            if (canMove)
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
                Debug.Log($"[HandleGroundContact] Block grounded at {block.GridPosition}. Lock timer started. LockDelay={lockDelay:F2}s");
            }
            
            // Use unscaled time to avoid issues with very small deltaTime
            float deltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.016f); // Minimum 60fps equivalent
            float oldTimer = lockTimer;
            lockTimer += deltaTime;
            Debug.Log($"[HandleGroundContact] Timer: {oldTimer:F3}s + {deltaTime:F3}s = {lockTimer:F3}s / {lockDelay:F2}s (Component: {GetInstanceID()})");
            
            if (lockTimer >= lockDelay)
            {
                Debug.Log($"[HandleGroundContact] Lock delay reached ({lockTimer:F2}s). Calling LockBlock().");
                LockBlock();
            }
        }
        
        /// <summary>
        /// Check if block can move to a position
        /// </summary>
        public bool CanMoveTo(Vector2Int gridPos)
        {
            bool isValid = GridManager.Instance.IsValidPosition(gridPos);
            bool isOccupied = GridManager.Instance.IsCellOccupied(gridPos);
            Debug.Log($"[CanMoveTo] Checking {gridPos}: IsValid={isValid}, IsOccupied={isOccupied}");
            return isValid && !isOccupied;
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
            Debug.Log($"[LockBlock] Called for block at {block.GridPosition}");
            
            if (isLocked) 
            {
                Debug.Log($"[LockBlock] Block already locked, returning");
                return;
            }
            
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
            
            Debug.Log($"✅ Block locked at {block.GridPosition} - Component will be destroyed");
            
            // Remove this component
            Destroy(this);
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
            Debug.Log($"[ResetLockDelay] Called for block at {block.GridPosition}");
            
            // Only reset if block can now move down (moved away from obstacle)
            if (CanMoveTo(block.GridPosition + Vector2Int.down))
            {
                Debug.Log($"[ResetLockDelay] Block can move down now. Resetting lock timer.");
                isGrounded = false;
                lockTimer = 0f;
            }
            else
            {
                Debug.Log($"[ResetLockDelay] Block still grounded, keeping timer at {lockTimer:F2}s");
            }
        }
    }
}
