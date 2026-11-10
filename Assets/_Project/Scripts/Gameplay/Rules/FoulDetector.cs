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

    private bool shotActive = false;
    private bool coinTouched = false;
    private bool coinPocketed = false;
    private bool strikerPocketed = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();
        if (scoreManager == null) scoreManager = FindObjectOfType<ScoreManager>();
        if (coinSpawner == null) coinSpawner = FindObjectOfType<CoinSpawner>();
    }

    /// <summary>Called by TurnManager.BeginShot() for both human and AI shots.</summary>
    public void RegisterShot()
    {
        shotActive = true;
        coinTouched = false;
        coinPocketed = false;
        strikerPocketed = false;
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

        bool foul = strikerPocketed || (!coinTouched && !coinPocketed);

        if (foul)
        {
            if (strikerPocketed)
                Debug.LogWarning($"Player {player} fouled: striker pocketed");
            else
                Debug.LogWarning($"Player {player} fouled: no coin touched");

            if (scoreManager != null)
                scoreManager.ApplyFoulPenalty(player);
        }

        coinTouched = false;
        coinPocketed = false;
        strikerPocketed = false;
        return foul;
    }
}
