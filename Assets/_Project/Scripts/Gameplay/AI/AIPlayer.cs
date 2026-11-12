using UnityEngine;
using System.Collections;

public class AIPlayer : MonoBehaviour
{
    public static AIPlayer Instance { get; private set; }

    public const int PlayerIndex = 2;

    [Header("AI Settings")]
    public AIDifficulty difficulty = AIDifficulty.Medium;
    public float thinkingDelay = 1.4f;
    public float slideDuration = 0.45f;
    public float aimPause = 0.42f;

    [Header("References")]
    public StrikerController strikerController;
    public TurnManager turnManager;
    public ScoreManager scoreManager;

    private bool isThinking = false;

    // Approach angles tried for each coin/pocket pair. A small cut keeps the
    // striker from following the coin straight into the pocket.
    private static readonly float[] cutAngles = { 10f, -10f, 16f, -16f, 0f };

    private struct PlannedShot
    {
        public float startX;
        public Vector3 direction;
        public float power;
        public Vector3 aimPoint;
    }

    private void Awake()
    {
        Instance = this;
        InitializeReferences();
    }

    private void InitializeReferences()
    {
        if (strikerController == null)
            strikerController = GetComponent<StrikerController>();
        if (strikerController == null)
            strikerController = FindObjectOfType<StrikerController>(true);

        if (turnManager == null)
            turnManager = FindObjectOfType<TurnManager>();
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();
    }

    /// <summary>Called by TurnManager when it becomes the AI's turn.</summary>
    public void TakeTurn()
    {
        if (isThinking) return;
        if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();
        if (turnManager == null) return;
        if (turnManager.GetCurrentPlayer() != PlayerIndex) return;
        if (scoreManager != null && scoreManager.IsGameEnded) return;

        StartCoroutine(AITurnCoroutine());
    }

    private IEnumerator AITurnCoroutine()
    {
        isThinking = true;

        // Let the HUD announce the turn before anything moves.
        float delay = Mathf.Max(0.2f, thinkingDelay) * UnityEngine.Random.Range(0.9f, 1.15f);
        yield return new WaitForSeconds(delay);

        // Safety: never fire while the previous shot is still being resolved.
        float waited = 0f;
        while (turnManager != null && turnManager.IsShotInProgress() && waited < 3f)
        {
            waited += Time.deltaTime;
            yield return null;
        }

        if (!CanShootNow() || strikerController == null)
        {
            isThinking = false;
            yield break;
        }

        PlannedShot shot;
        bool hasShot = TryPlanShot(out shot);

        if (!hasShot)
        {
            Debug.LogWarning("AIPlayer: no pot available, hitting the nearest coin");
            hasShot = TryPlanFallbackShot(out shot);
        }

        if (!hasShot)
        {
            // Absolute last resort: a firm straight shot so the turn never hangs.
            strikerController.ResetToBaseline(PlayerIndex);
            yield return new WaitForSeconds(0.2f);
            isThinking = false;
            strikerController.ShootTowards(new Vector3(0f, 0f, -1f), 32f);
            yield break;
        }

        // Line up visibly on the baseline, show the aim, pause, then strike.
        strikerController.ResetToBaseline(PlayerIndex);
        yield return new WaitForSeconds(0.12f);

        yield return strikerController.SlideAlongBaseline(shot.startX, slideDuration);

        float normalizedPower = Mathf.InverseLerp(
            strikerController.minForce, strikerController.maxForce, shot.power);
        strikerController.ShowAimPreview(shot.direction, normalizedPower);

        yield return new WaitForSeconds(aimPause);

        strikerController.HideAimPreview();

        if (!CanShootNow())
        {
            isThinking = false;
            yield break;
        }

        Debug.Log($"AIPlayer: aim {shot.aimPoint} from x={shot.startX:F2}, power {shot.power:F1}");

        // Clear the flag before the shot so an extra turn can start immediately.
        isThinking = false;

        if (!strikerController.ShootTowards(shot.direction, shot.power))
        {
            Debug.LogWarning("AIPlayer: shot was rejected, ending the turn to avoid a softlock");
            turnManager?.EndShot();
        }
    }

    private bool CanShootNow()
    {
        return turnManager != null
               && turnManager.GetCurrentPlayer() == PlayerIndex
               && (scoreManager == null || !scoreManager.IsGameEnded);
    }

