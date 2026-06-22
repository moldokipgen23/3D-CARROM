# 3D CARROM — FIX PLAN FOR AI AGENTS
# (DeepSeek / Qwen / Claude / GPT-4 / Gemini compatible)

## HOW TO USE THIS DOCUMENT

This project has ~88 C# Unity scripts already written but the game is NOT playable.
This file tells you exactly what is broken, what file to open, what lines to change,
and how to verify the fix. Work through fixes in the ORDER listed. Do not skip ahead.
Each section has an "ACCEPTANCE CHECK" — do not mark a fix done until you verify it.

**Before touching anything:**
1. Read the file mentioned before editing it.
2. Make the smallest change that fixes the problem — do not refactor.
3. After every fix run the Unity Editor and check the Console for new errors.

**Project root:** `D:\MOLDO GAMING\3D CARROM`  
**Scripts root:** `Assets\_Project\Scripts`  
**Scenes:** Boot, MainMenu, Game, Results (all in `Assets\_Project\Scenes`)

---

## QUICK STATUS TABLE

| System | Status |
|---|---|
| Board generation (BoardSetup.cs) | WORKING |
| Coin spawning (CoinSpawner.cs) | WORKING |
| Striker aim/drag (StrikerController.cs) | MOSTLY WORKING |
| Camera follow | WORKING |
| Audio pooling (AudioManager.cs) | MOSTLY WORKING |
| Coin pocketing → score update | BROKEN (Tier 1) |
| Turn switching | BROKEN (Tier 1) |
| Queen cover rule | BROKEN (Tier 1) |
| AI takes its turn | BROKEN (Tier 1) |
| Win condition triggers results screen | BROKEN (Tier 2) |
| Main menu (has buttons, lets player start) | BROKEN (Tier 2) |
| Results screen (waits for player input) | BROKEN (Tier 2) |
| Firebase / Nakama / Leaderboard | STUBS — no real data |

---

# TIER 1 — GAME CANNOT RUN AT ALL

Fix every item in this tier before touching anything else.
After all 4 are fixed you should be able to play a full game.

---

## FIX-01 — Coins are never scored when they enter a pocket

**Root cause:**
`PocketTrigger.cs` detects the coin entering its trigger collider but never calls
`coin.Pocket()` or tells `ScoreManager` a coin was scored.

**File:** `Assets\_Project\Scripts\Gameplay\Board\PocketTrigger.cs`

**What to read first:** Open PocketTrigger.cs and read the full file.

**What to change:**

Replace the entire `OnTriggerEnter` method body with the following logic:

```csharp
private void OnTriggerEnter(Collider other)
{
    // Only process objects on the Coin layer
    if (other.gameObject.layer != LayerMask.NameToLayer("Coin")) return;

    Coin coin = other.GetComponent<Coin>();
    if (coin == null || coin.IsPocketed) return;

    // Mark the coin as pocketed (stops it from being detected again)
    coin.Pocket();

    // Notify ScoreManager
    ScoreManager sm = ScoreManager.Instance;
    if (sm != null)
        sm.AddCoin(coin.Type);   // NOTE: method name may be AddCoins — check ScoreManager.cs line 50 and match exactly
}
```

**Also check:** `Assets\_Project\Scripts\Gameplay\Coins\Coin.cs`
- Find the `Pocket()` method (around line 71).
- Make sure it sets `IsPocketed = true` AND disables the Rigidbody/collider so the coin
  stops moving and stops triggering other pockets.
- If `Pocket()` is missing or does not set `IsPocketed = true`, add/fix it:

```csharp
public void Pocket()
{
    if (IsPocketed) return;
    IsPocketed = true;
    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
    }
    gameObject.SetActive(false); // hide coin from the board
}
```

**ACCEPTANCE CHECK:**
- In the Unity Editor, enter Play mode.
- Manually drag a coin (in the Scene window) into a pocket trigger.
- The Console should show no null-reference errors.
- ScoreManager's score for the active player should increment by 1.

---

## FIX-02 — Turn never switches after a shot

