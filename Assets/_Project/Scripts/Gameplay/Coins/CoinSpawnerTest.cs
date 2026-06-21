using UnityEngine;

public class CoinSpawnerTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("CoinSpawner test starting...");

        CoinSpawner spawner = GetComponent<CoinSpawner>();
        if (spawner == null)
        {
            Debug.LogError("CoinSpawner not found on this GameObject");
            return;
        }

        Debug.Log("CoinSpawner found");
        Debug.Log($"Board size: {spawner.boardSize}");
        Debug.Log($"Expected coins - White: {spawner.whiteCoinCount}, Black: {spawner.blackCoinCount}");

        Debug.Log($"Total expected positions: {spawner.spawnPositions.Length}");
        for (int i = 0; i < spawner.spawnPositions.Length; i++)
        {
            Debug.Log($"Position {i}: {spawner.spawnPositions[i]}");
        }

        Debug.Log("CoinSpawner test setup complete");
    }
}