    /// <summary>
    /// Geometrically correct ghost-ball aim: for every coin/pocket pair, work out
    /// where the striker has to sit on its baseline so that the striker, the coin
    /// and the pocket are exactly in line. This guarantees a straight pot instead
    /// of clamping the striker and firing at a spot it can no longer reach.
    /// </summary>
    private bool TryPlanShot(out PlannedShot shot)
    {
        shot = new PlannedShot { direction = new Vector3(0f, 0f, -1f), power = 34f };

        if (strikerController == null) return false;

        PocketTrigger[] pockets = FindObjectsOfType<PocketTrigger>();
        Coin[] coins = FindObjectsOfType<Coin>();
        if (pockets.Length == 0 || coins.Length == 0) return false;

        CoinSpawner spawner = FindObjectOfType<CoinSpawner>();
        float coinRadius = spawner != null ? spawner.coinRadius : 0.0623f;

        bool hasOwnCoin = false;
        for (int i = 0; i < coins.Length; i++)
        {
            if (IsPlayable(coins[i]) && coins[i].Type == CoinType.Black)
            {
                hasOwnCoin = true;
                break;
            }
        }

        float baselineZ = strikerController.baselineZ;
        float limit = strikerController.baselineWidth * 0.5f - strikerController.strikerRadius;
        float contactDistance = strikerController.strikerRadius + coinRadius;

        float bestScore = float.MaxValue;
        bool found = false;
        PlannedShot best = shot;

        for (int c = 0; c < coins.Length; c++)
        {
            Coin coin = coins[c];
            if (!IsPlayable(coin)) continue;

            // The AI plays black. Only hunt the queen once the black coins are gone.
            if (hasOwnCoin && coin.Type != CoinType.Black) continue;
            if (!hasOwnCoin && coin.Type != CoinType.Queen) continue;

            Vector3 coinPos = Flat(coin.transform.position);

            for (int p = 0; p < pockets.Length; p++)
            {
                PocketTrigger pocket = pockets[p];
                if (pocket == null) continue;

                Vector3 pocketPos = Flat(pocket.transform.position);
                Vector3 toPocket = pocketPos - coinPos;
                float pocketDistance = toPocket.magnitude;

                if (pocketDistance < coinRadius * 1.5f) continue;

                Vector3 pocketDirection = toPocket / pocketDistance;

                // Near-horizontal lines put the striker start far off the board.
                if (Mathf.Abs(pocketDirection.z) < 0.14f) continue;

                Vector3 ghost = coinPos - pocketDirection * contactDistance;

                // The contact point has to be on the playing surface.
                if (Mathf.Abs(ghost.x) > 1.38f || Mathf.Abs(ghost.z) > 1.38f) continue;
                if (ghost.z > baselineZ - strikerController.strikerRadius) continue;

                // Don't fire through the pack on the way in.
                // (The start position depends on the cut, so it is checked below.)

                if (IsPathBlocked(coinPos, pocketPos, coinRadius * 1.35f, coin, coins, coinRadius))
                    continue;

                // A straight pot sends the striker down the same line, straight
                // into the pocket after the coin. A small cut sends it sideways
                // while the coin still rolls into the pocket.
                for (int a = 0; a < cutAngles.Length; a++)
                {
                    float cut = cutAngles[a];
                    Vector3 approach = Quaternion.Euler(0f, cut, 0f) * pocketDirection;

                    if (Mathf.Abs(approach.z) < 0.14f) continue;

                    float t = (ghost.z - baselineZ) / approach.z;
                    if (t < contactDistance * 0.75f) continue;

                    float startX = ghost.x - approach.x * t;
                    if (Mathf.Abs(startX) > limit) continue;

                    Vector3 start = new Vector3(startX, 0f, baselineZ);

                    if (IsPathBlocked(start, ghost,
                            strikerController.strikerRadius + coinRadius * 0.5f, coin, coins, coinRadius))
                        continue;

                    float score = t * 0.9f + pocketDistance * 0.55f + Mathf.Abs(cut) * 0.022f;
                    score += UnityEngine.Random.Range(0f, 0.12f);

                    if (coin.Type == CoinType.Queen) score -= 0.45f;

                    if (score < bestScore)
                    {
                        bestScore = score;
                        best.startX = startX;
                        best.direction = approach;
                        best.aimPoint = ghost;
                        best.power = Mathf.Clamp(
                            20f + t * 8f + pocketDistance * 4f, 28f, 46f);
                        found = true;
                    }
                }
            }
        }

        if (!found) return false;

        ApplyAccuracyNoise(ref best);
        shot = best;
        return true;
    }

