using UnityEngine;

public class BoardTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Board test starting...");

        GameObject board = GameObject.Find("Board");
        if (board == null)
        {
            Debug.LogError("Board not found in scene");
            return;
        }

        BoardController boardController = board.GetComponent<BoardController>();
        if (boardController == null)
        {
            Debug.LogError("BoardController not found on Board");
            return;
        }

        Debug.Log("BoardController found on Board");
        Debug.Log($"Board dimensions: {boardController.boardWidth}x{boardController.boardHeight}");
        Debug.Log($"Border width: {boardController.borderWidth}");

        PocketTrigger[] pockets = boardController.pockets;
        Debug.Log($"Found {pockets.Length} pockets");

        for (int i = 0; i < pockets.Length; i++)
        {
            if (pockets[i] != null)
            {
                Debug.Log($"Pocket {i}: radius={pockets[i].pocketRadius}, layer={pockets[i].coinLayer}");
            }
        }

        boardController.CoinPocketed += OnCoinPocketed;

        Debug.Log("Board test completed successfully");
    }

    private void OnCoinPocketed(CoinType coinType)
    {
        Debug.Log($"BoardTest: Coin {coinType} was pocketed");
    }
}