**Root cause:**
`TurnManager.cs` has `EndShot()` but it switches turn immediately, before coins finish
moving. This causes the next shot to start while coins are still sliding — leading to
mis-fires, double-turns, or softlocks.

**File:** `Assets\_Project\Scripts\Gameplay\Rules\TurnManager.cs`

**What to read first:** Open TurnManager.cs and read the full file.

**What to change:**

Add a coroutine that waits for all Rigidbodies to stop before switching turn.
Find the `EndShot()` method and replace its body with:

```csharp
public void EndShot()
{
    StartCoroutine(WaitForPhysicsThenSwitch());
}

private IEnumerator WaitForPhysicsThenSwitch()
{
    // Wait at least one frame so forces are applied
    yield return new WaitForFixedUpdate();

    // Then wait until every coin and the striker are below velocity threshold
    const float sleepThreshold = 0.02f;
    const float maxWait = 8f; // safety — never wait more than 8 seconds
    float waited = 0f;

    while (waited < maxWait)
    {
        bool allSleeping = true;
        // Check striker
        Rigidbody strikerRb = StrikerController.Instance?.GetComponent<Rigidbody>();
        if (strikerRb != null && strikerRb.linearVelocity.magnitude > sleepThreshold)
            allSleeping = false;

        // Check coins
        Coin[] coins = FindObjectsOfType<Coin>();
        foreach (Coin c in coins)
        {
            if (c.IsPocketed) continue;
            Rigidbody rb = c.GetComponent<Rigidbody>();
            if (rb != null && rb.linearVelocity.magnitude > sleepThreshold)
            {
                allSleeping = false;
                break;
            }
        }

        if (allSleeping) break;
        waited += Time.fixedDeltaTime;
        yield return new WaitForFixedUpdate();
    }

    // Now safe to switch turn
    SwitchTurn();
}
```

**Also verify:** `SwitchTurn()` must reset the striker position to the new active
player's baseline, re-enable striker input, and update `GameHUD`.

**ACCEPTANCE CHECK:**
- Shoot the striker. Observe that the turn indicator in GameHUD does NOT change
  until all coins on the board have visibly stopped moving.
- Shoot toward an empty area — turn should switch in ~1-2 seconds.
- Shoot into a cluster — turn should switch only after all scattered coins stop.

---

## FIX-03 — ScoreManager queen cover rule is inverted; score enum mismatch

**Root cause A — enum mismatch:**
`ScoreManager.AddCoins()` receives a raw `int` (0/1/2) in some call sites but
`CoinType` enum in others. This causes silent mismatches.

**Root cause B — queen cover logic wrong:**
`HasQueenCover()` returns `true` if the queen coin object still exists (is not pocketed).
It should return `true` if the CURRENT player has pocketed at least one of their coins
AFTER the queen was pocketed in the SAME turn (the "cover" rule).

**File:** `Assets\_Project\Scripts\Gameplay\Rules\ScoreManager.cs`

**What to read first:** Read ScoreManager.cs in full. Note the exact method signatures.

**Fix A — standardize to CoinType enum:**

Find `AddCoins(int coinTypeInt)` or `AddCoin(int ...)` — rename/change the parameter
to `CoinType type` everywhere. Update `PocketTrigger.cs` (fixed in FIX-01) to pass
`coin.Type` directly. Remove any `switch (coinTypeInt)` blocks; replace with:

```csharp
public void AddCoin(CoinType type)
{
    int playerIndex = TurnManager.Instance.ActivePlayerIndex;

    switch (type)
    {
        case CoinType.White:
            // White coins count for Player 0 (first player)
            _scores[0]++;
            break;
        case CoinType.Black:
            // Black coins count for Player 1 (second player)
            _scores[1]++;
            break;
        case CoinType.Queen:
            // Queen is awarded to whoever pocketed it, pending cover
            _queenPocketedByPlayer = playerIndex;
            _queenPocketed = true;
            _queenCovered = false; // must be covered this turn or returned
            break;
    }

    OnScoreUpdated?.Invoke(_scores[0], _scores[1]);
    CheckWinCondition();
}
```

**Fix B — proper queen cover tracking:**

