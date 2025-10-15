using UnityEngine;
using ChromaticCascade.Core;
using ChromaticCascade.Data;
using ChromaticCascade.Scoring;

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
            // if (Time.frameCount % 60 == 0)
            // {
            //     Debug.Log($"[BlockSpawner] Waiting for current block to lock. CurrentBlock: {currentFallingBlock?.name} at {currentFallingBlock?.GridPosition}");
            // }
            return;
        }
        
        spawnTimer += Time.deltaTime;
        
        // Debug logging
        // if (spawnTimer >= spawnInterval * 0.9f && Time.frameCount % 10 == 0)
        // {
        //     Debug.Log($"[BlockSpawner] Spawn timer: {spawnTimer:F2}s / {spawnInterval:F2}s (90% threshold reached)");
        // }            if (spawnTimer >= spawnInterval)
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
            
            // Debug.Log($"Spawned block: {blockData.GetBlockID()} at {spawnGridPos}");
        }
        
        /// <summary>
        /// Called when a block locks in place
        /// </summary>
        public void OnBlockLocked(Block block)
        {
            // Debug.Log($"OnBlockLocked called for block at {block.GridPosition}");
            
            if (currentFallingBlock == block)
            {
                currentFallingBlock = null;
                spawnTimer = 0f; // Reset spawn timer
                // Debug.Log("Current falling block cleared. Next block will spawn in " + spawnInterval + " seconds");
                
                // Award points for block placement
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddBlockPlacementPoints();
                }
                
                // Check for evolutions
                if (EvolutionDetector.Instance != null)
                {
                    EvolutionDetector.Instance.CheckForEvolutions();
                }
                else
                {
                    Debug.LogError("[BlockSpawner] EvolutionDetector.Instance is NULL! Add EvolutionDetector component to scene!");
                }
            }
            else
            {
                // This is a block falling from gravity (not the spawned block)
                // Don't award points or check evolutions - just let it settle
                // Debug.Log($"Gravity block {block.name} locked at {block.GridPosition}");
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
        /// Queue a specific block to be spawned next (used for evolved blocks)
        /// </summary>
        public void QueueSpecificBlock(BlockData blockData)
        {
            if (blockData == null)
            {
                Debug.LogError("[BlockSpawner] Cannot queue null block data!");
                return;
            }
            
            // Replace the next block in the queue with this specific block
            if (nextBlockQueue.Count > 0)
            {
                // Remove the current next block
                nextBlockQueue.Dequeue();
            }
            
            // Add the specific block to the front of the queue
            System.Collections.Generic.Queue<BlockData> newQueue = new System.Collections.Generic.Queue<BlockData>();
            newQueue.Enqueue(blockData);
            
            // Add the rest of the blocks back
            while (nextBlockQueue.Count > 0)
            {
                newQueue.Enqueue(nextBlockQueue.Dequeue());
            }
            
            nextBlockQueue = newQueue;
            
            Debug.Log($"[BlockSpawner] Queued specific block: {blockData.GetBlockID()} as next block");
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
