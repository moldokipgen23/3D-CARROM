# CARROM GAMES 3D — AI AGENT BUILD SPEC

How to use this document: paste **one batch at a time** to your coding agent
(Claude Code, opencode, etc.). Do not paste the whole document. Each batch is
self-contained — it tells the agent what exists already, what to build, what
files to create, and how to know it's done. Move to the next batch only after
the current one passes its acceptance criteria.

---

## BATCH 0 — PROJECT FOUNDATION

**Goal:** Empty Unity project that builds to both platforms successfully.

**Prerequisites:** Unity 6 LTS or 2022 LTS installed, Android Build Support +
iOS Build Support modules installed.

**Steps for agent:**
1. Create new Unity project, **3D (URP) template**
2. Create folder structure:
   ```
   Assets/
     _Project/
       Scripts/
         Gameplay/
         UI/
         Networking/
         Data/
         Core/
       Art/
         Models/
         Materials/
         Textures/
       Prefabs/
       Scenes/
       Audio/
       Physics/
   ```
3. Create `.gitignore` for Unity (Library/, Temp/, Obj/, Logs/, UserSettings/,
   *.csproj, *.sln excluded; Assets/, ProjectSettings/, Packages/ included)
4. Create 4 empty scenes: `Boot`, `MainMenu`, `Game`, `Results`
5. Create `Assets/_Project/Scripts/Core/SceneFlow.cs` — a simple static class
   with scene name constants and a `LoadScene(string name)` wrapper
6. Set Android target: minSdkVersion 24, targetSdkVersion latest
7. Set iOS target: minimum iOS 13
8. Produce one empty build per platform to confirm pipeline works

**Acceptance criteria:**
- [ ] Project opens with no console errors
- [ ] All 4 scenes exist and are in Build Settings, in order
- [ ] Android build (.apk or .aab) completes without error
- [ ] iOS build (Xcode project export) completes without error
- [ ] Git repo initialized, first commit made

---

## BATCH 1 — CORE ARCHITECTURE

**Goal:** Clean code skeleton other batches plug into. No gameplay yet.

**Files to create:**
- `Core/GameManager.cs` — top-level MonoBehaviour singleton (DontDestroyOnLoad), holds current GameState enum: `{Boot, MainMenu, InGame, Results}`
- `Core/IService.cs` — empty marker interface for the service locator pattern
- `Core/ServiceLocator.cs` — static `Register<T>()`, `Get<T>()`, `Unregister<T>()`
- `Data/PlayerData.cs` — plain C# class: `string PlayerId, string Username, int Coins, int Diamonds, int XP, int Level`
- `Data/GameSettings.cs` — ScriptableObject: SFX volume, music volume, vibration toggle

