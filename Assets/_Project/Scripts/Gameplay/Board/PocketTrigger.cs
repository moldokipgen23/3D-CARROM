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

        if (other.CompareTag("Striker"))
        {
            TryPocketStriker(other);
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

    /// <summary>
    /// Keep checking while the striker overlaps the trigger. The trigger sphere
    /// is much wider than the hole, so a striker brushing the pocket mouth used
    /// to be swallowed instantly. A foul now only counts when its centre is
    /// genuinely over the hole, like a real striker dropping in.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (other != null && other.CompareTag("Striker"))
            TryPocketStriker(other);
    }

    private void TryPocketStriker(Collider other)
    {
        Vector3 delta = other.transform.position - transform.position;
        delta.y = 0f;

        // 80% of the pocket radius: the striker may hang on the rim without
        // being pocketed, which is both fairer and more realistic.
        if (delta.magnitude > pocketRadius * 0.80f) return;

        FoulDetector.Instance?.RegisterStrikerPocketed();
        int player = TurnManager.Instance != null ? TurnManager.Instance.GetCurrentPlayer() : 1;
        StrikerController.Instance?.ResetToBaseline(player);
    }
}
