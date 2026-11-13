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
        TryHandleOverlap(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHandleOverlap(other);
    }

    private void TryHandleOverlap(Collider other)
    {
        if (other == null) return;

        if (other.CompareTag("Striker"))
        {
            TryPocketStriker(other);
            return;
        }

        Coin coin = other.GetComponent<Coin>();
        if (coin == null)
            coin = other.GetComponentInParent<Coin>();

        if (coin == null || coin.IsPocketed) return;

        // The trigger sphere is wider than the pocket mouth. Requiring the coin
        // centre to be genuinely over the hole stops coins that merely brush the
        // pocket rim from being swallowed.
        Vector3 delta = other.transform.position - transform.position;
        delta.y = 0f;
        if (delta.magnitude > pocketRadius * 0.92f) return;

        coin.Pocket();

        // Foul tracking + scoring both happen exactly once here.
        FoulDetector.Instance?.RegisterPocketedCoin(coin);
        ScoreManager.Instance?.PocketCoin(coin);

        OnCoinPocketed?.Invoke(coin.Type, coin.gameObject);
    }

    private void TryPocketStriker(Collider other)
    {
        Vector3 delta = other.transform.position - transform.position;
        delta.y = 0f;

        // 78% of the pocket radius: the striker may hang on the rim without
        // being pocketed, which is both fairer and more realistic.
        if (delta.magnitude > pocketRadius * 0.78f) return;

        FoulDetector.Instance?.RegisterStrikerPocketed();
        int player = TurnManager.Instance != null ? TurnManager.Instance.GetCurrentPlayer() : 1;
        StrikerController.Instance?.ResetToBaseline(player);
    }
}
