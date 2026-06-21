using UnityEngine;

public class FoulDetector : MonoBehaviour
{
    [Header("Foul Detection Settings")]
    public float foulTimeout = 30f;
    public LayerMask coinLayer;
    
    [Header("References")]
    public TurnManager turnManager;
    public ScoreManager scoreManager;
    
    private bool strikerPocketed = false;
    private bool anyCoinPocketed = false;
    private bool queenPocketed = false;
    private float lastShotTime;
    private GameObject striker;
    
    private void Start()
    {
        InitializeReferences();
        FindStriker();
    }
    
    private void InitializeReferences()
    {
        if (turnManager == null)
        {
            turnManager = FindObjectOfType<TurnManager>();
        }
        if (scoreManager == null)
        {
            scoreManager = FindObjectOfType<ScoreManager>();
        }
    }
    
    private void FindStriker()
    {
        GameObject[] strikers = GameObject.FindGameObjectsWithTag("Striker");
        if (strikers.Length > 0)
        {
            striker = strikers[0];
        }
        else
        {
            Debug.LogWarning("No striker found for foul detection");
        }
    }
    
    private void Update()
    {
        DetectFouls();
    }
    
    private void DetectFouls()
    {
        ResetFoulFlags();
        
        CheckStrikerPocketed();
        CheckAnyCoinPocketed();
        CheckQueenPocketed();
        CheckNoCoinTouched();
        
        ApplyFoulsIfDetected();
    }
    
    private void ResetFoulFlags()
    {
        strikerPocketed = false;
        anyCoinPocketed = false;
        queenPocketed = false;
    }
    
    private void CheckStrikerPocketed()
    {
        if (striker != null)
        {
            PocketTrigger[] pockets = FindObjectsOfType<PocketTrigger>();
            foreach (PocketTrigger pocket in pockets)
            {
                if (IsObjectInPocket(striker, pocket))
                {
                    strikerPocketed = true;
                    Debug.LogWarning("Foul: Striker pocketed");
                    break;
                }
            }
        }
    }
    
    private void CheckAnyCoinPocketed()
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        foreach (GameObject coin in coins)
        {
            Coin coinComponent = coin.GetComponent<Coin>();
            if (coinComponent != null && !coinComponent.IsPocketed)
            {
                PocketTrigger[] pockets = FindObjectsOfType<PocketTrigger>();
                foreach (PocketTrigger pocket in pockets)
                {
                    if (IsObjectInPocket(coin, pocket))
                    {
                        anyCoinPocketed = true;
                        if (coinComponent.Type == CoinType.Queen)
                        {
                            queenPocketed = true;
                        }
                        Debug.Log($"Coin {coinComponent.Type} pocketed");
                        break;
                    }
                }
            }
        }
    }
    
    private void CheckNoCoinTouched()
    {
        float timeSinceLastShot = Time.time - lastShotTime;
        if (timeSinceLastShot > foulTimeout && !anyCoinPocketed)
        {
            Debug.LogWarning("Foul: No coin touched or pocketed within timeout");
        }
    }
    
    private bool IsObjectInPocket(GameObject obj, PocketTrigger pocket)
    {
        Collider objCollider = obj.GetComponent<Collider>();
        if (objCollider == null) return false;
        
        float distance = Vector3.Distance(obj.transform.position, pocket.transform.position);
        return distance <= pocket.pocketRadius;
    }
    
    private void ApplyFoulsIfDetected()
    {
        int currentPlayer = turnManager?.GetCurrentPlayer() ?? 1;
        
        if (strikerPocketed)
        {
            scoreManager?.ApplyFoulPenalty(currentPlayer);
            turnManager?.EndShot();
        }
        else if (!anyCoinPocketed && Time.time - lastShotTime > foulTimeout)
        {
            scoreManager?.ApplyFoulPenalty(currentPlayer);
            turnManager?.EndShot();
        }
        else if (queenPocketed && !HasQueenCover(currentPlayer))
        {
            scoreManager?.ApplyFoulPenalty(currentPlayer);
            turnManager?.EndShot();
        }
    }
    
    private bool HasQueenCover(int player)
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        foreach (GameObject coin in coins)
        {
            Coin coinComponent = coin.GetComponent<Coin>();
            if (coinComponent != null && coinComponent.Type == CoinType.Queen && !coinComponent.IsPocketed)
            {
                return true;
            }
        }
        return false;
    }
    
    public void RegisterShot()
    {
        lastShotTime = Time.time;
    }
}