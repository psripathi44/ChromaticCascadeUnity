# Chromatic Cascade - Sprint 1 Implementation

## 🎮 Overview
Sprint 1 establishes the foundation of Chromatic Cascade with basic block placement, evolution mechanics, and UI framework.

## 📁 Project Structure

```
Assets/
├── Art/
│   ├── Sprites/          # Block sprites and visual assets
│   ├── Materials/        # Materials for rendering
│   └── VFX/              # Visual effects prefabs
├── Audio/
│   ├── Music/            # Background music
│   └── SFX/              # Sound effects
├── Data/
│   ├── Colors/           # ColorData ScriptableObjects
│   ├── Tiers/            # TierData ScriptableObjects
│   └── Modes/            # GameModeData ScriptableObjects
├── Prefabs/
│   ├── Blocks/           # Block prefab variants (25 types)
│   └── UI/               # UI prefabs
├── Scenes/
│   ├── MainMenu          # Main menu scene
│   └── Gameplay          # Main gameplay scene
└── Scripts/
    ├── Core/             # Core systems (Grid, GameState)
    ├── Gameplay/         # Gameplay logic (Blocks, Evolution)
    ├── UI/               # UI managers and displays
    ├── Data/             # ScriptableObject definitions
    └── Utilities/        # Helper classes
```

## ✅ Implemented Features (Sprint 1)

### Core Systems
- ✅ **GridManager**: 7×10 grid with world/grid coordinate conversion
- ✅ **GridCell**: Cell data structure with occupancy tracking
- ✅ **GameStateManager**: Game state management (Menu, Playing, Paused, GameOver)

### Data Architecture
- ✅ **ColorData**: 5 colors with accessibility patterns
- ✅ **TierData**: 5 tiers with brightness multipliers and abilities
- ✅ **BlockData**: 25 unique block types (5 colors × 5 tiers)
- ✅ **AbilityData**: Tier ability definitions
- ✅ **PowerUpData**: Power-up definitions
- ✅ **GameModeData**: Game mode configurations

### Block System
- ✅ **Block**: Block component with color/tier properties
- ✅ **BlockFactory**: Object pooling and block creation
- ✅ **BlockSpawner**: Automatic block spawning at grid top
- ✅ **FallingBlock**: Automatic falling with lock delay
- ✅ **PlayerController**: Input handling (move left/right, soft drop, hard drop)

### Evolution System
- ✅ **EvolutionDetector**: Flood-fill algorithm for 4+ matches and 2×2 detection
- ✅ **EvolutionResolver**: Merge blocks into higher tiers with cascade support

### UI
- ✅ **UIManager**: Main HUD manager (score, combo, next block, pause)
- ✅ **ScoreDisplay**: Animated score counter with count-up effect

## 🛠️ Setup Instructions

### 1. Package Installation (Required)
Install these packages via Unity Package Manager (Window > Package Manager):

1. **TextMeshPro** (if not already installed)
   - Import TMP Essentials when prompted
   
2. **Addressables** (Optional for Sprint 1, required later)
   - com.unity.addressables
   
3. **Unity IAP** (Optional for Sprint 1, required Sprint 6)
   - com.unity.purchasing

### 2. Input System Setup
The project uses the new Unity Input System:

1. Open `Assets/InputSystem_Actions.inputactions`
2. Add a "Gameplay" Action Map with these actions:
   - **Move** (Value, Float) - Horizontal movement
   - **SoftDrop** (Button) - Hold to accelerate fall
   - **HardDrop** (Button) - Instant drop

3. Assign controls:
   - **Move**: Keyboard (A/D or Arrow Keys), Gamepad (Left Stick X)
   - **SoftDrop**: Keyboard (S or Down Arrow), Gamepad (Down)
   - **HardDrop**: Keyboard (Space), Gamepad (A Button)

### 3. Scene Setup

#### Create Gameplay Scene:
1. Create new scene: `Assets/Scenes/Gameplay.unity`
2. Add these GameObjects:

**Grid System:**
- Empty GameObject: "GameManager"
  - Add Component: `GridManager`
  - Set Grid Width: 7
  - Set Grid Height: 10
  - Set Cell Size: 1
  - Set Grid Origin: (-3.5, -5, 0) [centers grid in view]

**Block Management:**
- Empty GameObject: "BlockFactory" (child of GameManager)
  - Add Component: `BlockFactory`
  - Assign Default Block Prefab (create a simple cube prefab)
  
- Empty GameObject: "BlockSpawner" (child of GameManager)
  - Add Component: `BlockSpawner`
  - Set Spawn Interval: 1.5
  
