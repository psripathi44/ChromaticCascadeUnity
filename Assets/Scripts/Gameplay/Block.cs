using UnityEngine;
using ChromaticCascade.Data;

namespace ChromaticCascade.Gameplay
{
    /// <summary>
    /// Represents a single block in the game with color and tier properties
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Block : MonoBehaviour
    {
        [Header("Block Data")]
        [SerializeField] private BlockData blockData;
        
        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform vfxRoot;
        [SerializeField] private Transform glowOverlay;
        
        [Header("State")]
        [SerializeField] private Vector2Int currentGridPosition;
        [SerializeField] private bool isLocked = false;
        
        // Properties
        public BlockData BlockData => blockData;
        public ColorData ColorData => blockData?.colorData;
        public TierData TierData => blockData?.tierData;
        public Vector2Int GridPosition => currentGridPosition;
        public bool IsLocked => isLocked;
        
        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        /// <summary>
        /// Initialize block with specific block data
        /// </summary>
        public void Initialize(BlockData data, Vector2Int gridPos)
        {
            blockData = data;
            currentGridPosition = gridPos;
            
            UpdateVisuals();
        }
        
        /// <summary>
        /// Update visual appearance based on block data
        /// </summary>
        public void UpdateVisuals()
        {
            if (blockData == null)
            {
                Debug.LogWarning("Block has no BlockData assigned!");
                return;
            }
            
            // Set sprite
            if (spriteRenderer != null && blockData.blockSprite != null)
            {
                spriteRenderer.sprite = blockData.blockSprite;
            }
            
            // Set color (base color * tier brightness)
            if (spriteRenderer != null)
            {
                spriteRenderer.color = blockData.GetRenderColor();
            }
            
            // Update glow overlay if tier has glow
            if (glowOverlay != null && blockData.tierData != null)
            {
                SpriteRenderer glowRenderer = glowOverlay.GetComponent<SpriteRenderer>();
                if (glowRenderer != null)
                {
                    glowRenderer.color = new Color(
                        blockData.GetRenderColor().r,
                        blockData.GetRenderColor().g,
                        blockData.GetRenderColor().b,
                        blockData.tierData.glowIntensity
                    );
                }
            }
        }
        
        /// <summary>
        /// Set the grid position of this block
        /// </summary>
        public void SetGridPosition(Vector2Int newPosition)
        {
            currentGridPosition = newPosition;
        }
        
        /// <summary>
        /// Lock the block in place (can't be moved)
        /// </summary>
        public void Lock()
        {
            isLocked = true;
        }
        
        /// <summary>
        /// Unlock the block (can be moved)
        /// </summary>
        public void Unlock()
        {
            isLocked = false;
        }
        
        /// <summary>
        /// Check if this block matches another block (same color and tier)
        /// </summary>
        public bool Matches(Block other)
        {
            if (other == null || blockData == null || other.blockData == null)
                return false;
            
            return blockData.colorData == other.blockData.colorData &&
                   blockData.tierData == other.blockData.tierData;
        }
        
        /// <summary>
        /// Get the next tier block data (for evolution)
        /// </summary>
        public BlockData GetNextTierBlockData()
        {
            // This will be implemented when we create the BlockFactory
            // For now, return null
            return null;
        }
        
        /// <summary>
        /// Play evolution VFX
        /// </summary>
        public void PlayEvolutionEffect()
        {
            if (blockData?.tierData?.evolutionVFXPrefab != null && vfxRoot != null)
            {
                GameObject vfx = Instantiate(blockData.tierData.evolutionVFXPrefab, vfxRoot.position, Quaternion.identity);
                Destroy(vfx, 3f); // Clean up after 3 seconds
            }
        }
        
        private void OnValidate()
        {
            // Auto-assign components in editor
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw grid position debug info
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}
