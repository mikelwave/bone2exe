using UnityEngine;

// Object handling all levels in the world
[System.Serializable]
public class World
{
    [SerializeField]
    bool Unlocked = false;
    [SerializeField]
    LevelData[] levels = new LevelData[11]; //One world handles 11 levels
    
    public World()
    {
    }
    public bool GetUnlocked()
    {
        return Unlocked;
    }
    public void SetUnlocked(bool value)
    {
        Unlocked = value;
    }
    public LevelStats[] GetLevelStats(int ID)
    {
        try
        {
            return levels[ID].LevelStats;
        }
        catch (System.IndexOutOfRangeException)
        {
            Debug.LogError("Level "+ID+" out of range. Cannot display stats");
            throw;
        }
        
    }
    public bool SetLevelStats(LevelStats stats, int levelID)
    {
        // Determine stats type (0 - any%, 1 - 100%)
        byte type = (byte)((stats.EnemyKills == 100) ? 1 : 0);
        try
        {
            LevelStats curStats = levels[levelID].LevelStats[type];
            /* Always write a new record if time value in the level is 0
            In any% and 100%, time determines record
            if time matches, get higher kill %
            if time and kills matches, get lower death count */
            if((curStats.LevelTime == 0 || curStats.LevelTime > stats.LevelTime)
                ||(!curStats.SpecialItem && stats.SpecialItem)
                ||(curStats.LevelTime == stats.LevelTime &&
                    (curStats.EnemyKills < stats.EnemyKills || curStats.Deaths > stats.Deaths)))
            {
                // overwrite stats with new stats.
                levels[levelID].LevelStats[type] = stats;
                Debug.Log("Wrote new stats for level "+levelID+" stats type: "+(type == 0 ? ("any%") : ("100%")));
                return true;
            }
        }
        catch (System.IndexOutOfRangeException)
        {
            Debug.LogError("Level "+levelID+" does not exist. Error writing stats.");
            throw;
        }
        return false;
    }
    
    /*
    public string GetLevelSceneName(int ID)
    {
        //Get level scene string
        if(ID != Mathf.Clamp(ID,0,levels.Length-1) || levels[ID]==null) return "";
        else return  DataShare.self.levelNames[ID] levels[ID].GetSceneName();
    }
    public string GetLevelTitle(int ID)
    {
        //Get level title
        if(ID != Mathf.Clamp(ID,0,levels.Length-1) || levels[ID]==null) return "";
        else return levels[ID].LevelName;
    }*/
    public int GetCompletion(int ID)
    {
        //Get level completion
        if(ID == Mathf.Clamp(ID,0,levels.Length-1)) 
        return levels[ID].LevelValue;

        else return 0;
    }
    public void SetCompletion(int ID,byte completionMark)
    {
        try
        {
            byte val = levels[ID].LevelValue;
            levels[ID].LevelValue = (completionMark > val) ? completionMark : val;
            Debug.Log("Level "+ID+" mark set to: "+completionMark);
        }
        catch (System.IndexOutOfRangeException)
        {
            Debug.LogError("Level "+ID+" out of range. Failed to set completion mark.");
            throw;
        }
    }
    public int[] GetAllCompletions()
    {
        // Return an array of all level completions
        int[] array = new int[levels.Length];

        for(int i = 0;i<array.Length;i++)
            array[i] = levels[i].LevelValue;

        return array;
    }
    public bool HasACompletion()
    {
        // Return an array of all level completions
        int[] array = new int[levels.Length];

        for(int i = 0;i<array.Length;i++)
            if(levels[i].LevelValue != 0) return true;

        return false;
    }
    public int LevelCount()
    {
        return levels.Length;
    }
}

[System.Serializable]
// Single level object
class LevelData
{
    [SerializeField]
    [Range(0,2)]
    [Tooltip ("Level completion marker. (0 - Locked, 1 - Finished, 2 - Picked up special).")]
    byte levelCompletion = 0; // 0 - Locked, 1 - Finished, 2 - Picked up special

    [SerializeField]
    [Tooltip ("Level stats: 0 - any%, 1 - 100%.")]
    LevelStats[] levelStats = new LevelStats[2];

    public byte LevelValue {get {return levelCompletion;} set{levelCompletion = value;}}

    public LevelData()
    {
        levelCompletion = 0;
    }
    public LevelStats[] LevelStats {get {return levelStats;}}
}
[System.Serializable]
public class LevelStats
{
    [SerializeField]
    [Tooltip ("Level time in seconds.")]
    float levelTime = 0;

    [SerializeField]
    [Tooltip ("Minimal deaths on a level.")]
    byte deaths = 255;

    [SerializeField]
    [Tooltip ("Enemy kill percentage (%).")]
    [Range(0,100)]
    byte kills = 0;
    [SerializeField]
    [Tooltip ("Collected special item?")]
    bool special = false;

    public float LevelTime {get {return levelTime;} set{levelTime = value;}}
    public byte Deaths {get {return deaths;} set{deaths = value;}}
    public byte EnemyKills {get {return kills;} set{kills = value;}}
    public bool SpecialItem {get {return special;} set {special = value;}}
    public LevelStats()
    {
        levelTime = 0;
        deaths = 255;
        kills = 0;
        special = false;
    }
    public LevelStats(byte Deaths, byte Kills, float Time, bool Special)
    {
        deaths = Deaths;
        kills = Kills;
        levelTime = Time;
        special = Special;
    }
}
