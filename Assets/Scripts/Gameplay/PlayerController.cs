using UnityEngine;
using UnityEngine.InputSystem;
using ChromaticCascade.Core;

namespace ChromaticCascade.Gameplay
{
    /// <summary>
    /// Handles player input for controlling falling blocks
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }
        
        [Header("Movement Settings")]
        [SerializeField] private float moveDelay = 0.15f; // Time between horizontal moves
        [SerializeField] private float softDropSpeedMultiplier = 5f;
        
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        
        private InputAction moveAction;
        private InputAction dropAction;
        private InputAction hardDropAction;
        
        private Block currentBlock;
        private FallingBlock fallingComponent;
        private float moveTimer = 0f;
        private bool isSoftDropping = false;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            SetupInput();
        }
        
        private void SetupInput()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("InputActions not assigned to PlayerController!");
                return;
            }
            
            var gameplayMap = inputActions.FindActionMap("Gameplay");
            if (gameplayMap != null)
            {
                moveAction = gameplayMap.FindAction("Move");
                dropAction = gameplayMap.FindAction("SoftDrop");
                hardDropAction = gameplayMap.FindAction("HardDrop");
                
                // Enable actions
                moveAction?.Enable();
                dropAction?.Enable();
                hardDropAction?.Enable();
                
                // Subscribe to events
                if (hardDropAction != null)
                    hardDropAction.performed += OnHardDrop;
                
                if (dropAction != null)
                {
                    dropAction.started += _ => isSoftDropping = true;
                    dropAction.canceled += _ => isSoftDropping = false;
                }
            }
        }
        
        private void Update()
        {
            // Get current falling block from spawner
            if (BlockSpawner.Instance != null)
            {
                Block spawnerBlock = BlockSpawner.Instance.CurrentFallingBlock;
                if (spawnerBlock != currentBlock)
                {
                    currentBlock = spawnerBlock;
                    fallingComponent = currentBlock?.GetComponent<FallingBlock>();
                }
            }
            
            if (currentBlock == null || fallingComponent == null || fallingComponent.IsLocked)
                return;
            
            HandleMovement();
            HandleSoftDrop();
        }
        
        /// <summary>
        /// Handle horizontal movement input
        /// </summary>
        private void HandleMovement()
        {
            if (moveAction == null) return;
            
            moveTimer += Time.deltaTime;
            
            float moveInput = moveAction.ReadValue<float>();
            
            if (Mathf.Abs(moveInput) > 0.1f && moveTimer >= moveDelay)
            {
                moveTimer = 0f;
                
                int direction = moveInput > 0 ? 1 : -1;
                TryMoveHorizontal(direction);
            }
        }
        
        /// <summary>
        /// Attempt to move block left or right
        /// </summary>
        private bool TryMoveHorizontal(int direction)
        {
            Vector2Int currentPos = currentBlock.GridPosition;
            Vector2Int newPos = currentPos + new Vector2Int(direction, 0);
            
            if (fallingComponent.CanMoveTo(newPos))
            {
                fallingComponent.MoveToPosition(newPos);
                // Only reset lock delay if the block can now fall down after the horizontal move
                // This prevents interfering with the lock timer when the block is still grounded
                Vector2Int downPos = newPos + Vector2Int.down;
                if (fallingComponent.CanMoveTo(downPos))
                {
                    fallingComponent.ResetLockDelay();
                }
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Handle soft drop (accelerated falling)
        /// </summary>
        private void HandleSoftDrop()
        {
            if (isSoftDropping)
            {
                fallingComponent.SetFallSpeed(softDropSpeedMultiplier);
            }
            else
            {
                fallingComponent.SetFallSpeed(1f);
            }
        }
        
        /// <summary>
        /// Handle hard drop (instant drop)
        /// </summary>
        private void OnHardDrop(InputAction.CallbackContext context)
        {
            if (currentBlock != null && fallingComponent != null && !fallingComponent.IsLocked)
            {
                fallingComponent.HardDrop();
            }
        }
        
        /// <summary>
        /// Check if player is currently controlling a specific block
        /// </summary>
        public bool IsControllingBlock(Block block)
        {
            return currentBlock == block;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (hardDropAction != null)
                hardDropAction.performed -= OnHardDrop;
            
            // Disable actions
            moveAction?.Disable();
            dropAction?.Disable();
            hardDropAction?.Disable();
        }
    }
}
