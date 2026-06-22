# 3D Carrom - Session State

## Goal
Build a working 3D Carrom Android game using Unity 6 cloud builds. Must install and run on ALL Android phones (old + latest flagships).

## Environment
- Unity: 6000.5.0f1 (cloud: 6000.4.10f1)
- Build target: Android (IL2CPP, ARM64+ARMv7)
- Repo: https://github.com/moldokipgen23/3D-CARROM.git
- Cloud build: Unity Build Automation (Build #9 last triggered)

## What Was Fixed (all pushed to main)

### Compilation Errors (Builds #1-6) - ALL FIXED
- Removed `com.unity.inputsystem` from manifest.json
- Regenerated 133+ .meta files with valid GUIDs
- Fixed EditorBuildSettings scene GUIDs
- `PhysicMaterial` -> `PhysicsMaterial` (4 files)
- `Rigidbody.sharedMaterial` -> `Collider.sharedMaterial` (4 files)
- `Material.smoothness` -> `SetFloat("_Smoothness")` (Coin.cs)
- `PrimitiveType.Torus` -> custom torus mesh (BoardSetup.cs)
- Added `using System` (PlayerStats.cs)
- Fixed field names (CoinSpawnerTest.cs)
- Added `CheckQueenPocketed()` (FoulDetector.cs)
- Fixed `ref` params in iterators (PhysicsTuner.cs)
- Fixed `Vector3`/`Vector2` ambiguity (AIPlayer.cs)
- **Build #6 SUCCEEDED**

### Android Compatibility (Builds #7-8) - FIXED
- `AndroidTargetArchitectures: 1` -> `3` (ARM64+ARMv7) -- was causing "not compatible"
- `AndroidTargetSdkVersion: 34` -> `35` (Android 15)
- `AndroidBundleVersionCode: 1` -> `2`

### Scene/Orientation Fix (Build #9) - PARTIALLY FIXED
- Rewrote Boot.unity YAML: attached BootSceneInitializer to BootLoader GameObject
- Added AudioListener to Boot camera
- `defaultScreenOrientation: 1` -> `0` (AutoRotation)

## CURRENT BLOCKER: Black Screen After Unity Logo

### What We Know
- Build #9 compiles and installs fine
- After Unity splash screen, app shows black screen forever
- BootSceneInitializer IS attached to a GameObject now (was the first issue)
- Script should: init Firebase (stub) -> wait 2s -> load MainMenu

### Probable Causes (to investigate next session)
1. **BootSceneInitializer.Start() may throw** before LoadMainMenuAfterDelay coroutine runs
   - `ServiceLocator.Register<FirebaseService>(firebaseService.Service)` could fail
   - `FirebaseServiceBridge` component creation could fail
2. **SceneFlow.LoadScene may silently fail** - it has error logging but no visible UI feedback
3. **Target scenes are EMPTY** - MainMenu, Game, Results all just have cameras with no scripts/UI
   - MainMenu.unity: only Camera + Light (no MainMenuManager attached)
   - Game.unity: only Camera + empty "Game Manager Placeholder" (no BoardSetup, CoinSpawner, etc.)
   - Results.unity: only Camera (no ResultsScreen or ResultsSceneManager attached)
4. **The entire game was rebuilt from scratch** via script - scenes never had real game objects added back

### Next Session Action Plan

**Step 1: Simplify BootSceneInitializer**
- Remove Firebase dependency entirely
- Make it just: `Start() { SceneManager.LoadScene("MainMenu"); }`
- Or add a visible "Loading..." UI text so we can see if it's running

**Step 2: Fix MainMenu scene**
- Attach MainMenuManager to a GameObject
- Add a Canvas with Play button
- MainMenuManager.cs needs references to UI buttons

**Step 3: Fix Game scene**
- Attach BoardSetup to create the carrom board
- Attach CoinSpawner to spawn coins
- Attach StrikerController
- Attach GameManager, TurnManager, ScoreManager
- Attach FoulDetector, PhysicsTuner
- All these scripts exist in code but nothing references them in the scene

**Step 4: Fix Results scene**
- Attach ResultsScreen or ResultsSceneManager
- Add Canvas with score display and Main Menu button

**Step 5: Test with Debug.Log**
- Add Debug.Log at every step to trace where it stops

## Key Files

### Scenes (ALL need work)
- `Assets/_Project/Scenes/Boot.unity` - has BootLoader with BootSceneInitializer, still black
- `Assets/_Project/Scenes/MainMenu.unity` - EMPTY (camera only)
- `Assets/_Project/Scenes/Game.unity` - EMPTY (camera + empty placeholder)
- `Assets/_Project/Scenes/Results.unity` - EMPTY (camera only)

### Core Scripts (exist but not attached to scenes)
- `Assets/_Project/Scripts/Core/BootSceneInitializer.cs` - boot logic, Firebase dependent
- `Assets/_Project/Scripts/Core/SceneFlow.cs` - scene loading helper
- `Assets/_Project/Scripts/Core/GameManager.cs` - game state
- `Assets/_Project/Scripts/UI/MainMenuManager.cs` - main menu UI
- `Assets/_Project/Scripts/UI/ResultsScreen.cs` - results UI
- `Assets/_Project/Scripts/UI/ResultsSceneManager.cs` - results scene logic
- `Assets/_Project/Scripts/UI/GameSceneManager.cs` - game scene logic

### Gameplay Scripts (exist but not in Game scene)
- `Assets/_Project/Scripts/Gameplay/Board/BoardSetup.cs` - creates board geometry
- `Assets/_Project/Scripts/Gameplay/Coins/CoinSpawner.cs` - spawns coins
- `Assets/_Project/Scripts/Gameplay/Coins/Coin.cs` - coin behavior
- `Assets/_Project/Scripts/Gameplay/Striker/StrikerController.cs` - striker control
- `Assets/_Project/Scripts/Gameplay/PhysicsTuner.cs` - physics testing
- `Assets/_Project/Scripts/Gameplay/Rules/FoulDetector.cs` - foul detection
- `Assets/_Project/Scripts/Gameplay/AI/AIPlayer.cs` - AI opponent

### Config
- `ProjectSettings/ProjectSettings.asset` - Android: ARM64+ARMv7, API 35, AutoRotation, IL2CPP
- `ProjectSettings/EditorBuildSettings.asset` - 4 scenes listed with correct GUIDs
- `Packages/manifest.json` - no inputsystem

## Key Decisions Made
- Input System removed (simpler than fixing input code)
- Firebase wrapped in `#if FIREBASE_SDK` - compiles without SDK (stub mode)
- IL2CPP backend for ARM64 support
- Custom torus mesh instead of PrimitiveType.Torus (doesn't exist in Unity 6)
- AutoRotation orientation (was Portrait, user complained upside down)
