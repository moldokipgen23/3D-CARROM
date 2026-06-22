# CARROM GAMES 3D — AI AGENT BUILD SPEC

## ⚠️ CRITICAL — READ THIS BEFORE ANYTHING ELSE

**This project already exists with ~88 C# scripts, 4 scenes, and a full folder
structure. It is NOT a blank project. Do NOT start from scratch.**

The game compiles and runs but is NOT playable — core gameplay bugs prevent the
game loop from completing. The fix plan is in `TODO_FIX.md` at the project root.

**Correct order of operations for any AI agent:**
1. Read `TODO_FIX.md` first. Fix all Tier 1 and Tier 2 items.
2. Verify the game is playable (acceptance gate in TODO_FIX.md passes).
3. Then use this README to implement any remaining missing batches.

**"The project compiles with no errors" does NOT mean the game works.**
DeepSeek, Qwen, and similar agents have reported the project as "complete" based
on compile success alone. This is wrong. The game does not play. Use the
acceptance checks in TODO_FIX.md — not console output — as the definition of done.

**Before creating any file listed in a batch below:**
Check if that file already exists at `Assets\_Project\Scripts\`. If it does,
READ it first and only add what is missing. Do NOT overwrite existing files.

---

## HOW TO USE THIS DOCUMENT

Paste **one batch at a time** to your coding agent. Do not paste the whole document.
Each batch is self-contained — it tells the agent what exists already, what to build,
what files to create, and how to know it's done. Move to the next batch only after
the current one passes its acceptance criteria.

---

## BATCH 0 — PROJECT FOUNDATION

**Status: ALREADY COMPLETE. Skip this batch.**

The project already has:
- Unity project with 4 scenes: Boot, MainMenu, Game, Results
- Folder structure under `Assets/_Project/`
- `.gitignore` for Unity
- Android target minSdkVersion 24, targetSdkVersion 35
- Git repo initialized

**If verifying from scratch:**
- [ ] Project opens with no console errors
- [ ] All 4 scenes exist in Build Settings in order: Boot(0) MainMenu(1) Game(2) Results(3)
- [ ] Android build (.apk or .aab) completes without error
- [ ] iOS build (Xcode project export) completes without error

---

## BATCH 1 — CORE ARCHITECTURE

**Status: ALREADY COMPLETE. Verify only, do not recreate.**

Existing files (read before touching):
- `Core/GameManager.cs` — singleton, holds GameState enum
- `Core/IService.cs` — marker interface (intentionally empty)
- `Core/ServiceLocator.cs` — Register/Get/Unregister pattern
- `Data/PlayerData.cs` — plain C# DTO
- `Data/GameSettings.cs` — ScriptableObject for audio/vibration

**Acceptance criteria (verify these, do not rebuild):**
- [ ] GameManager persists across scene loads (DontDestroyOnLoad)
- [ ] ServiceLocator can register and retrieve a service
- [ ] No MonoBehaviour uses FindObjectOfType for cross-system communication

---

## BATCH 2 — FIREBASE SETUP

**Goal:** Working Firebase backend connected to the Unity project.

**Current state:** `Networking/FirebaseService.cs` exists but all functionality is
behind `#if FIREBASE_SDK`. The SDK has NOT been imported. The file is a stub.

**Steps for agent:**
1. Download Firebase Unity SDK from the Firebase console
   (Auth, Firestore, Analytics, Crashlytics, Remote Config packages).
2. Import each `.unitypackage` into the project via Assets → Import Package.
3. **Important:** Importing the Firebase SDK automatically defines `FIREBASE_SDK`
   as a scripting symbol in Player Settings. You do NOT need to add this define
   manually — the SDK sets it. Once imported, the `#if FIREBASE_SDK` blocks in
   `FirebaseService.cs` will compile and activate.
4. Place `google-services.json` (Android) into `Assets/` root.
5. Place `GoogleService-Info.plist` (iOS) into `Assets/` root.
6. Do NOT rewrite `FirebaseService.cs` — the implementation is already there,
   gated behind `#if FIREBASE_SDK`. It will work once the SDK is imported.
7. Enable Crashlytics, set up a test non-fatal log to confirm it reports.

**Firestore schema:**
```
players/{playerId}
  username: string
  coins: number
  diamonds: number
  xp: number
  level: number
  createdAt: timestamp
  lastLogin: timestamp
```

