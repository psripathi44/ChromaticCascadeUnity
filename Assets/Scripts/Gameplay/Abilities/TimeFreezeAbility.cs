using UnityEngine;
using ChromaticCascade.Core;
using ChromaticCascade.Scoring;
using System.Collections;
using System.Collections.Generic;

namespace ChromaticCascade.Gameplay.Abilities
{
    /// <summary>
    /// Tier 5 Ability: Freezes falling blocks for 10 seconds
    /// From gameplay.md: "Freezes falling blocks for 10 seconds"
    /// </summary>
    public class TimeFreezeAbility : TierAbility
    {
        [Header("Freeze Settings")]
        [SerializeField] private float freezeDuration = 10f;
        
        private static bool isFrozen = false;
        private static float freezeEndTime = 0f;
        
        public static bool IsFrozen => isFrozen && Time.time < freezeEndTime;
        
        public override void Activate(Vector2Int centerPos, List<Block> matchedBlocks)
        {
            Debug.Log($"[Tier 5 Ability] TIME FREEZE for {freezeDuration} seconds");
            
            // Activate freeze
            isFrozen = true;
            freezeEndTime = Time.time + freezeDuration;
            
            // Play VFX
            Vector3 worldPos = GridManager.Instance.GridToWorld(centerPos);
            PlayAbilityVFX(worldPos);
            
            // Award points
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddAbilityPoints(500, 0);
                Debug.Log($"[Time Freeze] Activated, +500 points");
            }
            
            // Start countdown coroutine
            StartCoroutine(FreezeCountdown());
        }
        
        private IEnumerator FreezeCountdown()
        {
            yield return new WaitForSeconds(freezeDuration);
            
            isFrozen = false;
            Debug.Log("[Time Freeze] Ended");
        }
        
        /// <summary>
        /// Check if time is currently frozen (for other systems to query)
        /// </summary>
        public static bool IsTimeFrozen()
        {
            return IsFrozen;
        }
        
        /// <summary>
        /// Get remaining freeze time
        /// </summary>
        public static float GetRemainingFreezeTime()
        {
            if (!IsFrozen) return 0f;
            return Mathf.Max(0f, freezeEndTime - Time.time);
        }
    }
}