**Acceptance criteria:**
- [ ] GameManager persists across scene loads (verify by loading MainMenu→Game and checking it's the same instance via a Debug.Log of GetInstanceID())
- [ ] ServiceLocator can register and retrieve a dummy service in a test script
- [ ] No MonoBehaviour uses `FindObjectOfType` for cross-system communication (all via ServiceLocator or events)

---

## BATCH 2 — FIREBASE SETUP

**Goal:** Working Firebase backend connected to the Unity project.

**Steps for agent:**
1. Import Firebase Unity SDK (Auth, Firestore, Analytics, Crashlytics, Remote Config modules)
2. Create `Networking/FirebaseService.cs` implementing `IService`:
   - `Task<bool> InitializeAsync()`
   - `Task<string> SignInAnonymouslyAsync()` (anonymous auth for first launch, upgradeable later)
   - `Task SavePlayerDataAsync(PlayerData data)` → writes to Firestore `players/{playerId}`
   - `Task<PlayerData> LoadPlayerDataAsync(string playerId)`
3. Register FirebaseService with ServiceLocator on Boot scene
4. Enable Crashlytics, set up a test non-fatal log to confirm it reports

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

---

## BATCH 3 — CARROM BOARD (3D)

**Goal:** A board exists in the `Game` scene with correct geometry and pocket detection.

**Specs:**
- Real carrom board: 29in × 29in playing surface → use **2.9 × 2.9 Unity
  units** (1 unit = 10in, keeps physics math sane)
- Board mesh: simple plane/cube with beveled border (~1.5in / 0.15 units border width)
- 4 pocket colliders: sphere triggers, radius ~0.05 units, positioned at each corner inset ~0.05 units from the edge
- Center circle: 0.16 unit diameter, positioned at board center, **texture/decal only, no collider**
- Baseline arrows: textured decals at each player's baseline, standard carrom layout (positioned per official carrom diagrams)

**Files to create:**
- `Gameplay/Board/PocketTrigger.cs` — `OnTriggerEnter` detects coin layer, fires `OnCoinPocketed(CoinType, GameObject coin)` event
- `Gameplay/Board/BoardController.cs` — holds references to all 4 pockets, exposes `event Action<CoinType> CoinPocketed`

**Materials:**
- URP Lit shader, wood/laminate base color texture (CC0 source, e.g. ambientCG "Wood" or "Laminate" pack), normal map for subtle grain, smoothness ~0.4-0.5 (not mirror-flat, real boards have slight sheen not a hard reflection)

**Acceptance criteria:**
- [ ] Board visible in Game scene with correct proportions (verify against reference photo)
- [ ] Dropping a test sphere into each pocket trigger fires the OnCoinPocketed event (test via Debug.Log)
- [ ] Board has no console errors, lighting looks reasonable in URP (not pitch black, not blown out)

---

## BATCH 4 — COINS

**Goal:** Physically simulated coins with correct relative properties.

**Specs (starting values — will need live tuning in Batch 6):**
- Coin: cylinder mesh, diameter ~0.032 units (real coin ≈ 1.3in), height ~0.005 units
- White/black coins: same mass (e.g. Rigidbody mass = 5), 9 of each
- Queen: same diameter, slightly distinct color (red), mass = 5
- Physics Material per coin: Dynamic Friction 0.3, Static Friction 0.3, Bounciness 0.1, Friction Combine = Minimum, Bounce Combine = Minimum
- Rigidbody: `useGravity = true` but constrained — lock Y-position via `Rigidbody.constraints` so coins stay on the board plane while still allowing X/Z physics response (do NOT disable gravity entirely, you need it pressing coins onto the board for friction to work correctly)

**Files to create:**
- `Gameplay/Coins/CoinType.cs` — enum `{White, Black, Queen}`
- `Gameplay/Coins/Coin.cs` — MonoBehaviour: `CoinType Type`, `bool IsPocketed`, subscribes to its board's `CoinPocketed` event filtered by self
- `Gameplay/Coins/CoinSpawner.cs` — places all 19 coins in official starting formation (concentric circles around center, queen in middle) at game start

**Material:**
- URP Lit, white/black solid color + queen red, smoothness ~0.6 for a slight specular highlight (this sells the 3D look)

**Acceptance criteria:**
- [ ] 19 coins spawn in correct official starting position
- [ ] Coins rest stably without jittering or sliding on their own (no physics instability)
- [ ] A test force applied to one coin causes realistic-looking collision propagation to neighbors

---

## BATCH 5 — STRIKER + SHOOTING MECHANIC

**Goal:** Player can aim and shoot the striker via touch/mouse drag.

**Specs:**
- Striker: larger cylinder, diameter ~0.041 units, mass = 8 (heavier than coins)
- Same Physics Material approach as coins but separate tuned values (striker needs slightly higher friction so it doesn't slide forever after a light tap)
- Placement: constrained to player's baseline area only (clamp X position to baseline width during aim phase)

**Input/Aim system:**
- `Gameplay/Striker/StrikerController.cs`:
  - On touch/mouse down on striker: enter Aim state
  - Drag direction = aim direction (striker visually shows a direction indicator, e.g. a thin line/arrow renderer)
  - Drag distance = power, clamped to `maxDragDistance`, mapped to `minForce`–`maxForce` (start: minForce = 2, maxForce = 15 — needs live tuning)
  - On release: `Rigidbody.AddForce(direction * power, ForceMode.Impulse)`, exit Aim state, enter Shot state (locked until all coins stop moving)
- Camera: tilted top-down, ~55° from vertical, positioned behind the active player's baseline, smoothly transitions side when turn changes

**Files to create:**
- `Gameplay/Striker/StrikerController.cs`
- `Gameplay/Striker/AimIndicator.cs` (LineRenderer-based direction/power visual)
- `Gameplay/Camera/CarromCameraController.cs`

**Acceptance criteria:**
- [ ] Dragging the striker shows a visible aim line that follows touch/mouse
- [ ] Releasing applies force proportional to drag distance, in the correct direction
- [ ] Striker cannot be placed/shot outside the legal baseline zone
- [ ] Camera angle matches reference (tilted, not flat top-down, not isometric)
- [ ] **This batch needs a manual feel-testing pass — do not consider it done from code review alone. Play it.**

---

## BATCH 6 — PHYSICS TUNING PASS

**Goal:** Make it feel like real carrom. This is QA, not new features.

**Process for agent (iterative):**
1. Play 10 test shots at varying power
2. Check: do coins glide and decelerate naturally, or stop too abruptly / slide forever?
3. Check: does a direct hit on a clustered coin transfer force realistically (not coins flying off at absurd speed, not coins barely moving)?
4. Adjust `Dynamic Friction` and `Bounciness` incrementally (±0.05 steps)
5. Adjust striker mass/force range if shots feel too weak/strong across the whole power slider range
6. Verify pocket capture: a coin moving fast over a pocket should still fall in if any part of its collider overlaps the trigger radius, not require pixel-perfect centering — but pockets shouldn't be a magnet either

**Acceptance criteria:**
- [ ] A "soft tap" shot moves the striker a few inches and stops — not instant stop, not a long awkward slide
- [ ] A "full power" shot can break a clustered formation realistically
- [ ] At least 3 manual test sessions logged with adjustments made each time (this prevents an agent from marking this "done" after one pass with no real tuning)

---

## BATCH 7 — SINGLE PLAYER GAME LOOP

**Goal:** A complete, playable Classic mode game, 2 players (no AI yet — hotseat/local).

**Files to create:**
- `Gameplay/Rules/TurnManager.cs` — tracks active player, switches turn after a shot resolves (waits for all Rigidbodies to reach near-zero velocity)
- `Gameplay/Rules/FoulDetector.cs` — detects: striker pocketed (foul), no coin touched/pocketed in a turn (foul), queen pocketed without a "cover" (carrom queen-cover rule)
- `Gameplay/Rules/ScoreManager.cs` — tracks pocketed coins per player, applies foul penalties (standard carrom: -1 coin returned to board on foul)
- `UI/GameHUD.cs` — shows current player, score, remaining coins
- `UI/ResultsScreen.cs` — win/lose/draw display, "Play Again" / "Main Menu" buttons

**Acceptance criteria:**
- [ ] Two players can play a full game locally start to finish without crashes
- [ ] Fouls are correctly detected and penalized per standard carrom rules
- [ ] Game correctly identifies a winner when one player pockets all their coins + queen (with cover rule)
- [ ] Results screen appears and both buttons work

**GATE: Do not proceed past this batch until Batches 3-7 together are
genuinely fun to play, with no AI, just two people taking turns. If it's not
fun here, no amount of economy/cosmetics in later batches will fix it.**

---

## BATCH 8 — AI OPPONENT

**Goal:** Single-player vs AI, 5 difficulty tiers.

**Approach:** Do NOT build a full physics-search AI. Use the "noisy aim" method:
- AI picks a target coin (nearest pocketable coin = simplest heuristic)
- Calculates ideal striker aim angle + power to send that coin toward the nearest pocket
- Applies random noise to angle and power, noise magnitude scaled by difficulty:
  - Easy: ±15° angle noise, ±30% power noise
  - Medium: ±8° angle noise, ±15% power noise
  - Hard: ±4° angle noise, ±8% power noise
  - Expert: ±2° angle noise, ±4% power noise, picks best of 2 candidate target coins
  - Master: ±1° angle noise, ±2% power noise, picks best of 3 candidate target coins, accounts for striker's own follow-through position for next turn (basic 1-shot lookahead)

**Files to create:**
- `Gameplay/AI/AIDifficulty.cs` — enum + noise parameter struct per tier
- `Gameplay/AI/AIPlayer.cs` — implements same input interface as StrikerController so it can "shoot" through the same code path as a human (no special-cased physics)

**Acceptance criteria:**
- [ ] AI takes its turn automatically with a believable "thinking" delay (0.5-1.5s)
- [ ] Easy is genuinely beatable by a beginner, Master is genuinely hard to beat
- [ ] AI never targets an illegal/already-pocketed coin

---

## BATCH 9 — NAKAMA MULTIPLAYER SERVER

**Goal:** Realtime PvP works between two devices.

**Steps for agent:**
1. Docker Compose setup for Nakama + its Postgres backend (local/dev first, production deployment is a separate later task)
2. Import Nakama Unity SDK
3. `Networking/NakamaService.cs` implementing `IService`:
   - `Task ConnectAsync(string firebaseToken)` — bridges Firebase auth to Nakama custom auth
   - `Task<IMatch> FindMatchAsync(MatchType type)` — Casual/Ranked/Private
   - `Task SendShotAsync(Vector2 direction, float power)` — sends local player's shot to opponent
   - `event Action<Vector2, float> OpponentShotReceived`

**Networking architecture decision (must be made explicit, not left ambiguous):**
- **Client-authoritative with server relay** for v1 (simpler, acceptable for a casual game — server just relays shot input, both clients simulate physics locally and trust the result). Full server-authoritative physics validation is a v2 hardening task, not required for launch.
- Both clients run identical Unity physics with identical starting coin positions (seeded/fixed), so shot input alone (direction + power) is enough to keep both clients in sync — **as long as Fixed Timestep and physics settings are identical on both clients.** Verify this explicitly.

**Acceptance criteria:**
- [ ] Two devices (or two Editor instances) can find each other in a casual match
- [ ] A shot taken on Device A produces the same resulting coin positions on Device B (verify by comparing final coin positions within small tolerance)
- [ ] Disconnection during a match is handled gracefully (opponent forfeit after timeout, not a hang)

---

## BATCH 10 — MATCHMAKING, FRIENDS, PRIVATE ROOMS

**Files to create:**
- `Networking/MatchmakingService.cs` — Casual/Ranked queue via Nakama matchmaker API
- `Networking/FriendsService.cs` — add/remove/invite via Nakama's built-in friends API
- `Networking/RoomService.cs` — generates a 6-character room code, hosts join via code

**Acceptance criteria:**
- [ ] Ranked queue matches players (test with 2+ simulated clients)
- [ ] Friend request → accept → both show as friends
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

**Files to create:**
- `Data/PlayerStats.cs`
- `UI/ProfileScreen.cs` — displays username, avatar, level, XP bar, stats grid
- `Gameplay/Avatars/AvatarSystem.cs` — loads default avatar set (start with ~12 free avatars, premium/event avatars gated behind currency, structure built now even if only free ones populated initially)

**Acceptance criteria:**
- [ ] Stats update correctly after each match (win/loss/draw all tested)
- [ ] Profile screen reflects live Firestore data, not cached/stale

---

## BATCH 12 — ECONOMY (Currency + Rewards + Missions)

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

**Files to create:**
- `Gameplay/Economy/CurrencyService.cs` — single source of truth for add/spend, always writes through to Firestore (no client-only currency state that can desync)
- `Gameplay/Economy/DailyRewardController.cs` — 7-day cycle, escalating rewards
- `Gameplay/Economy/MissionSystem.cs` — Daily (resets 24h), Weekly (resets 7d), Season (resets per season config from Remote Config)
- `Gameplay/Economy/LuckyWheel.cs` — weighted random reward table, configurable via Remote Config (so odds can be tuned live without app update)

**Acceptance criteria:**
- [ ] Currency changes are atomic and reflected immediately in UI
- [ ] Daily reward correctly blocks re-claiming within 24h, correctly resets streak if a day is missed
- [ ] Missions track progress correctly across a full session and persist across app restarts

---

## BATCH 13 — SHOP & COSMETICS

**Files to create:**
- `UI/ShopScreen.cs` — tabbed categories (Coins/Diamonds/Boards/Strikers/Bundles/VIP)
- `Gameplay/Cosmetics/CosmeticDatabase.cs` — ScriptableObject list of all board/striker skins with: id, display name, price, currency type, unlock requirement (purchase/event/level)
- `Gameplay/Cosmetics/CosmeticEquipper.cs` — swaps board/striker material+mesh at runtime based on equipped cosmetic id

**Cosmetic sets to populate (matches original scope):**
- Boards: Classic, Royal, Neon, Galaxy, Cyber, Fire, Ice, Temple, Heritage (9 total)
- Strikers: Wood, Gold, Fire, Dragon, Galaxy, Legendary, Animated (7 total)

**Acceptance criteria:**
- [ ] Purchasing a cosmetic deducts correct currency and unlocks it permanently for that player
- [ ] Equipping a cosmetic visually changes the board/striker in the next match
- [ ] Already-owned items show "Equip" not "Buy"

---

## BATCH 14 — RANKING & LEADERBOARDS

**Files to create:**
- `Networking/LeaderboardService.cs` — uses Nakama's built-in leaderboard API (don't build a custom Firestore leaderboard, Nakama already has this and it's better suited for ranked data)
- `Gameplay/Ranking/RankTierCalculator.cs` — maps rating number to tier: Bronze/Silver/Gold/Platinum/Diamond/Master/GrandMaster/Legend
- `UI/LeaderboardScreen.cs` — tabs for Global/Country/Friends/Season

**Acceptance criteria:**
- [ ] Rank correctly updates after ranked matches (win = rating up, loss = rating down, with standard ELO-style adjustment)
- [ ] Leaderboard displays correctly sorted, paginated for large player counts

---

## BATCH 15 — EVENTS & TOURNAMENTS

**Files to create:**
- `Gameplay/Events/EventScheduler.cs` — reads active events from Remote Config (start/end timestamps, event type, reward table)
- `Gameplay/Tournaments/TournamentManager.cs` — bracket generation, uses Nakama tournament API

**Acceptance criteria:**
- [ ] An event correctly activates/deactivates based on its configured time window
- [ ] A tournament bracket correctly advances winners and eliminates losers

---

## BATCH 16 — MONETIZATION (Ads, VIP, Battle Pass)

**Files to create:**
- `Monetization/AdMobService.cs` — Banner, Interstitial, Rewarded, App Open, Rewarded Interstitial (Unity AdMob SDK, test ad unit IDs first, real IDs swapped in before release)
- `Monetization/VIPSubscription.cs` — IAP integration (Unity IAP), grants no-ads flag + reward multiplier
- `Monetization/BattlePass.cs` — Free + Premium track, 100 levels, XP-gated unlocks

**Acceptance criteria:**
- [ ] Test ads display correctly in all 5 formats without crashing
- [ ] VIP purchase correctly removes ads and is detected on app restart (receipt validation, not just a local flag)
- [ ] Battle pass XP correctly unlocks track rewards at the right levels

---

## BATCH 17 — SOCIAL (Chat, Clans, Emotes)

**Files to create:**
- `Social/ChatService.cs` — uses Nakama's chat/channel API for Global/Friends/Clan/Match channels
- `Social/ClanSystem.cs` — create/join clan, clan-level Nakama group
- `Social/EmoteSystem.cs` — quick reaction triggers during a match, synced via Nakama match state

**Acceptance criteria:**
- [ ] Messages send and receive in real time in each channel type
- [ ] Clan creation/joining correctly updates Nakama group membership
- [ ] Emotes display on opponent's screen within ~200ms of being triggered

---

## BATCH 18 — VISUAL & AUDIO POLISH

**Files to create:**
- `VFX/PocketCaptureEffect.cs` — particle burst on coin pocketed
- `VFX/VictoryEffect.cs`
- `Audio/AudioManager.cs` — board impact, coin collision, pocket sound, button click, victory theme, all pooled (no GC allocation per sound)

**Acceptance criteria:**
- [ ] All listed sound events trigger correctly with no audio clipping/overlap issues
- [ ] Particle effects run on mobile without frame drops (test on a mid-range device, not just Editor)

---

## BATCH 19 — TESTING & LAUNCH PREP

**Steps for agent:**
1. Unity Play Mode tests for: foul detection, scoring, currency transactions, AI shot legality
2. Device test matrix: minimum 1 low-end Android, 1 mid-range Android, 1 recent iOS device
3. Firebase Analytics events verified: session_start, match_complete, purchase, ad_impression
4. Store assets: icon, screenshots (per-device-size requirements for Play Store/App Store), preview video script

**Acceptance criteria:**
- [ ] All automated tests pass
- [ ] No crashes across the device test matrix over a 30-minute session
- [ ] Store listing assets meet current Play Store/App Store size/format requirements (verify against current store guidelines at submission time, these change)

---

# AGENT EXECUTION RULES (apply to every batch)

1. **One batch per agent session.** Don't let context bleed between batches — start fresh, reference only "Batch N is built, here's Batch N+1" plus any specific files the new batch touches.
2. **No batch is "done" from code compiling alone.** Gameplay batches (3-9) require actual play-testing, not just no console errors.
3. **If a batch's acceptance criteria can't be verified, stop and flag it** — don't let the agent mark it complete and move on with an unverified system underneath.
4. **Physics values throughout this doc are starting points, not final.** Batch 6 exists specifically to tune them — expect to revisit friction/mass/force values even after Batch 6 as later batches (cosmetics changing coin meshes, etc.) are added.