Add these fields to the class (if not already present):
```csharp
private bool _queenPocketed = false;
private bool _queenCovered = false;
private int _queenPocketedByPlayer = -1;
```

Add a method called by TurnManager at end of turn AFTER all physics settles:
```csharp
public void OnTurnEnd()
{
    if (_queenPocketed && !_queenCovered)
    {
        // Queen not covered — return queen to board center
        _queenPocketed = false;
        _queenPocketedByPlayer = -1;
        ReturnQueenToBoard();
    }
}

// Call this when any non-queen coin is scored by the same player who pocketed the queen in the same turn
public void RegisterCoverAttempt(int playerIndex)
{
    if (_queenPocketed && !_queenCovered && playerIndex == _queenPocketedByPlayer)
        _queenCovered = true;
}
```

In `TurnManager.WaitForPhysicsThenSwitch()` (from FIX-02), before calling
`SwitchTurn()`, call:
```csharp
ScoreManager.Instance?.OnTurnEnd();
```

**Fix C — remove duplicate event declaration:**
Search ScoreManager.cs for `OnScoreUpdated` — if it appears twice as a field/event
declaration, delete the duplicate. Keep only one declaration.

**ACCEPTANCE CHECK:**
- Pocket a white coin → Player 1 score increments.
- Pocket a black coin → Player 2 score increments.
- Pocket the queen then pocket a white coin in the same turn → queen is awarded (no return to board).
- Pocket the queen then fail to pocket any coin in the same turn → queen reappears at center.

---

## FIX-04 — AI opponent never takes its turn

**Root cause:**
`AIPlayer.cs` has a callback `OnShotFired` that it expects to be called when it's the
AI's turn, but TurnManager never calls it. Additionally, after the AI shoots, it does
not invoke the shot-fired event so the turn never transitions.

**Files:**
- `Assets\_Project\Scripts\Gameplay\AI\AIPlayer.cs`
- `Assets\_Project\Scripts\Gameplay\Rules\TurnManager.cs`

**Fix in TurnManager:**

In `SwitchTurn()`, after determining the new active player, check if that player is
controlled by AI and notify the AI:

```csharp
private void SwitchTurn()
{
    ActivePlayerIndex = (ActivePlayerIndex + 1) % PlayerCount;
    OnTurnChanged?.Invoke(ActivePlayerIndex);

    // If it is the AI's turn, trigger it
    AIPlayer ai = FindObjectOfType<AIPlayer>();
    if (ai != null && ActivePlayerIndex == ai.PlayerIndex)
        ai.TakeTurn();
}
```

**Fix in AIPlayer:**

Find `TakeTurn()` (or `ExecuteShot()` / whatever method triggers the AI shot).
After `ApplyForceToStriker(...)` is called, the AI must notify TurnManager that a shot
was fired — the same way a human shot is notified:

```csharp
private IEnumerator ExecuteShotCoroutine()
{
    // Wait "thinking" delay
    yield return new WaitForSeconds(Random.Range(0.6f, 1.4f));

    Vector2 shotDir = CalculateBestShot();
    float power = CalculatePower();
    ApplyForceToStriker(shotDir, power);

    // Tell TurnManager the shot has been taken (same as human would)
    TurnManager.Instance?.EndShot();
}
```

Make sure `TakeTurn()` starts this coroutine:
```csharp
public void TakeTurn()
{
    StartCoroutine(ExecuteShotCoroutine());
}
```

**Also check:** `AIPlayer.PlayerIndex` must be set to `1` (second player) in the
Inspector or in `Start()`. TurnManager must know `PlayerCount = 2`.

**ACCEPTANCE CHECK:**
- Start the Game scene. After the human takes a shot and all coins stop, the AI should
  automatically aim and shoot within 1-2 seconds.
- The Console should show no null-reference errors during the AI turn.
- After the AI shoots, the turn should return to the human player.

---

# TIER 2 — GAME IS PLAYABLE BUT LOOP IS BROKEN

Fix these after all Tier 1 items pass.

---

## FIX-05 — Main menu has no buttons; auto-loads game after 1 second

**File:** `Assets\_Project\Scripts\UI\MainMenuManager.cs`

