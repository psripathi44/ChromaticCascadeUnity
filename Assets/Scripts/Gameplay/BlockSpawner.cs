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
        
        public Block CurrentFallingBlock => currentFallingBlock;
        public BlockData NextBlockData => nextBlockQueue.Count > 0 ? nextBlockQueue.Peek() : null;
        
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
            if (!autoSpawn || currentFallingBlock != null) return;
            
            spawnTimer += Time.deltaTime;
            
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
                Debug.Log("Spawn blocked - Game Over!");
                // TODO: Trigger game over
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
            
            // Add falling behavior
            FallingBlock fallingBehavior = currentFallingBlock.gameObject.AddComponent<FallingBlock>();
            fallingBehavior.Initialize(currentFallingBlock);
            
            Debug.Log($"Spawned block: {blockData.GetBlockID()} at {spawnGridPos}");
        }
        
        /// <summary>
        /// Called when a block locks in place
        /// </summary>
        public void OnBlockLocked(Block block)
        {
            if (currentFallingBlock == block)
            {
                currentFallingBlock = null;
                spawnTimer = 0f; // Reset spawn timer
            }
        }
        
        /// <summary>
        /// Set spawn interval (for difficulty scaling)
        /// </summary>
        public void SetSpawnInterval(float interval)
        {
            spawnInterval = Mathf.Max(0.1f, interval);
        }
    }
}