**Acceptance criteria:**
- [ ] App launches, signs in anonymously, writes a player doc to Firestore (verify in Firebase Console)
- [ ] Crashlytics dashboard shows the test event
- [ ] No Firebase init errors in console on cold start
- [ ] The `#if FIREBASE_SDK` define is active (check: Project Settings → Player → Scripting Define Symbols)

---

## BATCH 3 — CARROM BOARD (3D)

**Status: ALREADY COMPLETE. `BoardSetup.cs` procedurally generates the full board.**

**Do NOT recreate the board. Read `BoardSetup.cs` before touching anything.**

**⚠️ RENDER PIPELINE WARNING:**
This project uses the **Standard (Built-in) render pipeline**, NOT URP.
A previous version of this spec said "URP Lit shader" — that was wrong and caused
a black screen bug (fixed in commit `fef2161`).

- Use `Standard` shader for all materials, NOT `Universal Render Pipeline/Lit`
- The `_Glossiness` property controls smoothness in Standard shader (NOT `_Smoothness`)
- Do NOT switch the project to URP — it will break all existing materials

**Existing files (verify, do not recreate):**
- `Gameplay/Board/BoardSetup.cs` — builds table, frame, pockets, center, baselines
- `Gameplay/Board/BoardController.cs` — holds pocket references, fires CoinPocketed event
- `Gameplay/Board/PocketTrigger.cs` — OnTriggerEnter for coin detection

**Known bug in PocketTrigger.cs — fix this now:**
PocketTrigger detects coins entering it but does NOT call `coin.Pocket()` or update
`ScoreManager`. This is FIX-01 in `TODO_FIX.md`. Fix it before marking this batch done.

**Acceptance criteria:**
- [ ] Board visible in Game scene with correct proportions
- [ ] Dropping a coin into each pocket triggers the OnCoinPocketed event (check Console)
- [ ] Score updates when a coin enters a pocket (NOT just the trigger firing)
- [ ] No black screen — all materials use Standard shader
- [ ] No console errors

---

## BATCH 4 — COINS

**Status: ALREADY COMPLETE. Files exist. Verify physics setup.**

**Existing files:**
- `Gameplay/Coins/CoinType.cs` — enum: White, Black, Queen
- `Gameplay/Coins/Coin.cs` — MonoBehaviour with Type, IsPocketed, Pocket()
- `Gameplay/Coins/CoinSpawner.cs` — spawns 19 coins in official starting formation

**Physics spec (verify these values in Inspector or code, adjust if wrong):**
- Coin: cylinder, diameter ~0.032 units, height ~0.005 units
- Mass: 5 for all coins (white, black, queen)
- Physics Material: Dynamic Friction 0.3, Static Friction 0.3, Bounciness 0.1,
  Friction Combine = Minimum, Bounce Combine = Minimum
- Rigidbody: `useGravity = true`, Y-position locked via `RigidbodyConstraints`
  (do NOT disable gravity — gravity is needed for friction to work on the board plane)

**Known bug in Coin.cs — verify this is correct:**
`Pocket()` method must set `IsPocketed = true`, zero out velocity, set
`isKinematic = true`, and call `gameObject.SetActive(false)`. If any of these
are missing, fix them (see FIX-01 in TODO_FIX.md).

**Acceptance criteria:**
- [ ] 19 coins spawn in correct official starting position (queen in center)
- [ ] Coins rest stably — no jittering or sliding on their own
- [ ] A pocketed coin disappears from the board and does not re-trigger pockets
- [ ] Coin materials use Standard shader (not URP Lit)

---

## BATCH 5 — STRIKER + SHOOTING MECHANIC

**Status: MOSTLY COMPLETE. Known bug in AimIndicator.**

**Existing files:**
- `Gameplay/Striker/StrikerController.cs` — aim/drag/force logic
- `Gameplay/Striker/AimIndicator.cs` — LineRenderer visual (has a bug — see below)
- `Gameplay/Camera/CarromCameraController.cs` — smooth camera follow

**Known bug in AimIndicator.cs:**
Power percentage is hardcoded as `0.5f` — it never reads actual shot power from
StrikerController. Fix:
```csharp
// Wrong (current code):
float powerPct = 0.5f;

// Correct — read from StrikerController:
float powerPct = StrikerController.Instance != null
    ? StrikerController.Instance.CurrentPowerNormalized  // 0.0 to 1.0
    : 0f;
```
Add `public float CurrentPowerNormalized { get; private set; }` to StrikerController,
updated during drag. Dot/line color and length should scale with this value.