**What to change:**
Remove the auto-load timer. The Main Menu should wait for the player to press a
"Play" button. If the MainMenu scene has no UI Canvas with a Play button, create one.

```csharp
// In the Unity Editor:
// 1. Open MainMenu.unity scene
// 2. Add a UI Canvas (if none exists)
// 3. Add a Button named "PlayButton" with text "PLAY"
// 4. In MainMenuManager.cs:

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button _playButton;

    private void Start()
    {
        _playButton.onClick.AddListener(OnPlayClicked);
    }

    private void OnPlayClicked()
    {
        SceneFlow.LoadScene("Game");
    }
}
```

Assign the button in the Inspector after adding it to the scene.

**ACCEPTANCE CHECK:**
- App starts at Boot scene, loads Main Menu.
- No auto-transition occurs. The screen waits.
- Pressing/clicking "PLAY" loads the Game scene.

---

## FIX-06 — Results screen auto-dismisses after 2 seconds; win never triggers it

**File A:** `Assets\_Project\Scripts\UI\ResultsSceneManager.cs`  
**File B:** `Assets\_Project\Scripts\UI\ResultsScreen.cs`  
**File C:** `Assets\_Project\Scripts\UI\GameSceneManager.cs` (wires win → results)

**Fix A — stop auto-dismiss:**
In `ResultsSceneManager.cs`, remove or disable the auto-return coroutine.
The screen should stay visible until the player presses a button.

**Fix B — wire win condition to show results:**
In `ScoreManager.CheckWinCondition()` (or wherever the winner is determined),
after identifying the winner, show the ResultsScreen:

```csharp
private void CheckWinCondition()
{
    // Standard carrom win: pocket all your coins and cover the queen
    // Adjust thresholds to match your coin counts (9 white / 9 black)
    if (_scores[0] >= 9 && _queenCovered)
    {
        ShowResults(winnerIndex: 0);
    }
    else if (_scores[1] >= 9 && _queenCovered)
    {
        ShowResults(winnerIndex: 1);
    }
}

private void ShowResults(int winnerIndex)
{
    ResultsScreen rs = FindObjectOfType<ResultsScreen>(includeInactive: true);
    if (rs != null)
    {
        rs.gameObject.SetActive(true);
        rs.ShowResult(winnerIndex);
    }
}
```

**Fix C — ResultsScreen.ShowResult must display winner and wire buttons:**
In `ResultsScreen.cs`, implement `ShowResult(int winnerIndex)`:

```csharp
public void ShowResult(int winnerIndex)
{
    _winnerLabel.text = winnerIndex == 0 ? "Player 1 Wins!" : "Player 2 Wins!";
    _playAgainButton.onClick.RemoveAllListeners();
    _playAgainButton.onClick.AddListener(OnPlayAgain);
    _mainMenuButton.onClick.RemoveAllListeners();
    _mainMenuButton.onClick.AddListener(OnMainMenu);
}

private void OnPlayAgain()
{
    SceneFlow.LoadScene("Game");
}

private void OnMainMenu()
{
    SceneFlow.LoadScene("MainMenu");
}
```

Make sure `_winnerLabel`, `_playAgainButton`, `_mainMenuButton` are assigned in the
Inspector (they may already exist as serialized fields — just wire them up).

**ACCEPTANCE CHECK:**
- Play a game and pocket all required coins + queen with cover.
- Results screen appears with the correct winner name.
- "Play Again" restarts the game.
- "Main Menu" returns to the main menu.
- The screen does NOT auto-dismiss.

---

## FIX-07 — GameHUD updates every frame (performance + stale display)

**File:** `Assets\_Project\Scripts\UI\GameHUD.cs`

**What to change:**
Remove score/turn updates from `Update()`. Subscribe to events instead.

