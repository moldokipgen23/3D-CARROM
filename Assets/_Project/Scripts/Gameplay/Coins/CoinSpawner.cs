using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Coin Prefab References")]
    public GameObject whiteCoinPrefab;
    public GameObject blackCoinPrefab;
    public GameObject queenCoinPrefab;

    [Header("Board Settings")]
    public float boardWidth = 2.9f;
    public float boardHeight = 2.9f;
    public float centerCircleDiameter = 0.16f;

    [Header("Coin Settings")]
    public int whiteCoinCount = 9;
    public int blackCoinCount = 9;
    public int queenCoinCount = 1;

    [Header("Spawn Positions")]
    public Vector3[] spawnPositions = new Vector3[19];

    private void Start()
    {
        GenerateSpawnPositions();
        SpawnCoins();
    }

    private void GenerateSpawnPositions()
    {
        float halfWidth = boardWidth / 2f;
        float halfHeight = boardHeight / 2f;
        float centerCircleRadius = centerCircleDiameter / 2f;

        int positionIndex = 0;

        for (int row = 0; row < 5; row++)
        {
            for (int col = -2; col <= 2; col++)
            {
                if (positionIndex >= 19) break;

                float x = col * 0.15f;
                float z = row * 0.15f;

                if (row == 2 && col == 0)
                {
                    positionIndex++;
                    continue;
                }

                spawnPositions[positionIndex] = new Vector3(x, 0.01f, z);
                positionIndex++;
            }
        }

        Debug.Log($"Generated {positionIndex} spawn positions");
    }

    public void SpawnCoins()
    {
        int positionIndex = 0;

        for (int i = 0; i < whiteCoinCount; i++)
        {
            if (positionIndex >= spawnPositions.Length) break;

            GameObject coin = Instantiate(whiteCoinPrefab, spawnPositions[positionIndex], Quaternion.identity);
            coin.GetComponent<Coin>().Type = CoinType.White;
            coin.name = $"WhiteCoin_{i}";
            positionIndex++;
        }

        for (int i = 0; i < blackCoinCount; i++)
        {
            if (positionIndex >= spawnPositions.Length) break;

            GameObject coin = Instantiate(blackCoinPrefab, spawnPositions[positionIndex], Quaternion.identity);
            coin.GetComponent<Coin>().Type = CoinType.Black;
            coin.name = $"BlackCoin_{i}";
            positionIndex++;
        }

        if (queenCoinCount > 0 && positionIndex < spawnPositions.Length)
        {
            GameObject queen = Instantiate(queenCoinPrefab, spawnPositions[positionIndex], Quaternion.identity);
            queen.GetComponent<Coin>().Type = CoinType.Queen;
            queen.name = "QueenCoin";
        }

        Debug.Log($"Spawned {whiteCoinCount} white, {blackCoinCount} black, and {queenCoinCount} queen coins");
    }

    public void ResetCoins()
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        foreach (GameObject coin in coins)
        {
            coin.GetComponent<Coin>().Reset();
        }
        Debug.Log("All coins reset");
    }
}