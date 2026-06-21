using UnityEngine;
using System;

public class BoardController : MonoBehaviour
{
    [Header("Board Settings")]
    public float boardWidth = 2.9f;
    public float boardHeight = 2.9f;
    public float borderWidth = 0.15f;

    [Header("Pocket References")]
    public PocketTrigger[] pockets = new PocketTrigger[4];

    public event Action<CoinType> CoinPocketed;

    private void Awake()
    {
        SetupPockets();
    }

    private void SetupPockets()
    {
        Transform[] pocketTransforms = new Transform[4];
        for (int i = 0; i < 4; i++)
        {
            pocketTransforms[i] = transform.Find($"Pocket{i}");
        }

        for (int i = 0; i < 4; i++)
        {
            if (pocketTransforms[i] != null)
            {
                pockets[i] = pocketTransforms[i].GetComponent<PocketTrigger>();
                if (pockets[i] != null)
                {
                    pockets[i].OnCoinPocketed += HandleCoinPocketed;
                }
            }
        }
    }

    private void HandleCoinPocketed(CoinType coinType, GameObject coin)
    {
        Debug.Log($"BoardController: Coin {coinType} pocketed");
        CoinPocketed?.Invoke(coinType);
    }

    public void SetPocketLayer(LayerMask layer)
    {
        foreach (var pocket in pockets)
        {
            if (pocket != null)
            {
                pocket.coinLayer = layer;
            }
        }
    }
}