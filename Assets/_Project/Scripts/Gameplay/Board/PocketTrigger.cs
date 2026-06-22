using UnityEngine;
using System;

public class PocketTrigger : MonoBehaviour
{
    public event Action<CoinType, GameObject> OnCoinPocketed;

    [Header("Pocket Settings")]
    public float pocketRadius = 0.05f;
    public LayerMask coinLayer;

    private ScoreManager scoreManager;
    private TurnManager turnManager;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        if (coinLayer == 0)
            coinLayer = 1 << 0;

        scoreManager = ScoreManager.Instance;
        turnManager = FindObjectOfType<TurnManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & coinLayer) != 0)
        {
            var coin = other.GetComponent<Coin>();
            if (coin != null && !coin.IsPocketed)
            {
                Debug.Log($"Coin {coin.Type} pocketed in pocket at {transform.position}");
                coin.Pocket();
                OnCoinPocketed?.Invoke(coin.Type, other.gameObject);

                int currentPlayer = turnManager?.GetCurrentPlayer() ?? 1;
                scoreManager?.AddCoins(currentPlayer, coin.Type, 1);
            }
        }
    }
}