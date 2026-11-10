using UnityEngine;
using System.Collections;

public class AIPlayer : MonoBehaviour
{
    public static AIPlayer Instance { get; private set; }

    public const int PlayerIndex = 2;

    [Header("AI Settings")]
    public AIDifficulty difficulty = AIDifficulty.Medium;
    public float thinkingDelay = 1.0f;

    [Header("References")]
    public StrikerController strikerController;
    public TurnManager turnManager;
    public ScoreManager scoreManager;

    private bool isThinking = false;

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

        float delay = Mathf.Max(0.2f, thinkingDelay) * UnityEngine.Random.Range(0.6f, 1.4f);
        yield return new WaitForSeconds(delay);

        // Safety: never fire while the previous shot is still being resolved.
        float waited = 0f;
        while (turnManager != null && turnManager.IsShotInProgress() && waited < 2f)
        {
            waited += Time.deltaTime;
            yield return null;
        }

        bool canShoot = turnManager != null
                        && turnManager.GetCurrentPlayer() == PlayerIndex
                        && (scoreManager == null || !scoreManager.IsGameEnded);

        if (!canShoot || strikerController == null)
        {
            isThinking = false;
            yield break;
        }

        Vector3 direction;
        float power;

        if (CalculateBestShot(out direction, out power))
        {
            if (!strikerController.ShootTowards(direction, power))
            {
                Debug.LogWarning("AIPlayer: shot was rejected, ending turn to avoid a softlock");
                turnManager?.EndShot();
            }
        }
        else
        {
            // No pocket shot found: hit the nearest coin instead of firing at
            // an empty centre spot (which used to register as a foul).
            Vector3 fallbackDirection;
            float fallbackPower;
            if (TryCalculateFallbackShot(out fallbackDirection, out fallbackPower))
            {
                // TryCalculateFallbackShot has already lined the striker up with
                // the target coin; do not reset it again or the aim is lost.
                strikerController.ShootTowards(fallbackDirection, fallbackPower);
            }
            else
            {
                Debug.LogWarning("AIPlayer: no valid shot found, aiming at the centre");
                strikerController.ResetToBaseline(PlayerIndex);

                Vector3 fallback = Vector3.zero - strikerController.transform.position;
                fallback.y = 0f;

                if (fallback.sqrMagnitude > 0.0001f)
                    strikerController.ShootTowards(fallback.normalized, 28f);
                else
                    turnManager?.EndShot();
            }
        }

