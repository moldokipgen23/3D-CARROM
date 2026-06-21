using UnityEngine;

public static class RankTierCalculator
{
    private static readonly RankTier[] tiers = new RankTier[]
    {
        new RankTier("Bronze", 0, 99),
        new RankTier("Silver", 100, 299),
        new RankTier("Gold", 300, 599),
        new RankTier("Platinum", 600, 999),
        new RankTier("Diamond", 1000, 1499),
        new RankTier("Master", 1500, 1999),
        new RankTier("GrandMaster", 2000, 2499),
        new RankTier("Legend", 2500, int.MaxValue)
    };

    public static string GetTier(int rating)
    {
        foreach (RankTier tier in tiers)
        {
            if (rating >= tier.minRating && rating <= tier.maxRating)
            {
                return tier.name;
            }
        }
        return "Bronze";
    }

    public static int GetTierIndex(int rating)
    {
        for (int i = 0; i < tiers.Length; i++)
        {
            if (rating >= tiers[i].minRating && rating <= tiers[i].maxRating)
            {
                return i;
            }
        }
        return 0;
    }

    public static int GetMinRatingForTier(string tierName)
    {
        foreach (RankTier tier in tiers)
        {
            if (tier.name == tierName)
            {
                return tier.minRating;
            }
        }
        return 0;
    }

    public static int GetMaxRatingForTier(string tierName)
    {
        foreach (RankTier tier in tiers)
        {
            if (tier.name == tierName)
            {
                return tier.maxRating;
            }
        }
        return 99;
    }

    public static float GetProgressToNextTier(int rating)
    {
        string currentTier = GetTier(rating);
        int minRating = GetMinRatingForTier(currentTier);
        int maxRating = GetMaxRatingForTier(currentTier);

        if (maxRating == int.MaxValue) return 1f;

        int range = maxRating - minRating;
        int progress = rating - minRating;

        return (float)progress / range;
    }
}

public struct RankTier
{
    public string name;
    public int minRating;
    public int maxRating;

    public RankTier(string name, int minRating, int maxRating)
    {
        this.name = name;
        this.minRating = minRating;
        this.maxRating = maxRating;
    }
}
