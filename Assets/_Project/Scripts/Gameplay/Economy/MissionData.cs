public enum MissionType
{
    Daily,
    Weekly,
    Season
}

[System.Serializable]
public struct Mission
{
    public string id;
    public string type;
    public string description;
    public int target;
    public int progress;
    public int reward;
    public bool completed;
    public MissionType missionType;
}
