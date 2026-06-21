using UnityEngine;

public class AIDifficultyTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("AIDifficulty test starting...");

        AIDifficulty[] difficulties = (AIDifficulty[])System.Enum.GetValues(typeof(AIDifficulty));
        Debug.Log("Available AI difficulties:");

        foreach (AIDifficulty difficulty in difficulties)
        {
            AINoiseParameters noiseParams = AIDifficultySettings.GetNoiseParameters(difficulty);
            Debug.Log($"  {difficulty}: Angle Noise={noiseParams.angleNoise}°, Power Noise={noiseParams.powerNoisePercent}%, Candidate Coins={noiseParams.candidateTargetCoins}");
        }

        Debug.Log("AIDifficulty test setup complete");
    }
}