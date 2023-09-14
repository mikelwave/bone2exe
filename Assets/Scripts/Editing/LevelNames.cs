using UnityEngine;

[System.Serializable]
class ConstantLevelData
{
    [SerializeField]
    [Tooltip ("Name of the scene to load.")]
    public string levelScene = "";

    [SerializeField]
    [Tooltip ("Level name to display (leave blank for no level text intro).")]
    public string levelName = "";
}
[CreateAssetMenu(fileName = "ConstantLevelData", menuName = "ScriptableObjects/LevelNames", order = 1)]
public class LevelNames : ScriptableObject
{
    [SerializeField]
    ConstantLevelData[] constantLevelDatas = new ConstantLevelData[DataShare.levelAmount];

    public string GetLevelName(int index)
    {
        try
        {
            return constantLevelDatas[index].levelName;
        }
        catch (System.IndexOutOfRangeException)
        {
            return string.Empty;
            throw;
        }
    }
    public string GetLevelScene(int index)
    {
        try
        {
            return constantLevelDatas[index].levelScene;
        }
        catch (System.IndexOutOfRangeException)
        {
            return "MainMenu";
            throw;
        }
    }
}