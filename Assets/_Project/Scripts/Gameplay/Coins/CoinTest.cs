using UnityEngine;

public class CoinTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Coin test starting...");

        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        Debug.Log($"Found {coins.Length} coins");

        int whiteCount = 0;
        int blackCount = 0;
        int queenCount = 0;

        foreach (GameObject coin in coins)
        {
            Coin coinComponent = coin.GetComponent<Coin>();
            if (coinComponent != null)
            {
                Debug.Log($"Coin: Type={coinComponent.Type}, IsPocketed={coinComponent.IsPocketed}, Mass={coinComponent.Rigidbody.mass}");

                switch (coinComponent.Type)
                {
                    case CoinType.White:
                        whiteCount++;
                        break;
                    case CoinType.Black:
                        blackCount++;
                        break;
                    case CoinType.Queen:
                        queenCount++;
                        break;
                }
            }
        }

        Debug.Log($"Coin counts - White: {whiteCount}, Black: {blackCount}, Queen: {queenCount}");

        if (whiteCount == 9 && blackCount == 9 && queenCount == 1)
        {
            Debug.Log("Coin test passed: Correct number of coins spawned");
        }
        else
        {
            Debug.LogError($"Coin test failed: Expected 9 white, 9 black, 1 queen, but got {whiteCount}, {blackCount}, {queenCount}");
        }
    }
}