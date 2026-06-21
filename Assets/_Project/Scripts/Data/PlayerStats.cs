using UnityEngine;
using System;

[Serializable]
public class PlayerStats
{
    public string playerId;
    public int matchesPlayed;
    public int wins;
    public int losses;
    public int draws;
    public int bestStreak;
    public int currentStreak;
    public string highestRank;
    
    public PlayerStats()
    {
        playerId = System.Guid.NewGuid().ToString();
        matchesPlayed = 0;
        wins = 0;
        losses = 0;
        draws = 0;
        bestStreak = 0;
        currentStreak = 0;
        highestRank = "Bronze";
    }
    
    public void UpdateStats(bool win, bool draw)
    {
        matchesPlayed++;
        
        if (win)
        {
            wins++;
            currentStreak++;
            if (currentStreak > bestStreak)
            {
                bestStreak = currentStreak;
            }
        }
        else if (draw)
        {
            draws++;
            currentStreak = 0;
        }
        else
        {
            losses++;
            currentStreak = 0;
        }
        
        UpdateRank();
    }
    
    private void UpdateRank()
    {
        if (wins >= 50)
        {
            highestRank = "Legend";
        }
        else if (wins >= 30)
        {
            highestRank = "Master";
        }
        else if (wins >= 20)
        {
            highestRank = "GrandMaster";
        }
        else if (wins >= 10)
        {
            highestRank = "Diamond";
        }
        else if (wins >= 5)
        {
            highestRank = "Platinum";
        }
        else if (wins >= 2)
        {
            highestRank = "Gold";
        }
        else if (wins >= 1)
        {
            highestRank = "Silver";
        }
        else
        {
            highestRank = "Bronze";
        }
    }
}