        isThinking = false;
    }

    /// <summary>
    /// Ghost-ball aiming: find a coin/pocket pair where the striker can hit the
    /// coin on the correct side so that the coin travels into the pocket.
    /// </summary>
    private bool CalculateBestShot(out Vector3 direction, out float power)
    {
        direction = Vector3.zero;
        power = 8f;

        if (strikerController == null) return false;

        PocketTrigger[] pockets = FindObjectsOfType<PocketTrigger>();
        Coin[] coins = FindObjectsOfType<Coin>();

        if (pockets.Length == 0 || coins.Length == 0) return false;

        bool hasOwnCoin = false;
        for (int i = 0; i < coins.Length; i++)
        {
            if (coins[i] != null && !coins[i].IsPocketed
                && coins[i].gameObject.activeInHierarchy
                && coins[i].Type == CoinType.Black)
            {
                hasOwnCoin = true;
                break;
            }
        }

        Vector3 strikerPos = strikerController.transform.position;
        float bestScore = float.MaxValue;
        Vector3 bestGhost = Vector3.zero;
        bool found = false;

        for (int c = 0; c < coins.Length; c++)
        {
            Coin coin = coins[c];
            if (coin == null || coin.IsPocketed || !coin.gameObject.activeInHierarchy) continue;

            if (hasOwnCoin && coin.Type != CoinType.Black) continue;
            if (!hasOwnCoin && coin.Type != CoinType.Queen) continue;

            Vector3 coinPos = coin.transform.position;
            coinPos.y = 0f;

            for (int p = 0; p < pockets.Length; p++)
            {
                PocketTrigger pocket = pockets[p];
                if (pocket == null) continue;

                Vector3 pocketPos = pocket.transform.position;
                pocketPos.y = 0f;

                Vector3 toPocket = pocketPos - coinPos;
                float pocketDistance = toPocket.magnitude;
                if (pocketDistance < 0.06f) continue;

                Vector3 pocketDirection = toPocket / pocketDistance;
                Vector3 ghost = coinPos - pocketDirection * (strikerController.strikerRadius + 0.04f);

                // The ghost target must be inside the playing surface, otherwise
                // the striker would be aimed at (or through) a wall.
                if (Mathf.Abs(ghost.x) > 1.35f || Mathf.Abs(ghost.z) > 1.35f) continue;

                Vector3 toGhost = ghost - strikerPos;
                toGhost.y = 0f;
                float ghostDistance = toGhost.magnitude;
                if (ghostDistance < 0.08f) continue;

                Vector3 shootDirection = toGhost / ghostDistance;

                // Player 2 shoots from the +Z baseline towards -Z. Reject any
                // target that would send the striker away from the board.
                Vector3 intoBoard = new Vector3(0f, 0f, -1f);
                if (Vector3.Dot(shootDirection, intoBoard) < 0.2f) continue;

                float cutAngle = Vector3.Angle(shootDirection, pocketDirection);
                if (cutAngle > 70f) continue;

                float score = cutAngle * 1.5f + ghostDistance * 0.4f + pocketDistance * 0.6f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestGhost = ghost;
                    found = true;
                }
            }
        }

        if (!found) return false;

        // Move the striker along the baseline so it lines up with the ghost ball.
        strikerController.ResetToBaseline(PlayerIndex);
        strikerController.PlaceOnBaseline(bestGhost.x);

        strikerPos = strikerController.transform.position;
        Vector3 finalDirection = bestGhost - strikerPos;
        finalDirection.y = 0f;

        if (finalDirection.sqrMagnitude < 0.0001f) return false;

        float distanceToGhost = finalDirection.magnitude;
        finalDirection.Normalize();

        float rawPower = Mathf.Clamp(distanceToGhost * 12f + 16f, 24f, 46f);

        AINoiseParameters noise = AIDifficultySettings.GetNoiseParameters(difficulty);
        float angleNoise = UnityEngine.Random.Range(-noise.angleNoise, noise.angleNoise);
        float powerNoise = UnityEngine.Random.Range(-noise.powerNoisePercent, noise.powerNoisePercent);

        direction = Quaternion.Euler(0f, angleNoise, 0f) * finalDirection;
        direction.Normalize();

        power = Mathf.Clamp(
            rawPower * (1f + powerNoise / 100f),
            strikerController.minForce,
            strikerController.maxForce);

        return true;
    }

    /// <summary>
    /// Fallback when no pocket is reachable: line up on the baseline and hit the
    /// nearest coin, preferring the AI's own coins.
    /// </summary>
    private bool TryCalculateFallbackShot(out Vector3 direction, out float power)
    {
        direction = Vector3.zero;
        power = 30f;

        if (strikerController == null) return false;

        Coin[] coins = FindObjectsOfType<Coin>();
        Coin best = null;
        float bestScore = float.MaxValue;
        Vector3 intoBoard = new Vector3(0f, 0f, -1f);

        for (int i = 0; i < coins.Length; i++)
        {
            Coin coin = coins[i];
            if (coin == null || coin.IsPocketed || !coin.gameObject.activeInHierarchy) continue;

            Vector3 toCoinCheck = coin.transform.position - strikerController.transform.position;
            toCoinCheck.y = 0f;

            // Ignore coins behind the AI (never shoot back into its own wall).
            if (Vector3.Dot(toCoinCheck.normalized, intoBoard) < 0.1f) continue;

            float typeWeight = coin.Type == CoinType.Black ? 1f
                             : coin.Type == CoinType.Queen ? 1.2f
                             : 1.6f;

            float distance = toCoinCheck.magnitude * typeWeight;

            if (distance < bestScore)
            {
                bestScore = distance;
                best = coin;
            }
        }

        if (best == null) return false;

        strikerController.ResetToBaseline(PlayerIndex);
        strikerController.PlaceOnBaseline(best.transform.position.x);

        Vector3 toCoin = best.transform.position - strikerController.transform.position;
        toCoin.y = 0f;

        float distanceToCoin = toCoin.magnitude;
        if (distanceToCoin < 0.05f) return false;

        direction = toCoin / distanceToCoin;
        // Hitting a coin square needs less force than a cut shot.
        power = Mathf.Clamp(distanceToCoin * 10f + 16f, 24f, 44f);
        return true;
    }

    public void SetDifficulty(AIDifficulty newDifficulty)
    {
        difficulty = newDifficulty;
        Debug.Log($"AI difficulty set to: {difficulty}");
    }
}