**Striker physics spec:**
- Diameter: ~0.041 units, mass: 8 (heavier than coins)
- Constrained to player's baseline strip during aim phase
- Force range: minForce = 2, maxForce = 15 (starting values — Batch 6 tunes these)
- ForceMode.Impulse

**Camera:**
- Tilted top-down, ~55° from vertical
- Positioned behind the active player's baseline
- Smoothly transitions side when turn changes

**Acceptance criteria:**
- [ ] Drag shows a visible aim line that scales with drag distance
- [ ] Aim line color or length changes based on power (not hardcoded)
- [ ] Striker cannot be placed outside the legal baseline zone
- [ ] Releasing applies force in the correct direction proportional to drag distance
- [ ] **Manual feel-test required — code passing alone is not sufficient**

---

## BATCH 6 — PHYSICS TUNING PASS

**Goal:** Make it feel like real carrom. This is QA, not new features.

**⚠️ MULTIPLAYER SYNC REQUIREMENT — do this first:**
Before tuning, set `Edit → Project Settings → Time → Fixed Timestep` to exactly
`0.02` (50Hz). Do NOT change this value after this batch. Multiplayer sync
(Batch 9) depends on both clients running identical Fixed Timestep — any change
after Batch 9 is implemented will desync clients.

**Process for agent (iterative — must do all 3 passes):**

**Pass 1:**
1. Play 10 test shots at varying power levels (min, mid, max)
2. Check: do coins glide and decelerate naturally, or stop too abruptly / slide forever?
3. Check: does a direct hit on a coin cluster transfer force realistically?
4. Log observations — do NOT adjust yet

**Pass 2:**
5. Adjust Dynamic Friction ±0.05 steps based on Pass 1 observations
6. Adjust Bounciness ±0.05 steps
7. Play another 10 shots
8. Log new observations

**Pass 3:**
9. Adjust striker mass and force range if shots feel globally too weak or too strong
10. Verify pocket capture: a coin moving over a pocket should fall in without
    requiring pixel-perfect centering, but pockets should not act as magnets
11. Final 10 shots — confirm feel is satisfactory

**Acceptance criteria:**
- [ ] Fixed Timestep is set to 0.02 and documented in ProjectSettings
- [ ] A soft-tap shot moves striker a few units and stops naturally (not instant, not forever)
- [ ] A full-power shot can break a clustered formation realistically
- [ ] All 3 passes logged with adjustments noted (prevents marking done after one pass)

---

## BATCH 7 — SINGLE PLAYER GAME LOOP

**Status: FILES EXIST BUT BROKEN. Do not recreate — fix the existing files.**

**Read `TODO_FIX.md` before this batch. FIX-01 through FIX-09 all apply here.**

**Existing files (fix, do not recreate):**
- `Gameplay/Rules/TurnManager.cs` — has EndShot() but switches too early (FIX-02)
- `Gameplay/Rules/FoulDetector.cs` — RegisterShot() never called (FIX-09)
- `Gameplay/Rules/ScoreManager.cs` — queen cover logic wrong, enum mismatch (FIX-03)
- `UI/GameHUD.cs` — updates every frame instead of on events (FIX-07)
- `UI/ResultsScreen.cs` — never shown on win, auto-dismisses (FIX-06)

**The acceptance criteria below are the definition of done. "No compile errors"
is not done. "No console errors" is not done. These gameplay checks must pass:**

**Acceptance criteria:**
- [ ] Two players can play a full game locally, start to finish, without softlocking
- [ ] Turn switches ONLY after all Rigidbodies on the board reach near-zero velocity
- [ ] Fouls are detected: striker pocketed, no coin touched, queen without cover
- [ ] Foul penalty applies: last pocketed coin returned to board OR point deducted
- [ ] Queen cover rule works: pocket queen + pocket own coin same turn = queen awarded;
      pocket queen + fail to cover = queen returned to center
- [ ] Game correctly identifies winner and shows ResultsScreen
- [ ] ResultsScreen stays visible until player presses a button
- [ ] Both "Play Again" and "Main Menu" buttons work

