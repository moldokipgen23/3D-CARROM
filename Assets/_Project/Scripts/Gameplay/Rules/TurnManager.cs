using UnityEngine;
using System;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Game Settings")]
    public int maxTurns = 100;
    public float shotTimeout = 5f;
    public bool aiControlsPlayer2 = true;

    [Header("Player References")]
    // Kept for backwards compatibility with the existing scene/tests.
    // Player 1 and Player 2 share the same striker object; these are no longer
    // used to enable/disable the striker (that bug disabled the striker on Start).
    public GameObject player1;
    public GameObject player2;

    public event Action<int> OnTurnChanged;
    public event Action<int, int> OnScoreUpdated;

    private int currentPlayer = 1;
    private int turnCount = 0;
    private bool isShotInProgress = false;
    private bool resolvingShot = false;
    private bool gameOver = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // No turn is started here. GameSceneManager calls StartGame() only after
        // the board, coins, striker and UI are ready.
    }

    public void StartGame()
    {
        gameOver = false;
        currentPlayer = 1;
        turnCount = 0;
        isShotInProgress = false;
        resolvingShot = false;

        StrikerController.Instance?.ResetToBaseline(currentPlayer);

        OnTurnChanged?.Invoke(currentPlayer);
        Debug.Log("Game started - Player 1's turn");

        NotifyAITurn();
    }

    public void BeginShot()
    {
        if (gameOver) return;

        isShotInProgress = true;
        FoulDetector.Instance?.RegisterShot();
        Debug.Log($"Player {currentPlayer}'s shot started");
    }

    public void EndShot()
    {
        if (gameOver || resolvingShot) return;
        StartCoroutine(ResolveShot());
    }

    private IEnumerator ResolveShot()
    {
        resolvingShot = true;

        // Give the physics engine at least one fixed step to apply the force.
        yield return new WaitForFixedUpdate();

        const float sleepThreshold = 0.03f;
        const float maxWait = 8f;
        float waited = 0f;

        while (waited < maxWait)
        {
            bool allSleeping = true;

            Rigidbody[] bodies = FindObjectsOfType<Rigidbody>();
            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody rb = bodies[i];
                if (rb == null || !rb.gameObject.activeInHierarchy || rb.isKinematic) continue;

                if (rb.linearVelocity.sqrMagnitude > sleepThreshold * sleepThreshold)
                {
                    allSleeping = false;
                    break;
                }
            }

            if (allSleeping) break;

            waited += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isShotInProgress = false;

        // Fouls are evaluated after the shot has fully settled.
        bool fouled = false;
        if (FoulDetector.Instance != null)
            fouled = FoulDetector.Instance.EndTurnAndCheckFouls();

        // Read this before OnTurnEnd() resets the per-turn flags.
        bool earnedExtraTurn = !fouled
            && ScoreManager.Instance != null
            && ScoreManager.Instance.CurrentPlayerPocketedThisTurn;

        // Queen cover / return is resolved after fouls, still before the switch.
        ScoreManager.Instance?.OnTurnEnd();

        if (gameOver || (ScoreManager.Instance != null && ScoreManager.Instance.IsGameEnded))
        {
            resolvingShot = false;
            yield break;
        }

        if (earnedExtraTurn)
        {
            // Standard carrom: a clean shot that pockets your own coin keeps the turn.
            StrikerController.Instance?.ResetToBaseline(currentPlayer);
            OnTurnChanged?.Invoke(currentPlayer);
            Debug.Log($"Player {currentPlayer} pocketed a coin - same player shoots again");

            NotifyAITurn();
        }
        else
        {
            SwitchTurn();
        }

        resolvingShot = false;
    }

    public void SwitchTurn()
    {
        if (gameOver) return;

        currentPlayer = currentPlayer == 1 ? 2 : 1;
        turnCount++;

        StrikerController.Instance?.ResetToBaseline(currentPlayer);

        OnTurnChanged?.Invoke(currentPlayer);
        Debug.Log($"Turn switched to Player {currentPlayer}");

        NotifyAITurn();
    }

    private void NotifyAITurn()
    {
        if (gameOver || !aiControlsPlayer2 || currentPlayer != 2) return;

        AIPlayer ai = AIPlayer.Instance;
        if (ai == null)
            ai = FindObjectOfType<AIPlayer>(true);

        if (ai != null)
            ai.TakeTurn();
    }

    public void StopGame()
    {
        gameOver = true;
        isShotInProgress = false;
        resolvingShot = false;
        StopAllCoroutines();
        Debug.Log("TurnManager stopped - game over");
    }

    public bool IsAITurn()
    {
        return aiControlsPlayer2 && currentPlayer == 2;
    }

    public int GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public int GetTurnCount()
    {
        return turnCount;
    }

    public bool IsShotInProgress()
    {
        return isShotInProgress;
    }
}
