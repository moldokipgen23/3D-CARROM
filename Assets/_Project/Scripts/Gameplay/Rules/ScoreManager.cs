using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Player Settings")]
    public int whitePlayerScore = 0;
    public int blackPlayerScore = 0;

    [Header("Game Settings")]
    public int coinsToWin = 9;

    [Header("References")]
    public TurnManager turnManager;
    public GameHUD gameHUD;

    // Index 0 = White / Player 1, index 1 = Black / Player 2
    private int[] playerScores = new int[2];

    private bool queenOnBoard = true;
    private bool queenPocketedThisTurn = false;
    private bool queenCoveredThisTurn = false;
    private int queenPocketedByPlayer = 0;
    private int queenAwardedTo = 0;
    private bool gameEnded = false;
    private bool currentPlayerPocketedThisTurn = false;
    private bool ownCoinPocketedThisShot = false;

    public event Action<int, int> OnScoreUpdated;

    public bool IsGameEnded { get { return gameEnded; } }
    public bool IsQueenOnBoard { get { return queenOnBoard; } }

    /// <summary>
    /// True when the player whose turn is currently resolving has pocketed at
    /// least one of their own coins. TurnManager uses this to keep the turn.
    /// </summary>
    public bool CurrentPlayerPocketedThisTurn { get { return currentPlayerPocketedThisTurn; } }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeScores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (turnManager == null)
            turnManager = FindObjectOfType<TurnManager>();
        if (gameHUD == null)
            gameHUD = FindObjectOfType<GameHUD>(true);

        UpdateHUD();
    }

    private void InitializeScores()
    {
        playerScores[0] = 0;
        playerScores[1] = 0;
        queenOnBoard = true;
        queenPocketedThisTurn = false;
        queenCoveredThisTurn = false;
        queenPocketedByPlayer = 0;
        queenAwardedTo = 0;
        gameEnded = false;
        currentPlayerPocketedThisTurn = false;
        ownCoinPocketedThisShot = false;
    }

    /// <summary>
    /// Single entry point used by PocketTrigger whenever a coin enters a pocket.
    /// White coins belong to Player 1, black coins belong to Player 2.
    /// The queen is held pending until the same player covers it in the same turn.
    /// </summary>
    public void PocketCoin(Coin coin)
    {
        if (gameEnded || coin == null) return;

        int currentPlayer = GetCurrentPlayer();

        if (coin.Type == CoinType.Queen)
        {
            queenOnBoard = false;
            queenPocketedThisTurn = true;
            queenPocketedByPlayer = currentPlayer;

            // If the player already pocketed one of their own coins earlier in
            // this same shot, that coin covers the queen.
            queenCoveredThisTurn = ownCoinPocketedThisShot;

            // Deadlock guard: if the player has no coins left to use as a cover,
            // pocketing the queen automatically counts as covered.
            if (playerScores[currentPlayer - 1] >= coinsToWin)
            {
                queenCoveredThisTurn = true;
                Debug.Log($"Player {currentPlayer} pocketed the queen with no coins left - auto-covered");
            }
            else if (queenCoveredThisTurn)
            {
                Debug.Log($"Player {currentPlayer} pocketed the queen and covered it in the same shot");
            }
            else
            {
                Debug.Log($"Player {currentPlayer} pocketed the queen - cover required");
            }
        }
        else
        {
            int owner = coin.Type == CoinType.White ? 1 : 2;
            playerScores[owner - 1]++;

            if (owner == currentPlayer)
            {
                currentPlayerPocketedThisTurn = true;
                ownCoinPocketedThisShot = true;
            }

            if (queenPocketedThisTurn && queenPocketedByPlayer == currentPlayer && owner == currentPlayer)
            {
                queenCoveredThisTurn = true;
                Debug.Log($"Player {currentPlayer} covered the queen with a {coin.Type} coin");
            }

            Debug.Log($"Player {owner} now has {playerScores[owner - 1]}/{coinsToWin} coins");
        }

        UpdateScore(currentPlayer);
    }

    /// <summary>
    /// Backwards-compatible entry point (older callers/tests). New code should use PocketCoin.
    /// </summary>
    public void AddCoins(int player, CoinType coinType, int count)
    {
        if (gameEnded) return;

        if (coinType == CoinType.Queen)
        {
            RegisterQueenPocketed(player);
            return;
        }

        int playerIndex = player - 1;
        if (playerIndex < 0 || playerIndex >= 2) return;

        playerScores[playerIndex] += count;
        UpdateScore(player);
    }

    /// <summary>
    /// Called at the end of a turn, after all physics has settled, before the turn switches.
    /// Awards a covered queen or returns an uncovered queen to the centre spot.
    /// </summary>
    public void OnTurnEnd()
    {
        if (gameEnded) return;

        if (queenPocketedThisTurn)
        {
            if (queenCoveredThisTurn)
            {
                queenAwardedTo = queenPocketedByPlayer;
                queenOnBoard = false;
                Debug.Log($"Player {queenAwardedTo} takes the queen");
            }
            else
            {
                Debug.Log($"Player {queenPocketedByPlayer} failed to cover the queen - returning it to the board");
                ReturnQueenToBoard();
            }
        }

        queenPocketedThisTurn = false;
        queenCoveredThisTurn = false;
        queenPocketedByPlayer = 0;
        currentPlayerPocketedThisTurn = false;
        ownCoinPocketedThisShot = false;

        CheckWinCondition();
        UpdateHUD();
    }

    private void ReturnQueenToBoard()
    {
        queenOnBoard = true;

        Coin[] coins = FindObjectsOfType<Coin>(true);
        for (int i = 0; i < coins.Length; i++)
        {
            if (coins[i] != null && coins[i].Type == CoinType.Queen)
            {
                coins[i].ResetCoin();
                Debug.Log("Queen returned to the centre spot");
                return;
            }
        }

        Debug.LogWarning("ScoreManager could not find the queen coin to return it");
    }

    /// <summary>
    /// Legacy helper - marks the queen as permanently awarded to a player.
    /// </summary>
    public void RegisterQueenPocketed(int player)
    {
        if (gameEnded) return;

        int playerIndex = player - 1;
        if (playerIndex < 0 || playerIndex >= 2) return;

        queenAwardedTo = player;
        queenOnBoard = false;
        Debug.Log($"Player {player} has properly pocketed the queen!");
        UpdateHUD();
    }

    public bool HasQueenPocketed(int player)
    {
        return queenAwardedTo == player;
    }

    public bool ApplyFoulPenalty(int player)
    {
        if (gameEnded) return false;

        int playerIndex = player - 1;
        if (playerIndex < 0 || playerIndex >= 2) return false;

        bool returnedCoin = false;
        if (playerScores[playerIndex] > 0)
        {
            playerScores[playerIndex]--;
            returnedCoin = ReturnPocketedCoinToBoard(player);
        }

        Debug.LogWarning($"Player {player} fouled - one coin returned. Total: {playerScores[playerIndex]}");
        UpdateScore(player);
        return returnedCoin;
    }

    /// <summary>
    /// Real carrom foul rule: one already-pocketed coin goes back to the board,
    /// placed on the centre spot (or the nearest free spot around it).
    /// </summary>
    private bool ReturnPocketedCoinToBoard(int player)
    {
        CoinType type = player == 1 ? CoinType.White : CoinType.Black;
        Coin[] coins = FindObjectsOfType<Coin>(true);

        Coin target = null;
        for (int i = 0; i < coins.Length; i++)
        {
            if (coins[i] != null && coins[i].Type == type && coins[i].IsPocketed)
            {
                target = coins[i];
                break;
            }
        }

        if (target == null) return false;

        target.ResetCoin();

        Vector3 spawn = target.transform.position;
        Vector3 spot = FindOpenCentreSpot(spawn.y, 0.11f, target, coins);

        target.transform.position = spot;
        if (target.Rigidbody != null)
        {
            target.Rigidbody.position = spot;
            target.Rigidbody.rotation = Quaternion.identity;
            target.Rigidbody.linearVelocity = Vector3.zero;
            target.Rigidbody.angularVelocity = Vector3.zero;
        }

        Debug.Log($"Player {player}'s {type} coin returned to the centre");
        return true;
    }

    private Vector3 FindOpenCentreSpot(float y, float radius, Coin ignore, Coin[] coins)
    {
        StrikerController striker = FindObjectOfType<StrikerController>(true);

        for (int ring = 0; ring < 8; ring++)
        {
            float ringRadius = ring == 0 ? 0f : 0.12f + (ring - 1) * 0.11f;
            int steps = ring == 0 ? 1 : 8 * ring;

            for (int i = 0; i < steps; i++)
            {
                float angle = Mathf.PI * 2f * i / steps;
                Vector3 point = new Vector3(
                    Mathf.Cos(angle) * ringRadius, y, Mathf.Sin(angle) * ringRadius);

                if (IsFreeSpot(point, radius, ignore, coins, striker))
                    return point;
            }
        }

        return new Vector3(0f, y, 0f);
    }

    private static bool IsFreeSpot(Vector3 point, float radius, Coin ignore,
        Coin[] coins, StrikerController striker)
    {
        float limitSq = radius * radius;

        for (int i = 0; i < coins.Length; i++)
        {
            Coin coin = coins[i];
            if (coin == null || coin == ignore) continue;
            if (coin.IsPocketed || !coin.gameObject.activeInHierarchy) continue;

            Vector3 delta = coin.transform.position - point;
            delta.y = 0f;
            if (delta.sqrMagnitude < limitSq) return false;
        }

        if (striker != null)
        {
            Vector3 delta = striker.transform.position - point;
            delta.y = 0f;
            float strikerLimit = radius + striker.strikerRadius;
            if (delta.sqrMagnitude < strikerLimit * strikerLimit) return false;
        }

        return true;
    }

    public void ResetScores()
    {
        InitializeScores();
        Debug.Log("All scores reset");
        UpdateHUD();
    }

    private int GetCurrentPlayer()
    {
        TurnManager tm = turnManager != null ? turnManager : TurnManager.Instance;
        return tm != null ? tm.GetCurrentPlayer() : 1;
    }

    private void UpdateScore(int player)
    {
        int playerIndex = Mathf.Clamp(player - 1, 0, 1);
        OnScoreUpdated?.Invoke(player, playerScores[playerIndex]);
        UpdateHUD();
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (gameEnded) return;

        // A player wins once all of their coins are pocketed and the queen has
        // been pocketed and covered (queenAwardedTo != 0). A queen that is still
        // pending cover, or that was returned to the board, does not count.
        for (int p = 1; p <= 2; p++)
        {
            if (playerScores[p - 1] >= coinsToWin && queenAwardedTo != 0)
            {
                EndGame(p);
                return;
            }
        }
    }

    private void EndGame(int winner)
    {
        gameEnded = true;

        bool aiControlsPlayer2 = turnManager != null && turnManager.aiControlsPlayer2;
        string winnerName = winner == 2 && aiControlsPlayer2 ? "AI" : $"Player {winner}";
        string message = $"{winnerName} wins!";

        Debug.Log($"Game ended: {message}");
        turnManager?.StopGame();

        ResultsScreen resultsScreen = FindObjectOfType<ResultsScreen>(true);
        if (resultsScreen != null)
        {
            resultsScreen.ShowResults(winner, message);
        }
        else
        {
            Debug.Log($"GAME OVER - {message}");
        }
    }

    public int GetPlayerScore(int player)
    {
        if (player == 3) return queenAwardedTo;
        if (player < 1 || player > 2) return 0;
        return playerScores[player - 1];
    }

    public int GetRemainingCoins()
    {
        return Mathf.Max(0, 18 - playerScores[0] - playerScores[1]);
    }

    public string GetQueenStatusText()
    {
        if (queenPocketedThisTurn && !queenCoveredThisTurn)
            return $"Queen: P{queenPocketedByPlayer} needs cover";
        if (queenAwardedTo != 0)
            return $"Queen: P{queenAwardedTo}";
        if (queenOnBoard)
            return "Queen: on board";
        return "Queen: pocketed";
    }

    public void UpdateHUD()
    {
        if (gameHUD == null)
            gameHUD = FindObjectOfType<GameHUD>(true);
        if (gameHUD != null)
            gameHUD.Refresh();
    }
}
