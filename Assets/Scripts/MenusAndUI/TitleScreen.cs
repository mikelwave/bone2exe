using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class TitleScreen : Menu
{
    public static TitleScreen titleMain;
    public GameObject StatisticsMenu;
    static bool skipIntro = false;

    // Camera movement
    [Range (0,1)]
    public float sine = 0;
    [Range (-2,2)]
    public float speed = 4;
    [Range (0.1f,7)]
    public float sineAmp = 5;
    bool idleCamera = false;
    Transform camTransform;
    Vector3 rootCamPos;
    PlayableDirector director;
    IEnumerator IPlayIntro()
    {
        DataShare.LoadMusic("PreMenu",false);
        menu.SetActive(false);

        // Wait until intro cutscene finishes
        float endTime = (float)director.duration;
        float elapsedTime = 0;
        director.Play();
        while(elapsedTime<endTime)
        {
            elapsedTime+=Time.unscaledDeltaTime;
            yield return 0;
            if(MGInput.GetButton(MGInput.controls.Player.Jump) && !skipIntro)
            {
                skipIntro = true;
                ScreenEffects.FadeScreen(2f,true,Color.white);
                yield return 0;
                yield return new WaitUntil(()=>ScreenEffects.fadeCor == null);
                director.Stop();
                director.initialTime = director.duration;
                director.Play();
                endTime = Time.timeSinceLevelLoad;
                yield return 0;
            }
        }
        skipIntro = true;
        director.Stop();
        director.transform.GetChild(4).GetComponent<SpriteRenderer>().enabled = true;
        ScreenEffects.FadeScreen(3f,false,Color.white);
        ShowMenu();
    }
    public void PlayGame()
    {
        //Load level select
        this.enabled = false;
        DataShare.LoadSceneWithTransition("LevelSelect");
    }

    public void ShowMenu()
    {
        self.OpenMenu();
        idleCamera = true;
    }
    void MoveCamera()
    {
        sine = Mathf.Repeat(sine+Time.deltaTime*speed,1);
        
        float PIsine = sine * Mathf.PI * 2;

        Vector2 pos;
        pos.x = rootCamPos.x;
        pos.y = rootCamPos.y + Mathf.Cos(PIsine) * sineAmp + sineAmp/2;

        camTransform.position = pos;
    }
    protected override void Awake()
    {
        base.Awake();
        self = this;
        titleMain = this;
        director = GameObject.Find("Background").GetComponent<PlayableDirector>();
        
        // Assign buttons
        camTransform = GameObject.FindWithTag("MainCamera").transform;
        rootCamPos = camTransform.position;
        rootCamPos.y+=sineAmp/2;
        if(!skipIntro)
        StartCoroutine(IPlayIntro());
        else
        {
            LoadMenuMusic();
            ShowMenu();
        }
    }
    public void LoadMenuMusic()
    {
        DataShare.LoadMusic("Menu",true);
    }
    protected override void Update()
    {
        if(idleCamera)
        {
            if(MGInput.GetButtonDown(MGInput.controls.Player.Jump))
            {
                selection.OnClick();
            }
            MoveCamera();
        }
    }
}
