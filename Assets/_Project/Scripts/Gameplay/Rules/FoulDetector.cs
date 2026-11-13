using UnityEngine;

public class FoulDetector : MonoBehaviour
{
    public static FoulDetector Instance { get; private set; }

    [Header("Foul Detection Settings")]
    public float foulTimeout = 30f;
    public LayerMask coinLayer;

    [Header("References")]
    public TurnManager turnManager;
    public ScoreManager scoreManager;
    public CoinSpawner coinSpawner;
    public GameHUD gameHUD;

    private bool shotActive = false;
    private bool coinTouched = false;
    private bool coinPocketed = false;
    private bool strikerPocketed = false;

    // The striker dropping in is reported on impact so the player immediately
    // understands why the shot stopped; the penalty summary refreshes the same
    // card once the shot settles instead of stacking a second banner.
    private bool strikerFoulAnnounced = false;
    private bool foulSoundPlayed = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();
        if (scoreManager == null) scoreManager = FindObjectOfType<ScoreManager>();
        if (coinSpawner == null) coinSpawner = FindObjectOfType<CoinSpawner>();

        if (gameHUD == null && scoreManager != null) gameHUD = scoreManager.gameHUD;
        if (gameHUD == null) gameHUD = FindObjectOfType<GameHUD>(true);
    }

    /// <summary>Called by TurnManager.BeginShot() for both human and AI shots.</summary>
    public void RegisterShot()
    {
        shotActive = true;
        coinTouched = false;
        coinPocketed = false;
        strikerPocketed = false;
        strikerFoulAnnounced = false;
        foulSoundPlayed = false;
    }

    /// <summary>Called by the striker when it physically touches a coin.</summary>
    public void RegisterCoinTouched()
    {
        coinTouched = true;
    }

    /// <summary>Called by PocketTrigger when a coin is pocketed.</summary>
    public void RegisterPocketedCoin(Coin coin)
    {
        coinPocketed = true;
        coinTouched = true;
    }

    /// <summary>Called by PocketTrigger when the striker enters a pocket.</summary>
    public void RegisterStrikerPocketed()
    {
        strikerPocketed = true;

        // Fire the on-screen call immediately. Waiting for the shot to settle
        // meant the striker had already teleported back to the baseline and the
        // turn silently changed, which looked like a bug.
        if (shotActive && !strikerFoulAnnounced)
        {
            strikerFoulAnnounced = true;

            if (gameHUD == null) gameHUD = FindObjectOfType<GameHUD>(true);

            int player = turnManager != null ? turnManager.GetCurrentPlayer() : 1;
            int opponent = player == 1 ? 2 : 1;

            PlayFoulSoundOnce();

            gameHUD?.ShowFoul("STRIKER POCKETED",
                $"Foul against {PlayerName(player)}\nTurn passes to {PlayerName(opponent)}");
        }
    }

    /// <summary>
    /// Evaluated after a shot has fully settled, before the turn switches.
    /// A foul is either the striker being pocketed or the striker touching no coin.
    /// Returns true when a foul was committed.
    /// </summary>
    public bool EndTurnAndCheckFouls()
    {
        if (!shotActive) return false;
        shotActive = false;

        int player = turnManager != null ? turnManager.GetCurrentPlayer() : 1;
        int opponent = player == 1 ? 2 : 1;

        bool foul = strikerPocketed || (!coinTouched && !coinPocketed);

        if (foul)
        {
            string reason = strikerPocketed ? "striker pocketed" : "no coin touched";
            Debug.LogWarning($"Player {player} fouled: {reason}");

            bool coinReturned = false;
            if (scoreManager != null)
                coinReturned = scoreManager.ApplyFoulPenalty(player);

            if (gameHUD == null) gameHUD = FindObjectOfType<GameHUD>(true);

            PlayFoulSoundOnce();

            string detail = coinReturned
                ? $"1 coin returned to the board\nTurn passes to {PlayerName(opponent)}"
                : $"Turn passes to {PlayerName(opponent)}";

            gameHUD?.ShowFoul(
                strikerPocketed ? "STRIKER POCKETED" : "NO COIN TOUCHED", detail);
        }

        coinTouched = false;
        coinPocketed = false;
        strikerPocketed = false;
        strikerFoulAnnounced = false;
        return foul;
    }

    private string PlayerName(int player)
    {
        if (player == 1) return "You";
        return turnManager != null && turnManager.aiControlsPlayer2 ? "AI" : "Player 2";
    }

    private void PlayFoulSoundOnce()
    {
        if (foulSoundPlayed) return;
        foulSoundPlayed = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayFoulSound();
    }
}