    /// <summary>
    /// Fallback when no pot is available: line up on the nearest reachable coin and
    /// hit it squarely. Always makes contact, so the AI never fouls by missing.
    /// </summary>
    private bool TryPlanFallbackShot(out PlannedShot shot)
    {
        shot = new PlannedShot { direction = new Vector3(0f, 0f, -1f), power = 34f };

        if (strikerController == null) return false;

        Coin[] coins = FindObjectsOfType<Coin>();
        if (coins.Length == 0) return false;

        float baselineZ = strikerController.baselineZ;
        float limit = strikerController.baselineWidth * 0.5f - strikerController.strikerRadius;
        Vector3 intoBoard = new Vector3(0f, 0f, -1f);

        Coin best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < coins.Length; i++)
        {
            Coin coin = coins[i];
            if (!IsPlayable(coin)) continue;

            Vector3 coinPos = Flat(coin.transform.position);
            Vector3 toCoin = coinPos;
            toCoin.z -= baselineZ;

            if (Vector3.Dot(toCoin.normalized, intoBoard) < 0.05f) continue;

            float distance = toCoin.magnitude;
            float typeWeight = coin.Type == CoinType.Black ? 0.8f
                             : coin.Type == CoinType.Queen ? 0.9f
                             : 1.15f;
            distance *= typeWeight;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = coin;
            }
        }

        if (best == null) return false;

        Vector3 bestPos = Flat(best.transform.position);
        float startX = Mathf.Clamp(bestPos.x, -limit, limit);
        Vector3 start = new Vector3(startX, 0f, baselineZ);

        Vector3 direction = bestPos - start;
        direction.y = 0f;

        float shotDistance = direction.magnitude;
        if (shotDistance < 0.05f) return false;
        direction /= shotDistance;

        if (Vector3.Dot(direction, intoBoard) < 0.1f) return false;

        shot.startX = startX;
        shot.direction = direction;
        shot.aimPoint = bestPos;
        shot.power = Mathf.Clamp(18f + shotDistance * 10f, 30f, 44f);

        ApplyAccuracyNoise(ref shot);
        return true;
    }

    private void ApplyAccuracyNoise(ref PlannedShot shot)
    {
        AINoiseParameters noise = AIDifficultySettings.GetNoiseParameters(difficulty);

        // The aim is already solved, so only a small error is applied - otherwise
        // the AI would look like it deliberately misses.
        float angleNoise = UnityEngine.Random.Range(-noise.angleNoise, noise.angleNoise) * 0.45f;
        float powerNoise = UnityEngine.Random.Range(
            -noise.powerNoisePercent, noise.powerNoisePercent) * 0.5f;

        shot.direction = Quaternion.Euler(0f, angleNoise, 0f) * shot.direction;
        shot.direction.Normalize();

        shot.power = Mathf.Clamp(
            shot.power * (1f + powerNoise / 100f),
            strikerController.minForce,
            strikerController.maxForce);
    }

    private static bool IsPlayable(Coin coin)
    {
        return coin != null
               && !coin.IsPocketed
               && coin.gameObject.activeInHierarchy;
    }

    private static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v;
    }

    private static bool IsPathBlocked(
        Vector3 start, Vector3 end, float clearance,
        Coin ignore, Coin[] coins, float coinRadius)
    {
        Vector3 segment = end - start;
        float lengthSq = segment.sqrMagnitude;
        if (lengthSq < 0.0001f) return false;

        float limit = clearance + coinRadius;
        float limitSq = limit * limit;

        for (int i = 0; i < coins.Length; i++)
        {
            Coin other = coins[i];
            if (other == ignore || !IsPlayable(other)) continue;

            Vector3 point = Flat(other.transform.position);
            float t = Vector3.Dot(point - start, segment) / lengthSq;
            if (t <= 0.02f || t >= 0.98f) continue;

            Vector3 closest = start + segment * t;
            if ((point - closest).sqrMagnitude < limitSq)
                return true;
        }

        return false;
    }

    public void SetDifficulty(AIDifficulty newDifficulty)
    {
        difficulty = newDifficulty;
        Debug.Log($"AI difficulty set to: {difficulty}");
    }
}
