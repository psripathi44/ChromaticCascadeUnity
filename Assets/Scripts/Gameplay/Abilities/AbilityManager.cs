using UnityEngine;
using System.Collections.Generic;

namespace ChromaticCascade.Gameplay.Abilities
{
    /// <summary>
    /// Manages tier abilities and their activation
    /// According to gameplay.md: "Tier Abilities: Automatically activate when their triggering match occurs"
    /// </summary>
    public class AbilityManager : MonoBehaviour
    {
        public static AbilityManager Instance { get; private set; }
        
        [Header("Ability Prefabs")]
        [SerializeField] private RowClearAbility rowClearPrefab;
        [SerializeField] private ColorBombAbility colorBombPrefab;
        [SerializeField] private CascadeAbility cascadePrefab;
        [SerializeField] private TimeFreezeAbility timeFreezePrefab;
        
        private Dictionary<int, TierAbility> abilityInstances = new Dictionary<int, TierAbility>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeAbilities();
        }
        
        private void InitializeAbilities()
        {
            // Create ability instances (or use prefabs if assigned)
            if (rowClearPrefab == null)
            {
                rowClearPrefab = gameObject.AddComponent<RowClearAbility>();
            }
            if (colorBombPrefab == null)
            {
                colorBombPrefab = gameObject.AddComponent<ColorBombAbility>();
            }
            if (cascadePrefab == null)
            {
                cascadePrefab = gameObject.AddComponent<CascadeAbility>();
            }
            if (timeFreezePrefab == null)
            {
                timeFreezePrefab = gameObject.AddComponent<TimeFreezeAbility>();
            }
            
            // Set tier levels (CRITICAL - abilities need to know their tier!)
            SetAbilityTierLevel(rowClearPrefab, 2);
            SetAbilityTierLevel(colorBombPrefab, 3);
            SetAbilityTierLevel(cascadePrefab, 4);
            SetAbilityTierLevel(timeFreezePrefab, 5);
            
            // Map tier levels to abilities
            abilityInstances[2] = rowClearPrefab;
            abilityInstances[3] = colorBombPrefab;
            abilityInstances[4] = cascadePrefab;
            abilityInstances[5] = timeFreezePrefab;
            
            Debug.Log("[AbilityManager] Initialized all tier abilities");
        }
        
        /// <summary>
        /// Set tier level on ability using reflection (since tierLevel is protected)
        /// </summary>
        private void SetAbilityTierLevel(TierAbility ability, int tier)
        {
            if (ability == null) return;
            
            var field = typeof(TierAbility).GetField("tierLevel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(ability, tier);
        }
        
        /// <summary>
        /// Trigger ability for a specific tier
        /// Called when evolved blocks of that tier are created from a match
        /// </summary>
        public void TriggerAbility(int tierLevel, Vector2Int centerPos, List<Block> matchedBlocks)
        {
            if (abilityInstances.TryGetValue(tierLevel, out TierAbility ability))
            {
                Debug.Log($"[AbilityManager] Triggering Tier {tierLevel} ability");
                ability.Activate(centerPos, matchedBlocks);
            }
            else
            {
                Debug.LogWarning($"[AbilityManager] No ability found for Tier {tierLevel}");
            }
        }
        
        /// <summary>
        /// Check if a specific tier has an ability
        /// </summary>
        public bool HasAbility(int tierLevel)
        {
            return abilityInstances.ContainsKey(tierLevel);
        }
    }
}