**GATE: Do not proceed past this batch until 2 people (or 1 person playing both
sides) complete a full game with correct rules, no crashes, and no softlocks.
No amount of economy, cosmetics, or networking fixes a broken core game.**

---

## BATCH 8 — AI OPPONENT

**Status: FILE EXISTS BUT BROKEN. Fix existing file.**

**Existing files:**
- `Gameplay/AI/AIDifficulty.cs` — enum + noise params per tier (complete, do not touch)
- `Gameplay/AI/AIPlayer.cs` — broken: never takes its turn (FIX-04 in TODO_FIX.md)

**Fix summary (read FIX-04 in TODO_FIX.md for full code):**
1. `TurnManager.SwitchTurn()` must check if the new active player is AI and call `ai.TakeTurn()`
2. `AIPlayer.TakeTurn()` must start a coroutine that waits 0.6-1.4s, calculates a shot, fires, then calls `TurnManager.Instance.EndShot()`

**AI difficulty targets (verify against AIDifficulty.cs — adjust if values differ):**
- Easy: ±15° angle noise, ±30% power noise
- Medium: ±8° angle noise, ±15% power noise
- Hard: ±4° angle noise, ±8% power noise
- Expert: ±2° angle noise, ±4% power noise
- Master: ±1° angle noise, ±2% power noise

**Acceptance criteria:**
- [ ] AI takes its turn automatically after human's turn ends, with a visible delay
- [ ] AI never targets an already-pocketed coin
- [ ] Easy difficulty is beatable without effort; Master requires genuine skill
- [ ] No console errors during AI turn execution

---

## BATCH 9 — NAKAMA MULTIPLAYER SERVER

**Goal:** Realtime PvP works between two devices.

**Current state:** `Networking/NakamaService.cs` exists as a stub behind `#if NAKAMA_SDK`.

**Steps for agent:**
1. Install Docker Desktop (required for local Nakama server)
2. Run Nakama server locally using official Docker Compose:
   ```bash
   # Download from heroiclabs.com/docs/nakama/getting-started/docker-quickstart/
   docker-compose up
   ```
3. Import Nakama Unity SDK via Package Manager
   (add git URL: `https://github.com/heroiclabs/nakama-unity` or download .unitypackage)
4. Importing the SDK automatically activates `#if NAKAMA_SDK` blocks in
   `NakamaService.cs` — do NOT rewrite the file from scratch, the structure is there
5. Update server connection in `NakamaService.cs`:
   ```csharp
   const string host = "127.0.0.1"; // dev; change to prod host before release
   const int port = 7350;
   const string serverKey = "defaultkey";
   ```

**⚠️ DETERMINISM REQUIREMENT:**
Both clients must produce identical coin positions from identical shot input.
This ONLY works if:
- `CoinSpawner.cs` uses a fixed/deterministic starting formation (no `Random` calls
  for initial coin placement — verify this in CoinSpawner.cs before Batch 9)
