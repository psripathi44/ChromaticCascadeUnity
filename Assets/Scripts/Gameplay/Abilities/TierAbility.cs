using UnityEngine;
using ChromaticCascade.Core;
using System.Collections.Generic;

namespace ChromaticCascade.Gameplay.Abilities
{
    /// <summary>
    /// Base class for tier abilities
    /// According to gameplay.md: "Tier Abilities: Automatically activate when their triggering match occurs"
    /// </summary>
    public abstract class TierAbility : MonoBehaviour
    {
        [Header("Ability Settings")]
        [SerializeField] protected int tierLevel;
        [SerializeField] protected float activationDelay = 0.3f;
        
        /// <summary>
        /// Activate the ability at the specified position
        /// </summary>
        /// <param name="centerPos">The grid position where the evolved block is created</param>
        /// <param name="matchedBlocks">The blocks that triggered this evolution</param>
        public abstract void Activate(Vector2Int centerPos, List<Block> matchedBlocks);
        
        /// <summary>
        /// Get the tier level this ability belongs to
        /// </summary>
        public int TierLevel => tierLevel;
        
        /// <summary>
        /// Play VFX for this ability
        /// </summary>
        protected void PlayAbilityVFX(Vector3 worldPos)
        {
            // TODO: Implement VFX spawning when we have VFX prefabs
            Debug.Log($"[Tier {tierLevel} Ability] VFX at {worldPos}");
        }
    }
}