**Evolution System:**
- Empty GameObject: "EvolutionDetector" (child of GameManager)
  - Add Component: `EvolutionDetector`
  
- Empty GameObject: "EvolutionResolver" (child of GameManager)
  - Add Component: `EvolutionResolver`

**Player Control:**
- Empty GameObject: "PlayerController" (child of GameManager)
  - Add Component: `PlayerController`
  - Assign Input Actions asset

**UI:**
- Canvas: "GameplayUI"
  - Canvas Scaler: Scale with Screen Size (1080×1920)
  - Add Component: `UIManager`
  - Create child TextMeshPro objects for:
    - Score display
    - Combo display
    - Next block preview
    - Pause button

**Camera:**
- Main Camera:
  - Position: (0, 2.5, -10)
  - Size (Orthographic): 6

### 4. Create Block Data Assets

#### Color Data (Create 5 assets in `Assets/Data/Colors/`):
1. Right-click > Create > Chromatic Cascade > Data > Color Data
2. Create for each color:
   - Red: #FF4B4B
   - Blue: #4B8AFF
   - Green: #44D07A
   - Yellow: #F7C948
   - Purple: #A05BFF

#### Tier Data (Create 5 assets in `Assets/Data/Tiers/`):
1. Right-click > Create > Chromatic Cascade > Data > Tier Data
2. Create for each tier:
   - Tier 1: Brightness 1.0, No Ability
   - Tier 2: Brightness 1.2, Row Clear Ability
   - Tier 3: Brightness 1.4, Color Bomb Ability
   - Tier 4: Brightness 1.7, Cascade Ability
   - Tier 5: Brightness 2.0, Time Freeze Ability

#### Block Data (Create 25 assets linking colors and tiers):
- Naming: `BlockData_Red_Tier1`, etc.
- Assign corresponding ColorData and TierData to each

### 5. Create Block Prefabs

1. Create a basic block prefab:
   - GameObject with Sprite Renderer
   - Add Component: `Block`
   - Add child GameObject: "VFX_Root"
   - Add child GameObject: "Glow_Overlay" with SpriteRenderer

2. Assign to BlockFactory's Default Block Prefab field

### 6. Build Settings
1. File > Build Settings
2. Add Scenes:
   - MainMenu (create placeholder scene)
   - Gameplay
3. Select platform: iOS or Android
4. Switch Platform

## 🎮 Controls

### Keyboard:
- **A / Left Arrow**: Move left
- **D / Right Arrow**: Move right
- **S / Down Arrow**: Soft drop (hold to accelerate)
- **Spacebar**: Hard drop (instant placement)
- **Escape**: Pause

### Touch (Mobile):
- **Swipe Left/Right**: Move block
- **Tap**: Hard drop
- **Swipe Down**: Soft drop

## 🐛 Known Issues / TODO

- [ ] Create placeholder sprites for blocks (currently need visual assets)
- [ ] Implement Main Menu scene
- [ ] Add evolution VFX particle effects
- [ ] Add sound effects for block placement and evolution
- [ ] Implement score system (Sprint 2)
- [ ] Implement save system (Sprint 2)
- [ ] Fine-tune lock delay timing
- [ ] Add game over detection and UI

## 📝 Next Steps (Sprint 2)

1. Implement complete scoring system with combo multipliers
2. Build save/load system with cloud sync
3. Create game mode configuration system
4. Implement difficulty scaling
5. Add game over and retry flow
6. Create pause menu

## 🔧 Debugging

### Grid Visualization:
- Select GridManager in Hierarchy
- Check "Show Grid Gizmos" in Inspector
- Green = empty cells, Red = occupied cells

### Evolution Detection:
- Select EvolutionDetector
- Check "Debug Visualization"
- Matched blocks highlighted in yellow

## 💡 Tips

1. **Testing Evolution**: Place multiple blocks of same color manually in Scene view to test matching
2. **Adjusting Grid**: Modify GridManager's Grid Origin to center grid in camera view
3. **Performance**: BlockFactory uses object pooling - adjust Initial Pool Size based on needs
4. **Input Testing**: Test keyboard controls first, then configure touch controls

## 📚 Architecture Notes

- **Singleton Pattern**: Core managers use singleton for global access
- **Object Pooling**: Blocks are pooled to reduce GC pressure
- **Event-Driven**: Evolution system uses coroutines and events for cascades
- **Data-Driven**: All game balance in ScriptableObjects for designer control

---

**Sprint 1 Status**: ✅ Foundation Complete
**Next Sprint**: Sprint 2 - Scoring & Persistence