- `Fixed Timestep` is `0.02` on both clients (set in Batch 6 — do not change)
- Physics settings (friction, mass, bounciness) are identical on both clients
  (they are, since they're baked into the project — do not change them per-device)

**Networking architecture (client-authoritative relay for v1):**
- Server relays shot input (direction + power) only — does NOT simulate physics
- Both clients simulate physics locally from the same input
- This is acceptable for a casual game at launch
- Full server-authoritative physics is a v2 hardening task

**Acceptance criteria:**
- [ ] Two devices/Editor instances find each other in a casual match
- [ ] A shot on Device A produces coin positions within 0.001 unit tolerance on Device B
- [ ] Disconnection during a match: disconnected player forfeits after 30s timeout
- [ ] No hang on disconnect — game resolves cleanly

---

## BATCH 10 — MATCHMAKING, FRIENDS, PRIVATE ROOMS

**Status: STUB FILES EXIST. Implement against Nakama API.**

**Existing files (implement, do not recreate):**
- `Networking/MatchmakingService.cs` — Casual/Ranked via Nakama matchmaker API
- `Networking/FriendsService.cs` — add/remove/invite via Nakama friends API
- `Networking/RoomService.cs` — 6-character room code, host/join via code

**Acceptance criteria:**
- [ ] Ranked queue matches players (test with 2+ simulated clients)
- [ ] Friend request → accept → both show as friends in FriendsService
- [ ] Private room code joins correctly, rejects invalid/expired codes

---

## BATCH 11 — PLAYER PROFILE & STATS

**Firestore schema addition:**
```
players/{playerId}/stats
  matchesPlayed: number
  wins: number
  losses: number
  draws: number
  bestStreak: number
  currentStreak: number
  highestRank: string
```

**Existing files (verify/implement):**
- `Data/PlayerStats.cs`
- `UI/ProfileScreen.cs`

**Acceptance criteria:**
- [ ] Stats update correctly after each match result
- [ ] Profile screen reflects live Firestore data, not cached/stale values

---

## BATCH 12 — ECONOMY (Currency + Rewards + Missions)

**Status: STUB FILES EXIST. Known bug in DailyRewardController.**

**Existing files (fix and implement):**
- `Gameplay/Economy/CurrencyService.cs`
- `Gameplay/Economy/DailyRewardController.cs` — **has a critical bug: FIX-10 in TODO_FIX.md**
- `Gameplay/Economy/MissionSystem.cs`

**⚠️ Bug in DailyRewardController.cs — fix before anything else in this batch:**
`Task.Delay(rewardCooldownHours * 1000)` delays 24,000ms = 24 seconds, not 24 hours.
Fix: `await Task.Delay(TimeSpan.FromHours(rewardCooldownHours));`
Full fix with PlayerPrefs-based claim guard is in FIX-10 of TODO_FIX.md.

**Firestore schema addition:**
```
players/{playerId}/economy
  coins: number
  diamonds: number
  tokens: number
  tickets: number
  lastDailyReward: timestamp
  missionProgress: map<missionId, progress>
```

**Acceptance criteria:**
- [ ] DailyReward cannot be claimed twice within 24 hours (test by manipulating PlayerPrefs)
- [ ] Currency changes write through to Firestore immediately (no client-only state)
- [ ] Missions track progress across app restarts
- [ ] Weekly missions reset on Monday only

---

## BATCH 13 — SHOP & COSMETICS

**Status: STUB FILES EXIST. Known shared-material bug.**

**Existing files (fix and implement):**
- `UI/ShopScreen.cs`
- `Gameplay/Cosmetics/CosmeticDatabase.cs`
- `Gameplay/Cosmetics/CosmeticEquipper.cs` — **has a bug: FIX-14 in TODO_FIX.md**

**⚠️ Bug in CosmeticEquipper.cs — fix before testing cosmetics:**
`renderer.sharedMaterial = x` modifies the shared asset and affects all objects using
that material. Fix: use `renderer.material = x` to create a per-instance copy.
Full fix is in FIX-14 of TODO_FIX.md.

**All materials must use Standard shader — NOT URP Lit** (see Batch 3 warning).

**Cosmetic sets:**
- Boards: Classic, Royal, Neon, Galaxy, Cyber, Fire, Ice, Temple, Heritage (9 total)
- Strikers: Wood, Gold, Fire, Dragon, Galaxy, Legendary, Animated (7 total)

**Acceptance criteria:**
- [ ] Purchasing deducts correct currency and permanently unlocks the item
- [ ] Equipping changes board/striker visuals in the next match
- [ ] Already-owned items show "Equip" not "Buy"
- [ ] Equipping one cosmetic does NOT change any other object's appearance

---

## BATCH 14 — RANKING & LEADERBOARDS

**Status: STUB FILES EXIST.**

**Existing files:**
- `Networking/LeaderboardService.cs` — returns hardcoded fake scores, must be replaced
- `Gameplay/Ranking/RankTierCalculator.cs` — complete, do not touch
- `UI/LeaderboardScreen.cs` — complete UI structure, needs real data source

**Use Nakama's built-in leaderboard API** — do NOT build a custom Firestore
leaderboard. Nakama's leaderboard is already optimized for ranked data.

**Tiers:** Bronze → Silver → Gold → Platinum → Diamond → Master → GrandMaster → Legend

**Acceptance criteria:**
- [ ] Rank updates after ranked matches (ELO-style: win = up, loss = down)
- [ ] Leaderboard shows real player data — no hardcoded fake scores
- [ ] Leaderboard is correctly sorted and paginated

---

## BATCH 15 — EVENTS & TOURNAMENTS

**Status: STUB FILES EXIST.**

**Existing files:**
- `Gameplay/Events/EventScheduler.cs`
- `Gameplay/Tournaments/TournamentManager.cs`

**Acceptance criteria:**
- [ ] Event activates/deactivates based on Remote Config time window
- [ ] Tournament bracket advances winners and eliminates losers correctly

---

## BATCH 16 — MONETIZATION (Ads, VIP, Battle Pass)

**Status: STUB FILES EXIST.**

**Existing files:**
- `Monetization/AdMobService.cs`
- `Monetization/VIPSubscription.cs`
- `Monetization/BattlePass.cs`

**Use test ad unit IDs first. Swap to real IDs only immediately before release.**

**Acceptance criteria:**
- [ ] All 5 ad formats display without crashing (Banner, Interstitial, Rewarded, App Open, Rewarded Interstitial)
- [ ] VIP purchase: receipt validated server-side, not just a local flag
- [ ] VIP no-ads status persists across app restarts
- [ ] Battle pass XP unlocks rewards at correct levels

---

## BATCH 17 — SOCIAL (Chat, Clans, Emotes)

**Status: STUB FILES EXIST.**

**Existing files:**
- `Social/ChatService.cs` — stores messages in memory only (lost on close); must use Nakama chat API
- `Social/ClanSystem.cs` — clan data in memory only; must use Nakama groups API
- `Social/EmoteSystem.cs`

**Acceptance criteria:**
- [ ] Messages persist and load from Nakama (not lost on app close)
- [ ] Clan creation/joining updates Nakama group membership
- [ ] Emotes appear on opponent screen within ~200ms

---

## BATCH 18 — VISUAL & AUDIO POLISH

**Status: FILES EXIST. Known memory leak in AudioManager.**

**Existing files:**
- `VFX/PocketCaptureEffect.cs`
- `VFX/VictoryEffect.cs`
- `Audio/AudioManager.cs` — **has a memory leak: FIX-08 in TODO_FIX.md**

**⚠️ Fix AudioManager before this batch:**
SFX AudioSources are never returned to the pool after playback — the pool grows
unboundedly. Fix is in FIX-08 of TODO_FIX.md.

**All audio materials, models, shaders: Standard pipeline only (no URP).**

**Acceptance criteria:**
- [ ] All sound events trigger correctly with no clipping
- [ ] Profiler shows AudioSource count stays flat during a session (pool not leaking)
- [ ] Particle effects run without frame drops on a mid-range Android device

---

## BATCH 19 — TESTING & LAUNCH PREP

**Steps for agent:**
1. Unity Play Mode tests for: foul detection, scoring, currency transactions, AI legality
2. Device test matrix: 1 low-end Android, 1 mid-range Android, 1 recent iOS device
3. Firebase Analytics events verified: session_start, match_complete, purchase, ad_impression
4. Store assets: icon, screenshots (per current store size requirements), preview video

**Acceptance criteria:**
- [ ] All automated tests pass
- [ ] 30-minute session on low-end Android: no crash, no freeze, no softlock
- [ ] 30-minute session on iOS: no crash, no freeze, no softlock
- [ ] Store listing assets meet current Play Store / App Store requirements
  (verify against current guidelines at submission time — these change regularly)

---

# AGENT EXECUTION RULES (apply to every batch)

1. **Before creating any file, check if it already exists.** Read it first.
   Only add what is missing. Do NOT overwrite existing working code.

2. **"No compile errors" is not done.** "No console errors" is not done.
   Every gameplay batch requires the acceptance criteria to pass in Play mode.

3. **One batch per agent session.** Don't let context bleed between batches.

4. **If a batch's acceptance criteria cannot be verified, stop and report it.**
   Do not mark a batch complete and move forward with an unverified system underneath.

5. **Physics values are starting points, not final.**
   Batch 6 exists to tune them. Do not change physics values in any other batch
   unless a specific fix in TODO_FIX.md calls for it.

6. **Standard shader only. No URP Lit.**
   Any new material must use the Standard shader with `_Glossiness` for smoothness.
   Do NOT use `_Smoothness` (that is URP). Do NOT switch the render pipeline.

7. **Fixed Timestep = 0.02 and must not change after Batch 6.**
   Multiplayer sync depends on this being identical on all clients.

8. **CoinSpawner must be deterministic.**
   No `UnityEngine.Random` calls for initial coin placement positions.
   Both clients must produce identical starting layouts for multiplayer sync to work.

9. **If a new console error appears after your edit that was not there before, revert.**
   Do not patch forward on top of a self-introduced error.
