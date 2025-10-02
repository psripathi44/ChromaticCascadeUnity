using UnityEngine;
using UnityEditor;
using System.IO;
using ChromaticCascade.Data;

namespace ChromaticCascade.Editor
{
    /// <summary>
    /// Editor utility to create all game data assets
    /// </summary>
    public class DataAssetCreator : EditorWindow
    {
        [MenuItem("Chromatic Cascade/Create All Data Assets")]
        public static void CreateAllDataAssets()
        {
            CreateColorDataAssets();
            CreateTierDataAssets();
            CreateBlockDataAssets();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("✅ All data assets created successfully!");
        }
        
        [MenuItem("Chromatic Cascade/Create Color Data Assets")]
        public static void CreateColorDataAssets()
        {
            string path = "Assets/Data/Colors";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            // Red
            CreateColorData(path, "ColorData_Red", ColorType.Red, new Color32(255, 75, 75, 255));
            
            // Blue
            CreateColorData(path, "ColorData_Blue", ColorType.Blue, new Color32(75, 138, 255, 255));
            
            // Green
            CreateColorData(path, "ColorData_Green", ColorType.Green, new Color32(68, 208, 122, 255));
            
            // Yellow
            CreateColorData(path, "ColorData_Yellow", ColorType.Yellow, new Color32(247, 201, 72, 255));
            
            // Purple
            CreateColorData(path, "ColorData_Purple", ColorType.Purple, new Color32(160, 91, 255, 255));
            
            Debug.Log("✅ Created 5 Color Data assets in " + path);
        }
        
        [MenuItem("Chromatic Cascade/Create Tier Data Assets")]
        public static void CreateTierDataAssets()
        {
            string path = "Assets/Data/Tiers";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            // Tier 1
            CreateTierData(path, "TierData_1", 1, 1.0f, 0.0f, AbilityType.None, 10);
            
            // Tier 2
            CreateTierData(path, "TierData_2", 2, 1.2f, 0.3f, AbilityType.RowClear, 25);
            
            // Tier 3
            CreateTierData(path, "TierData_3", 3, 1.4f, 0.5f, AbilityType.ColorBomb, 50);
            
            // Tier 4
            CreateTierData(path, "TierData_4", 4, 1.7f, 0.7f, AbilityType.Cascade, 100);
            
            // Tier 5
            CreateTierData(path, "TierData_5", 5, 2.0f, 1.0f, AbilityType.TimeFreeze, 250);
            
            Debug.Log("✅ Created 5 Tier Data assets in " + path);
        }
        
        [MenuItem("Chromatic Cascade/Create Block Data Assets")]
        public static void CreateBlockDataAssets()
        {
            string path = "Assets/Data/Blocks";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            // Load Color and Tier data
            ColorData[] colors = LoadAllAssets<ColorData>("Assets/Data/Colors");
            TierData[] tiers = LoadAllAssets<TierData>("Assets/Data/Tiers");
            
            if (colors.Length == 0 || tiers.Length == 0)
            {
                Debug.LogError("❌ Please create Color and Tier data assets first!");
                return;
            }
            
            int count = 0;
            foreach (var color in colors)
            {
                foreach (var tier in tiers)
                {
                    string fileName = $"BlockData_{color.colorType}_Tier{tier.tierLevel}";
                    CreateBlockData(path, fileName, color, tier);
                    count++;
                }
            }
            
            Debug.Log($"✅ Created {count} Block Data assets in " + path);
        }
        
        // Helper Methods
        private static void CreateColorData(string path, string fileName, ColorType colorType, Color32 baseColor)
        {
            string fullPath = $"{path}/{fileName}.asset";
            
            // Check if already exists
            ColorData existing = AssetDatabase.LoadAssetAtPath<ColorData>(fullPath);
            if (existing != null)
            {
                Debug.Log($"⚠️ {fileName} already exists, skipping...");
                return;
            }
            
            ColorData asset = ScriptableObject.CreateInstance<ColorData>();
            asset.colorType = colorType;
            asset.baseColor = baseColor;
            asset.pattern = (AccessibilityPattern)((int)colorType); // Assign different patterns
            
            AssetDatabase.CreateAsset(asset, fullPath);
            Debug.Log($"Created: {fileName}");
        }
        
        private static void CreateTierData(string path, string fileName, int tierLevel, float brightness, float glow, AbilityType ability, int evolutionBonus)
        {
            string fullPath = $"{path}/{fileName}.asset";
            
            // Check if already exists
            TierData existing = AssetDatabase.LoadAssetAtPath<TierData>(fullPath);
            if (existing != null)
            {
                Debug.Log($"⚠️ {fileName} already exists, skipping...");
                return;
            }
            
            TierData asset = ScriptableObject.CreateInstance<TierData>();
            asset.tierLevel = tierLevel;
            asset.brightnessMultiplier = brightness;
            asset.glowIntensity = glow;
            asset.abilityType = ability;
            asset.evolutionBonus = evolutionBonus;
            
            AssetDatabase.CreateAsset(asset, fullPath);
            Debug.Log($"Created: {fileName}");
        }
        
        private static void CreateBlockData(string path, string fileName, ColorData color, TierData tier)
        {
            string fullPath = $"{path}/{fileName}.asset";
            
            // Check if already exists
            BlockData existing = AssetDatabase.LoadAssetAtPath<BlockData>(fullPath);
            if (existing != null)
            {
                return; // Skip silently for blocks
            }
            
            BlockData asset = ScriptableObject.CreateInstance<BlockData>();
            asset.colorData = color;
            asset.tierData = tier;
            asset.minBlocksToEvolve = 4; // Default: 4+ blocks to evolve
            asset.allows2x2Evolution = true;
            
            AssetDatabase.CreateAsset(asset, fullPath);
        }
        
        private static T[] LoadAllAssets<T>(string path) where T : Object
        {
            if (!Directory.Exists(path))
                return new T[0];
            
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { path });
            T[] assets = new T[guids.Length];
            
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }
            
            return assets;
        }
    }
}
