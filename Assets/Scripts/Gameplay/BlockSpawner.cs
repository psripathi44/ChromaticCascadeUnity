using UnityEngine;
using ChromaticCascade.Core;
using ChromaticCascade.Data;

namespace ChromaticCascade.Gameplay
{
    /// <summary>
    /// Handles block spawning at the top of the grid
    /// </summary>
    public class BlockSpawner : MonoBehaviour
    {
        public static BlockSpawner Instance { get; private set; }
        
        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private bool autoSpawn = true;
        
        [Header("Preview")]
        [SerializeField] private int previewQueueSize = 1;
        
        private float spawnTimer = 0f;
        private Block currentFallingBlock;
        private System.Collections.Generic.Queue<BlockData> nextBlockQueue = new System.Collections.Generic.Queue<BlockData>();
        private bool isGameOver = false;
        
        public Block CurrentFallingBlock => currentFallingBlock;
        public BlockData NextBlockData => nextBlockQueue.Count > 0 ? nextBlockQueue.Peek() : null;
        public bool IsGameOver => isGameOver;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            // Initialize preview queue
            for (int i = 0; i < previewQueueSize; i++)
            {
                nextBlockQueue.Enqueue(BlockFactory.Instance.GetRandomTier1BlockData());
            }
            
            // Spawn first block
            if (autoSpawn)
            {
                SpawnNextBlock();
            }
        }
        
        private void Update()
        {
            if (!autoSpawn || isGameOver)
            {
                return;
            }
            
            if (currentFallingBlock != null)
            {
                // Debug logging every 60 frames (~1 second)
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[BlockSpawner] Waiting for current block to lock. CurrentBlock: {currentFallingBlock?.name} at {currentFallingBlock?.GridPosition}");
                }
                return;
            }
            
            spawnTimer += Time.deltaTime;
            
            // Debug logging
            if (spawnTimer >= spawnInterval * 0.9f && Time.frameCount % 10 == 0)
            {
                Debug.Log($"[BlockSpawner] Spawn timer: {spawnTimer:F2}s / {spawnInterval:F2}s (90% threshold reached)");
            }
            
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnNextBlock();
            }
        }
        
        /// <summary>
        /// Spawn the next block at the spawn position
        /// </summary>
        public void SpawnNextBlock()
        {
            // Check if spawn is blocked (game over)
            if (GridManager.Instance.IsSpawnBlocked())
            {
                if (!isGameOver)
                {
                    isGameOver = true;
                    Debug.Log("🎮 GAME OVER - Spawn position blocked!");
                    // TODO: Trigger game over UI/event
                }
                return;
            }
            
            // Get next block data from queue
            BlockData blockData = nextBlockQueue.Dequeue();
            
            // Refill queue
            nextBlockQueue.Enqueue(BlockFactory.Instance.GetRandomTier1BlockData());
            
            // Spawn block
            Vector2Int spawnGridPos = GridManager.Instance.GetSpawnPosition();
            Vector3 spawnWorldPos = GridManager.Instance.GridToWorld(spawnGridPos);
            
            currentFallingBlock = BlockFactory.Instance.CreateBlock(blockData, spawnWorldPos);
            
            // Add falling behavior (check if it doesn't already exist)
            FallingBlock fallingBehavior = currentFallingBlock.gameObject.GetComponent<FallingBlock>();
            if (fallingBehavior == null)
            {
                fallingBehavior = currentFallingBlock.gameObject.AddComponent<FallingBlock>();
            }
            fallingBehavior.Initialize(currentFallingBlock);
            
            Debug.Log($"Spawned block: {blockData.GetBlockID()} at {spawnGridPos}");
        }
        
        /// <summary>
        /// Called when a block locks in place
        /// </summary>
        public void OnBlockLocked(Block block)
        {
            Debug.Log($"OnBlockLocked called for block at {block.GridPosition}");
            
            if (currentFallingBlock == block)
            {
                currentFallingBlock = null;
                spawnTimer = 0f; // Reset spawn timer
                Debug.Log("Current falling block cleared. Next block will spawn in " + spawnInterval + " seconds");
            }
            else
            {
                Debug.LogWarning($"Locked block {block.name} is not the current falling block!");
            }
        }
        
        /// <summary>
        /// Set spawn interval (for difficulty scaling)
        /// </summary>
        public void SetSpawnInterval(float interval)
        {
            spawnInterval = Mathf.Max(0.1f, interval);
        }
        
        /// <summary>
        /// Reset the spawner for a new game
        /// </summary>
        public void ResetGame()
        {
            isGameOver = false;
            spawnTimer = 0f;
            currentFallingBlock = null;
            
            // Clear and refill preview queue
            nextBlockQueue.Clear();
            for (int i = 0; i < previewQueueSize; i++)
            {
                nextBlockQueue.Enqueue(BlockFactory.Instance.GetRandomTier1BlockData());
            }
            
            Debug.Log("🔄 Game Reset - Ready to play!");
        }
    }
}