In `Start()`:
```csharp
private void Start()
{
    ScoreManager.Instance.OnScoreUpdated += RefreshScores;
    TurnManager.Instance.OnTurnChanged += RefreshTurn;
    RefreshScores(0, 0);   // initialize display
    RefreshTurn(0);
}

private void OnDestroy()
{
    if (ScoreManager.Instance != null)
        ScoreManager.Instance.OnScoreUpdated -= RefreshScores;
    if (TurnManager.Instance != null)
        TurnManager.Instance.OnTurnChanged -= RefreshTurn;
}

private void RefreshScores(int p1Score, int p2Score)
{
    _player1ScoreLabel.text = p1Score.ToString();
    _player2ScoreLabel.text = p2Score.ToString();
}

private void RefreshTurn(int activePlayer)
{
    _turnLabel.text = activePlayer == 0 ? "Player 1" : "Player 2";
}
```

Delete or comment out any score/turn reading inside `Update()`.

**ACCEPTANCE CHECK:**
- HUD reflects current score after each coin is pocketed (not delayed, not stale).
- No `Update()` poll remains for score or turn display.

---

## FIX-08 — AudioManager SFX pool leaks AudioSources

**File:** `Assets\_Project\Scripts\Audio\AudioManager.cs`

**Root cause:**
New AudioSources are created when the pool is exhausted but `ReturnSource()` is never
called after playback finishes, so the pool grows unboundedly.

**Fix:**
Replace the "play and forget" pattern with a coroutine that returns the source after
the clip finishes:

```csharp
public void PlaySFX(AudioClip clip, float volume = 1f)
{
    AudioSource src = GetSourceFromPool();
    src.clip = clip;
    src.volume = volume;
    src.Play();
    StartCoroutine(ReturnAfterPlay(src, clip.length));
}

private IEnumerator ReturnAfterPlay(AudioSource src, float delay)
{
    yield return new WaitForSeconds(delay);
    src.Stop();
    src.clip = null;
    ReturnSource(src);
}
```

**ACCEPTANCE CHECK:**
- After 30 seconds of gameplay with many coin collisions, open the Unity Profiler.
- The number of AudioSource components on the AudioManager GameObject should not
  grow past the initial pool size (typically 8-16).

---

## FIX-09 — Foul detection: RegisterShot is never called

**File:** `Assets\_Project\Scripts\Gameplay\Rules\FoulDetector.cs`

**Root cause:**
`RegisterShot()` tracks the state needed to detect fouls (did any coin move? was
striker pocketed?) but nothing calls it when a shot is taken.

**Fix:**
In `StrikerController.cs`, find where force is applied to the striker (the `OnRelease`
or equivalent method). After `AddForce(...)`, call:

```csharp
FoulDetector.Instance?.RegisterShot();
```

Also make sure `FoulDetector.OnTriggerEnter` (for striker-in-pocket detection) is on
the striker's collider, not on the pocket trigger. If the striker uses the same
`PocketTrigger` as coins, add a check:

```csharp
// In PocketTrigger.OnTriggerEnter, BEFORE the Coin check:
if (other.CompareTag("Striker"))
{
    FoulDetector.Instance?.RegisterStrikerPocketed();
    // Also reset striker position:
    StrikerController.Instance?.ResetToBaseline();
    TurnManager.Instance?.EndShot();
    return;
}
```

**ACCEPTANCE CHECK:**
- Shoot the striker directly into a pocket. The game should:
  1. Detect a foul.
  2. Deduct a point OR return the last pocketed coin to the board (per standard carrom rules).
  3. Hand the turn to the other player.

---

## FIX-10 — DailyRewardController delay is 24 seconds instead of 24 hours

**File:** `Assets\_Project\Scripts\Gameplay\Economy\DailyRewardController.cs`

**Root cause:** `Task.Delay(rewardCooldownHours * 1000)` — `Task.Delay` takes
milliseconds. `rewardCooldownHours * 1000` = 24,000 ms = 24 seconds, not 24 hours.

**Fix:**
```csharp
// Wrong:
await Task.Delay(rewardCooldownHours * 1000);

// Correct (hours → ms):
await Task.Delay(TimeSpan.FromHours(rewardCooldownHours));
```

Also verify: the reward claim check should compare `DateTime.UtcNow` against the
stored `lastDailyReward` timestamp — not use a runtime delay. Fix the claim guard:

