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

    private bool pocketsWired = false;

    private void Awake()
    {
        // BoardSetup builds the pockets during Start(), so this may find
        // nothing yet. BoardSetup calls SetupPockets() again afterwards.
        SetupPockets();
    }

    /// <summary>
    /// Finds every PocketTrigger under this board and subscribes to its event.
    /// Safe to call more than once.
    /// </summary>
    public void SetupPockets()
    {
        if (pocketsWired) return;

        PocketTrigger[] found = GetComponentsInChildren<PocketTrigger>(true);

        if (found.Length == 0)
            return;

        if (pockets == null || pockets.Length != 4)
            pockets = new PocketTrigger[4];

        for (int i = 0; i < pockets.Length; i++)
        {
            if (i < found.Length)
                pockets[i] = found[i];
        }

        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] != null)
                found[i].OnCoinPocketed += HandleCoinPocketed;
        }

        pocketsWired = true;
    }

    private void HandleCoinPocketed(CoinType coinType, GameObject coin)
    {
        CoinPocketed?.Invoke(coinType);
    }

    public void SetPocketLayer(LayerMask layer)
    {
        if (pockets == null) return;

        foreach (var pocket in pockets)
        {
            if (pocket != null)
                pocket.coinLayer = layer;
        }
    }
}
