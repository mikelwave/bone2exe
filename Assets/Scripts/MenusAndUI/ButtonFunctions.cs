using UnityEngine;

[RequireComponent (typeof(Menu))]
public class ButtonFunctions : MonoBehaviour
{
    Menu main;
    
    [SerializeField]
    Sprite[] messageBoxImages;

    // Special event when exiting the level without reaching the normal exit
    public delegate void LevelSelectEvent();
    public static LevelSelectEvent levelSelectEvent;
    void Awake()
    {
        main = GetComponent<Menu>();
    }
    public void RestartLevelConfirm()
    {
        main.ConfirmCheck(messageBoxImages[1],"<uppercase>Restart level?</uppercase>\n\n\n\n\n<size=44.6>(Life will be lost)",Restart);
    }
    public void LevelSelectConfirm()
    {
        main.ConfirmCheck(messageBoxImages[0],"<uppercase>Return to level select?</uppercase>\n\n\n\n\n<size=44.6>(Level progress will be lost)",LevelSelect);
    }
    public void QuitGameConfirm()
    {
        main.ConfirmCheck(messageBoxImages[0],"<uppercase>Really quit?</uppercase>\n\n\n\n\n<size=44.6>(Unsaved progress will be lost)",Quit);
    }
    public void Options()
    {
        // Spawn options canvas
        Instantiate(main.optionsMenu);
    }
    public void Statistics()
    {
        // Spawn stats canvas
        Instantiate(TitleScreen.titleMain.StatisticsMenu);
    }

    public void LevelSelect()
    {
        // Destroy pause menu
        levelSelectEvent?.Invoke();
        PauseMenu.allowPause = false;
        GameMaster.Reset();
        DataShare.LoadSceneWithTransition("LevelSelect");
    }
    public void DieFiveTimes()
    {
        // wtf
        PlayerControl.Death(5);
    }
    void Restart()
    {
        PlayerControl.Death(1);
    }
    void Quit()
    {
        Debug.Log("GAME QUIT");
        Application.Quit();
    }
}
