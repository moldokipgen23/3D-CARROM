using System;

[Serializable]
public class PlayerData
{
    public string PlayerId;
    public string Username;
    public int Coins;
    public int Diamonds;
    public int XP;
    public int Level;

    public PlayerData()
    {
        PlayerId = Guid.NewGuid().ToString();
        Username = "Player";
        Coins = 0;
        Diamonds = 0;
        XP = 0;
        Level = 1;
    }

    public PlayerData(string username)
    {
        PlayerId = Guid.NewGuid().ToString();
        Username = username;
        Coins = 0;
        Diamonds = 0;
        XP = 0;
        Level = 1;
    }
}