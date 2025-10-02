using UnityEngine;
using System.Collections.Generic;
using ChromaticCascade.Data;
using ChromaticCascade.Core;

namespace ChromaticCascade.Gameplay
{
    /// <summary>
    /// Factory for creating and managing block instances
    /// </summary>
    public class BlockFactory : MonoBehaviour
    {
        public static BlockFactory Instance { get; private set; }
        
        [Header("Block Database")]
        [SerializeField] private List<BlockData> allBlockData = new List<BlockData>();
        
        [Header("Prefabs")]
        [SerializeField] private GameObject defaultBlockPrefab;
        
        [Header("Pooling")]
        [SerializeField] private Transform blockPoolParent;
        [SerializeField] private int initialPoolSize = 20;
        
        private Queue<Block> blockPool = new Queue<Block>();
        private Dictionary<string, BlockData> blockDataDictionary = new Dictionary<string, BlockData>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeBlockDatabase();
            InitializePool();
        }
        
        /// <summary>
        /// Initialize the block data dictionary for fast lookups
        /// </summary>
        private void InitializeBlockDatabase()
        {
            blockDataDictionary.Clear();
            
            foreach (BlockData data in allBlockData)
            {
                if (data != null)
                {
                    string key = data.GetBlockID();
                    if (!blockDataDictionary.ContainsKey(key))
                    {
                        blockDataDictionary.Add(key, data);
                    }
                }
            }
            
            Debug.Log($"Block database initialized with {blockDataDictionary.Count} block types");
        }
        
        /// <summary>
        /// Pre-instantiate blocks for object pooling
        /// </summary>
        private void InitializePool()
        {
            if (blockPoolParent == null)
            {
                GameObject poolObj = new GameObject("BlockPool");
                poolObj.transform.SetParent(transform);
                blockPoolParent = poolObj.transform;
            }
            
            for (int i = 0; i < initialPoolSize; i++)
            {
                Block block = CreateNewBlockInstance();
                block.gameObject.SetActive(false);
                blockPool.Enqueue(block);
            }
        }
        
        /// <summary>
        /// Create a new block instance (not from pool)
        /// </summary>
        private Block CreateNewBlockInstance()
        {
            GameObject blockObj = Instantiate(defaultBlockPrefab, blockPoolParent);
            Block block = blockObj.GetComponent<Block>();
            
            if (block == null)
            {
                block = blockObj.AddComponent<Block>();
            }
            
            return block;
        }
        
        /// <summary>
        /// Create a block with specific color and tier
        /// </summary>
        public Block CreateBlock(ColorData colorData, TierData tierData, Vector3 worldPosition)
        {
            if (colorData == null || tierData == null)
            {
                Debug.LogError("Cannot create block with null ColorData or TierData");
                return null;
            }
            
            string blockID = $"{colorData.colorType}_Tier{tierData.tierLevel}";
            
            if (!blockDataDictionary.TryGetValue(blockID, out BlockData blockData))
            {
                Debug.LogError($"BlockData not found for {blockID}");
                return null;
            }
            
            return CreateBlock(blockData, worldPosition);
        }
        
        /// <summary>
        /// Create a block from BlockData
        /// </summary>
        public Block CreateBlock(BlockData blockData, Vector3 worldPosition)
        {
            if (blockData == null)
            {
                Debug.LogError("Cannot create block with null BlockData");
                return null;
            }
            
            // Get block from pool or create new
            Block block;
            if (blockPool.Count > 0)
            {
                block = blockPool.Dequeue();
                block.gameObject.SetActive(true);
            }
            else
            {
                block = CreateNewBlockInstance();
            }
            
            // Position and initialize
            block.transform.position = worldPosition;
            
            Vector2Int gridPos = GridManager.Instance.WorldToGrid(worldPosition);
            block.Initialize(blockData, gridPos);
            
            return block;
        }
        
        /// <summary>
        /// Return a block to the pool
        /// </summary>
        public void ReturnToPool(Block block)
        {
            if (block == null) return;
            
            block.gameObject.SetActive(false);
            block.transform.SetParent(blockPoolParent);
            blockPool.Enqueue(block);
        }
        
        /// <summary>
        /// Get BlockData for a specific color and tier
        /// </summary>
        public BlockData GetBlockData(ColorType color, int tier)
        {
            string blockID = $"{color}_Tier{tier}";
            
            if (blockDataDictionary.TryGetValue(blockID, out BlockData blockData))
            {
                return blockData;
            }
            
            return null;
        }
        
        /// <summary>
        /// Get BlockData for the next tier (evolution)
        /// </summary>
        public BlockData GetNextTierBlockData(BlockData current)
        {
            if (current == null || current.tierData == null) return null;
            
            int nextTier = current.tierData.tierLevel + 1;
            if (nextTier > 5) return null; // Max tier is 5
            
            return GetBlockData(current.colorData.colorType, nextTier);
        }
        
        /// <summary>
        /// Get a random Tier 1 BlockData (for spawning)
        /// </summary>
        public BlockData GetRandomTier1BlockData()
        {
            ColorType[] colors = { ColorType.Red, ColorType.Blue, ColorType.Green, ColorType.Yellow, ColorType.Purple };
            ColorType randomColor = colors[Random.Range(0, colors.Length)];
            
            return GetBlockData(randomColor, 1);
        }
    }
}
