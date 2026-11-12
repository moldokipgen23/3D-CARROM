using UnityEngine;

public enum AIDifficulty
{
    Easy,
    Medium,
    Hard,
    Expert,
    Master
}

public struct AINoiseParameters
{
    public float angleNoise; // degrees
    public float powerNoisePercent; // percentage
    public int candidateTargetCoins; // number of candidate target coins to evaluate

    public AINoiseParameters(float angleNoise, float powerNoisePercent, int candidateTargetCoins)
    {
        this.angleNoise = angleNoise;
        this.powerNoisePercent = powerNoisePercent;
        this.candidateTargetCoins = candidateTargetCoins;
    }
}

public static class AIDifficultySettings
{
    public static AINoiseParameters GetNoiseParameters(AIDifficulty difficulty)
    {
        switch (difficulty)
        {
            case AIDifficulty.Easy:
                return new AINoiseParameters(10f, 25f, 1);
            case AIDifficulty.Medium:
                return new AINoiseParameters(5f, 12f, 1);
            case AIDifficulty.Hard:
                return new AINoiseParameters(3f, 8f, 1);
            case AIDifficulty.Expert:
                return new AINoiseParameters(1.8f, 4f, 2);
            case AIDifficulty.Master:
                return new AINoiseParameters(0.8f, 2f, 3);
            default:
                return new AINoiseParameters(5f, 12f, 1);
        }
    }
}