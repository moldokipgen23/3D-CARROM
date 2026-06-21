using UnityEngine;
using System;

public class PocketTrigger : MonoBehaviour
{
    public event Action<CoinType, GameObject> OnCoinPocketed;

    [Header("Pocket Settings")]
    public float pocketRadius = 0.05f;
    public LayerMask coinLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & coinLayer) != 0)
        {
            var coin = other.GetComponent<Coin>();
            if (coin != null && !coin.IsPocketed)
            {
                Debug.Log($"Coin {coin.Type} pocketed in pocket at {transform.position}");
                OnCoinPocketed?.Invoke(coin.Type, other.gameObject);
            }
        }
    }
}