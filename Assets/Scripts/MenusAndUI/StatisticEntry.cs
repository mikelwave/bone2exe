using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatisticEntry : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] texts = new TextMeshProUGUI[4];
    [SerializeField] Image specialImage;

    void SetLevelName(string s)
    {
        texts[0].text = s;
    }
    void SetDeaths(string s)
    {
        texts[1].text = s;
    }
    void SetKills(string s)
    {
        texts[2].text = s;
    }
    void SetTime(string s)
    {
        texts[3].text = s;
    }
    public LevelStats[] stats;
    int worldID, levelID;

    public void SetLevelStats(LevelStats[] stats, int worldID, int levelID)
    {
        this.worldID = worldID;
        this.levelID = levelID;
        this.stats = stats;
        stats = null;
        worldID = 0;
        levelID = 0;
    }
    public void UpdateLevelStats(byte statsID)
    {
        SetLevelName(DataShare.GetLevelName(worldID,levelID));

        // Stats
        SetDeaths(stats[statsID].Deaths.ToString("00"));
        SetKills(stats[statsID].EnemyKills.ToString()+'%');
        if(!stats[statsID].SpecialItem && stats[(int)Mathf.Repeat(statsID+1,stats.Length)].SpecialItem)
        HalfTransparentSpecialImage();
        else ToggleSpecialImage(stats[statsID].SpecialItem);

        float time = stats[statsID].LevelTime;

        string formattedTime = "SUCKS";
        if(time<=600)
        {
            formattedTime = string.Format("{0:#0}:{1:00}.{2:00}",
            Mathf.Floor(time / 60), // Minutes
            Mathf.Floor(time) % 60, // Seconds
            Mathf.Floor((time * 100) % 100)); // Miliseconds
        }
        SetTime(time == 0 ? "-:--.--" : formattedTime);
    }
    public void ToggleSpecialImage(bool toggle)
    {
        if(toggle) specialImage.color = Color.white;
        specialImage.gameObject.SetActive(toggle);
    }
    public void HalfTransparentSpecialImage()
    {
        specialImage.color = new Color(1,1,1,0.5f);
        specialImage.gameObject.SetActive(true);
    }
    public void MakeBlank()
    {
        texts[0].text = string.Empty;
        texts[1].text = string.Empty;
        texts[2].text = string.Empty;
        texts[3].text = string.Empty;
        ToggleSpecialImage(false);
    }
    public void MakeUnknown()
    {
        texts[0].text = "<align=center>???";
        texts[1].text = string.Empty;
        texts[2].text = string.Empty;
        texts[3].text = "-:--.--";
        ToggleSpecialImage(false);
    }
}