```csharp
public bool CanClaimToday()
{
    if (!PlayerPrefs.HasKey("LastDailyReward")) return true;
    DateTime last = DateTime.Parse(PlayerPrefs.GetString("LastDailyReward"));
    return (DateTime.UtcNow - last).TotalHours >= 24.0;
}

public void ClaimDailyReward()
{
    if (!CanClaimToday()) return;
    PlayerPrefs.SetString("LastDailyReward", DateTime.UtcNow.ToString("o"));
    PlayerPrefs.Save();
    // ... grant reward ...
}
```

**ACCEPTANCE CHECK:**
- Claim a daily reward. Exit play mode. Re-enter play mode.
- `CanClaimToday()` returns false.
- Manually set `LastDailyReward` in PlayerPrefs to 25 hours ago.
- `CanClaimToday()` returns true.

---

# TIER 3 — BACKEND & PRODUCTION STUBS

These are non-blocking for local gameplay but required before release.
Implement only after Tier 1 and Tier 2 are fully working.

---

## FIX-11 — Firebase is a stub (#if FIREBASE_SDK always false)

**File:** `Assets\_Project\Scripts\Networking\FirebaseService.cs`

**Steps:**
1. Download Firebase Unity SDK from `firebase.google.com/docs/unity/setup`.
2. Import packages: `FirebaseAuth.unitypackage`, `FirebaseFirestore.unitypackage`,
   `FirebaseAnalytics.unitypackage`, `FirebaseCrashlytics.unitypackage`.
3. Add the `google-services.json` (Android) and `GoogleService-Info.plist` (iOS)
   from your Firebase project console to `Assets/`.
4. In `FirebaseService.cs`, the `#if FIREBASE_SDK` define is set automatically by
   the SDK import — no manual define needed once SDK is imported.
5. Test: in `InitializeAsync()`, add `Debug.Log("Firebase initialized: " + app.Name);`
   Run in Editor and confirm this line prints.

**ACCEPTANCE CHECK:**
- Firebase Console → Firestore → players collection contains a document after app launch.
- Firebase Console → Crashlytics → a test event appears within 5 minutes.

---

## FIX-12 — LeaderboardService returns hardcoded fake data

**File:** `Assets\_Project\Scripts\Networking\LeaderboardService.cs`

**Current broken code (look for):**
```csharp
score = 1000 - (i * 50)  // fake, line ~43
```

**Fix:**
Replace the fake-data methods with calls to Nakama's leaderboard API once FIX-11
(Firebase) and the Nakama SDK (FIX-13) are in place. Until then, leave this as a
clearly labeled stub — do not ship with fake leaderboard data:

```csharp
public async Task<List<LeaderboardEntry>> GetGlobalLeaderboard()
{
#if NAKAMA_SDK
    // ... real Nakama call here
#else
    Debug.LogWarning("LeaderboardService: Nakama SDK not imported. Returning empty list.");
    return new List<LeaderboardEntry>();
#endif
}
```

---

## FIX-13 — Nakama multiplayer is a stub

**File:** `Assets\_Project\Scripts\Networking\NakamaService.cs`

**Steps:**
1. Install Docker Desktop.
2. Run Nakama locally:
   ```
   docker-compose up
   ```
   Use the official Nakama Docker Compose from `heroiclabs.com/docs/nakama/getting-started/docker-quickstart/`.
3. Import Nakama Unity SDK (`com.heroiclabs.nakama-unity`) via Package Manager
   (add git URL or download `.unitypackage`).
4. The `#if NAKAMA_SDK` blocks in `NakamaService.cs` activate automatically once the
   package is imported.
5. Update `NakamaService.ConnectAsync()` with your local/dev Nakama server host:
   ```csharp
   const string host = "127.0.0.1";
   const int port = 7350;
   const string serverKey = "defaultkey";
   ```
6. Test: two Editor instances should be able to find each other in a casual match.

**ACCEPTANCE CHECK:**
- Device A presses Play. Device B presses Play. Both see "Match found."
- A shot on Device A produces identical coin positions on Device B.

---

## FIX-14 — CosmeticEquipper modifies shared materials (runtime visual bug)

**File:** `Assets\_Project\Scripts\Gameplay\Cosmetics\CosmeticEquipper.cs`

