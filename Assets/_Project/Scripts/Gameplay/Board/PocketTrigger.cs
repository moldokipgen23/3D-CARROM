using UnityEngine;
using System;

public class PocketTrigger : MonoBehaviour
{
    public event Action<CoinType, GameObject> OnCoinPocketed;

    [Header("Pocket Settings")]
    public float pocketRadius = 0.05f;
    public LayerMask coinLayer;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // Striker in a pocket = foul.  Put it back on the active player's baseline.
        if (other.CompareTag("Striker"))
        {
            FoulDetector.Instance?.RegisterStrikerPocketed();
            int player = TurnManager.Instance != null ? TurnManager.Instance.GetCurrentPlayer() : 1;
            StrikerController.Instance?.ResetToBaseline(player);
            return;
        }

        Coin coin = other.GetComponent<Coin>();
        if (coin == null)
            coin = other.GetComponentInParent<Coin>();

        if (coin == null || coin.IsPocketed) return;

        coin.Pocket();

        // Foul tracking + scoring both happen exactly once here.
        FoulDetector.Instance?.RegisterPocketedCoin(coin);
        ScoreManager.Instance?.PocketCoin(coin);

        OnCoinPocketed?.Invoke(coin.Type, coin.gameObject);
    }
}
