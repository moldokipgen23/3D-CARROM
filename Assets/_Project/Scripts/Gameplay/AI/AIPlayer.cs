using UnityEngine;
using System.Collections;

public class AIPlayer : MonoBehaviour
{
    [Header("AI Settings")]
    public AIDifficulty difficulty = AIDifficulty.Medium;
    public float thinkingDelay = 1.0f; // 0.5-1.5s thinking delay
    
    [Header("References")]
    public StrikerController strikerController;
    public TurnManager turnManager;
    public ScoreManager scoreManager;
    
    private Vector2 targetShot;
    private float targetPower;
    private bool isThinking = false;
    private bool isReadyToShoot = false;
    
    private void Start()
    {
        InitializeReferences();
    }
    
    private void InitializeReferences()
    {
        if (strikerController == null)
        {
            strikerController = GetComponent<StrikerController>();
        }
        if (turnManager == null)
        {
            turnManager = FindObjectOfType<TurnManager>();
        }
        if (scoreManager == null)
        {
            scoreManager = FindObjectOfType<ScoreManager>();
        }
        
        if (strikerController != null)
        {
            strikerController.OnShotFired += OnShotCompleted;
        }
    }
    
    private void Update()
    {
        if (turnManager != null && turnManager.GetCurrentPlayer() == 2 && isReadyToShoot && !isThinking)
        {
            StartCoroutine(AITurn());
        }
    }
    
    private IEnumerator AITurn()
    {
        isThinking = true;
        isReadyToShoot = false;
        
        Debug.Log($"AI Player thinking... (Difficulty: {difficulty})");
        
        yield return new WaitForSeconds(thinkingDelay);
        
        CalculateAndExecuteShot();
        isThinking = false;
    }
    
    private void CalculateAndExecuteShot()
    {
        AINoiseParameters noiseParams = AIDifficultySettings.GetNoiseParameters(difficulty);
        
        Vector2 shot = CalculateBestShot(noiseParams);
        
        if (shot != Vector2.zero)
        {
            ExecuteShot(shot.x, shot.y);
        }
        else
        {
            Debug.LogWarning("AI could not find a valid shot");
            turnManager?.EndShot();
        }
    }
    
    private Vector2 CalculateBestShot(AINoiseParameters noiseParams)
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        Vector3 strikerPosition = strikerController.transform.position;
        
        Vector3 bestShot = Vector3.zero;
        float bestScore = float.MaxValue;
        
        int coinsEvaluated = 0;
        int maxCoinsToEvaluate = noiseParams.candidateTargetCoins;
        
        foreach (GameObject coin in coins)
        {
            Coin coinComponent = coin.GetComponent<Coin>();
            if (coinComponent == null || coinComponent.IsPocketed)
            {
                continue;
            }
            
            coinsEvaluated++;
            if (coinsEvaluated > maxCoinsToEvaluate)
            {
                break;
            }
            
            Vector2 shot = CalculateShotToPocket(coinComponent.Type, coin.transform.position, noiseParams);
            
            if (shot != Vector2.zero)
            {
                float score = CalculateShotScore(shot, coin.transform.position);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestShot = shot;
                }
            }
        }
        
        if (bestShot == Vector2.zero)
        {
            Debug.LogWarning("AI found no valid shots");
        }
        
        return bestShot;
    }
    
    private Vector2 CalculateShotToPocket(CoinType coinType, Vector3 coinPosition, AINoiseParameters noiseParams)
    {
        PocketTrigger[] pockets = FindObjectsOfType<PocketTrigger>();
        Vector3 strikerPosition = strikerController.transform.position;
        
        Vector3 bestShot = Vector3.zero;
        float bestScore = float.MaxValue;
        
        foreach (PocketTrigger pocket in pockets)
        {
            Vector3 pocketPosition = pocket.transform.position;
            
            Vector2 shot = CalculateDirectShot(strikerPosition, coinPosition, pocketPosition);
            if (shot == Vector2.zero)
            {
                continue;
            }
            
            shot = ApplyNoise(shot, noiseParams);
            
            float score = CalculateShotScore(shot, coinPosition);
            if (score < bestScore)
            {
                bestScore = score;
                bestShot = shot;
            }
        }
        
        return bestShot;
    }
    
    private Vector2 CalculateDirectShot(Vector3 strikerPos, Vector3 targetPos, Vector3 pocketPos)
    {
        Vector3 toTarget = targetPos - strikerPos;
        Vector3 toPocket = pocketPos - strikerPos;
        
        float angleToTarget = Mathf.Atan2(toTarget.z, toTarget.x) * Mathf.Rad2Deg;
        float angleToPocket = Mathf.Atan2(toPocket.z, toPocket.x) * Mathf.Rad2Deg;
        
        float distanceToTarget = toTarget.magnitude;
        float distanceToPocket = toPocket.magnitude;
        
        float idealPower = Mathf.Clamp(distanceToTarget * 1.5f, 2f, 15f);
        
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(angleToTarget, angleToPocket));
        if (angleDifference > 90f)
        {
            return Vector2.zero;
        }
        
        return new Vector2(angleToTarget, idealPower);
    }
    
    private Vector2 ApplyNoise(Vector2 shot, AINoiseParameters noiseParams)
    {
        float angleNoise = UnityEngine.Random.Range(-noiseParams.angleNoise, noiseParams.angleNoise);
        float powerNoise = UnityEngine.Random.Range(-noiseParams.powerNoisePercent, noiseParams.powerNoisePercent);
        
        float noisyAngle = shot.x + angleNoise;
        float noisyPower = shot.y * (1f + powerNoise / 100f);
        
        noisyPower = Mathf.Clamp(noisyPower, 2f, 15f);
        
        return new Vector2(noisyAngle, noisyPower);
    }
    
    private float CalculateShotScore(Vector2 shot, Vector3 targetPosition)
    {
        float angleScore = 0f;
        float powerScore = 0f;
        
        Vector3 strikerPosition = strikerController.transform.position;
        Vector3 shotDirection = new Vector3(Mathf.Sin(shot.x * Mathf.Deg2Rad), 0, Mathf.Cos(shot.x * Mathf.Deg2Rad));
        
        Vector3 predictedTargetPosition = strikerPosition + shotDirection * shot.y;
        Vector3 toTarget = targetPosition - predictedTargetPosition;
        float distanceToTarget = toTarget.magnitude;
        
        angleScore = distanceToTarget;
        powerScore = Mathf.Abs(shot.y - 8f);
        
        return angleScore + powerScore * 0.5f;
    }
    
    private void ExecuteShot(float angle, float power)
    {
        Vector3 worldDirection = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
        
        Debug.Log($"AI executing shot - Angle: {angle:F1}°, Power: {power:F1}");
        
        ApplyForceToStriker(worldDirection, power);
        isReadyToShoot = true;
    }
    
    private void ApplyForceToStriker(Vector3 direction, float power)
    {
        if (strikerController != null && strikerController.Rigidbody != null)
        {
            Vector3 force = direction * power;
            strikerController.Rigidbody.AddForce(force, ForceMode.Impulse);
        }
    }
    
    private void OnShotCompleted(Vector2 direction, float power)
    {
        Debug.Log($"AI shot completed - Direction: {direction}, Power: {power:F1}");
        turnManager?.EndShot();
        isReadyToShoot = false;
    }
    
    public void SetDifficulty(AIDifficulty newDifficulty)
    {
        difficulty = newDifficulty;
        Debug.Log($"AI difficulty set to: {difficulty}");
    }
}