**Root cause:**
Line ~71-73 assigns a material directly to a renderer without calling
`renderer.material` (which creates an instance). This modifies the shared material
asset on disk and affects ALL objects using that material.

**Fix:**
```csharp
// Wrong — modifies shared asset:
renderer.sharedMaterial = newMaterial;

// Correct — creates a per-instance copy:
renderer.material = newMaterial;
// or, if you need to reuse instances:
Material instance = new Material(newMaterial);
renderer.material = instance;
```

**ACCEPTANCE CHECK:**
- Equip a cosmetic. Change to a different cosmetic.
- The previous cosmetic should NOT be visually affected (no shared material bleed).

---

## FIX-15 — AudioManager memory leak: pool never grows back

**Already covered in FIX-08 above. Mark complete when FIX-08 is done.**

---

# KNOWN ARCHITECTURE WEAKNESSES (do not fix now — note for future)

These are real issues but fixing them now risks destabilizing working code.
Address them only after the entire Tier 1 + Tier 2 list is green.

| Issue | File | Notes |
|---|---|---|
| `FindObjectOfType<>` scattered everywhere | Multiple | Replace with ServiceLocator after all fixes done |
| `IService` interface is empty | Core/IService.cs | Add `Initialize()` method in a future pass |
| CosmeticDatabase uses linear search O(n) | CosmeticDatabase.cs | Convert to Dictionary when adding more items |
| Chat messages lost on close | Social/ChatService.cs | Requires Nakama chat (FIX-13) first |
| MissionSystem: weekly reset uses Monday check incorrectly | MissionSystem.cs | Fix after FIX-12 economy stubs resolved |
| BattlePass uses local DateTime, not server time | BattlePass.cs | Replace with Firebase server timestamp after FIX-11 |

---

# ACCEPTANCE GATE — MINIMUM FOR PLAYABLE BUILD

The game is considered minimally playable when ALL of the following are true
with no console errors and no crashes:

- [ ] FIX-01: Coin enters pocket → score updates
- [ ] FIX-02: Turn switches only after all physics settles
- [ ] FIX-03: Queen cover rule works correctly
- [ ] FIX-04: AI takes its turn automatically
- [ ] FIX-05: Main menu waits for player button press
- [ ] FIX-06: Results screen appears on win and waits for button press
- [ ] FIX-07: HUD is event-driven (no Update poll)
- [ ] FIX-08: Audio pool does not grow during a session
- [ ] FIX-09: Striker-in-pocket foul is detected
- [ ] FIX-10: Daily reward timer is 24 hours, not 24 seconds

---

# ACCEPTANCE GATE — MINIMUM FOR STORE SUBMISSION

After the playable gate is green, the following must also pass:

- [ ] FIX-11: Firebase Auth + Firestore writing real data
- [ ] FIX-12: Leaderboard shows real player data (no fake scores)
- [ ] FIX-13: Two devices can play a full match via Nakama
- [ ] FIX-14: Cosmetic equip does not corrupt shared materials
- [ ] 30 minutes of continuous play on a real Android device with no crash
- [ ] 30 minutes of continuous play on a real iOS device with no crash
- [ ] All 4 scenes in Build Settings in order: Boot(0), MainMenu(1), Game(2), Results(3)

---

# AGENT RULES (any AI must follow these)

1. Read the target file in full before editing it.
2. Fix one item at a time. Run Unity after each fix and check Console.
3. Do not refactor code that is not broken. Do not rename variables unnecessarily.
4. Do not add features. This document is for fixing, not building.
5. If a fix requires creating a new method, put it in the same file unless the
   existing file would exceed ~300 lines — then ask the user before splitting.
6. Physics values (friction, force, drag) are NOT to be changed during fixes unless
   explicitly called out in a fix item above. They are tuning concerns, not bugs.
7. If a fix breaks a passing system (new console error appears that was not there
   before your edit), revert that edit and report the conflict instead of patching forward.
8. The only acceptable definition of "done" for each fix is: its ACCEPTANCE CHECK
   passes in the Unity Editor in Play mode, with no new errors in the Console.
