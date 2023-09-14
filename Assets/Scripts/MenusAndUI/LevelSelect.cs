using System.Collections;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

// Clicking levels
class LevelIconCollider : MonoBehaviour, IPointerDownHandler
{
    delegate void ClickEvent();
    ClickEvent clickEvent;
    public void Init(LevelSelect main)
    {
        clickEvent = main.LoadLevel;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        clickEvent?.Invoke();
    }
}

// Displaying levels
class LevelDisplay
{
    public Animator anim;
    TextMeshProUGUI levelDisplay,levelDeaths,levelTime,levelKills;
    RectTransform worldSwitchPrompt;
    GameObject specialIcon;
    LevelSelect main;
    public void Init(LevelSelect main)
    {
        this.main = main;
        Transform mainTr;
        anim = GameObject.Find("MenuOverlay").GetComponent<Animator>();

        mainTr = anim.transform.GetChild(1);
        levelDisplay = mainTr.GetChild(0).GetComponent<TextMeshProUGUI>();
        ///Debug.Log("Checking level display: "+(levelDisplay==null?"ERR":"OK"));
        levelDeaths = mainTr.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>();
        ///Debug.Log("Checking level deaths: "+(levelDeaths==null?"ERR":"OK"));
        levelKills = mainTr.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
        ///Debug.Log("Checking level kills: "+(levelKills==null?"ERR":"OK"));
        levelTime = mainTr.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        ///Debug.Log("Checking level time: "+(levelTime==null?"ERR":"OK"));
        specialIcon = mainTr.GetChild(5).gameObject;
        ///Debug.Log("Special presence: "+(specialIcon==null?"ERR":"OK"));

        worldSwitchPrompt = anim.transform.GetChild(3).GetComponent<RectTransform>();
        worldSwitchPrompt.gameObject.SetActive(main.canSwitchWorldType);
    }
    public void WorldSwitchEffect()
    {
        main.StartCoroutine(IWorldSwitchAnimation());
    }
    IEnumerator IWorldSwitchAnimation()
    {
        float progress = 0;
        Vector3 startScale = worldSwitchPrompt.localScale;
        Vector3 endScale = Vector3.one * 1.1f;
        while(progress<1)
        {
            progress += Time.deltaTime * 4;
            worldSwitchPrompt.localScale = Vector3.Lerp(startScale,endScale,Mathf.Sin(progress * Mathf.PI));
            yield return 0;
        }
    }
    Coroutine switchStatsCor;
    IEnumerator ISwitchStats(LevelStats[] stats, int worldID, int levelID)
    {
        int statsID = 0;
        while (true)
        {
            SetDisplayData(stats,worldID, levelID, statsID);
            statsID = (int)Mathf.Repeat(++statsID,stats.Length);
            yield return new WaitForSeconds(1f);
        }
    }
    public void DisplayData(int worldID, int levelID)
    {
        if(switchStatsCor != null) main.StopCoroutine(switchStatsCor);

        LevelStats[] stats = DataShare.GetLevelStats(worldID,levelID);

        // Check which stats to display, 0 = any%, 1 = 100%
        byte whichPresent = (byte)((stats[0].LevelTime != 0 ? 1 : 0) + (stats[1].LevelTime != 0 ? 2 : 0 ));
        if(whichPresent == 0) whichPresent = 1;
        int statsID = 0;
        if(whichPresent == 3) // Both present, display on a cycle
        {
            switchStatsCor = main.StartCoroutine(ISwitchStats(stats,worldID,levelID));
            return;
        }
        else statsID = whichPresent - 1; // Any or All present only
        SetDisplayData(stats,worldID, levelID, statsID);
    }
    void SetDisplayData(LevelStats[] stats,int worldID, int levelID, int statsID)
    {
        levelDisplay.text = DataShare.GetLevelName(worldID,levelID);

        // Stats
        levelDeaths.text = stats[statsID].Deaths.ToString("00");
        levelKills.text = stats[statsID].EnemyKills.ToString()+'%';
        float time = stats[statsID].LevelTime;
        specialIcon.SetActive(stats[statsID].SpecialItem);

        string formattedTime = "SUCKS";

        if(time<=600)
        {
            formattedTime = string.Format("{0:#0}:{1:00}.{2:00}",
            Mathf.Floor(time / 60), //Minutes
            Mathf.Floor(time) % 60, //Seconds
            Mathf.Floor((time * 100) % 100)); //Miliseconds
        }

        levelTime.text = time == 0 ? "-:--.--" : formattedTime;
    }
}
public class LevelSelect : MonoBehaviour
{
    Camera cam;
    Vector3 lastPos = Vector3.zero;
    public Vector2 RotationLimits = new Vector2(32.5f,-32.5f);
    Transform Arrow;
    Transform Chamber,Background;
    PlayableDirector director;
    bool lockMenuSwitch = false;
    public TimelineAsset[] timelineAssets = new TimelineAsset[2];
    public LevelSelectButton[] levelSelectButtons = new LevelSelectButton[2];
    public Image[] levelIcons;
    [SerializeField] Sprite[] levelSprites;
    Image worldMapDisplay;
    LevelDisplay levelDisplay = new LevelDisplay();
    Volume volume;

    // World type switching
    [SerializeField] Sprite[] worldMapSprites = new Sprite[2];
    public bool canSwitchWorldType = false;
    bool currentWorldTypeLight = true;

    // 0 - World Select, 1 = level select
    public static int menuType = 0;
    //0-3
    int worldSelect = 0;
    int WorldSelect { get { return worldSelect + (!currentWorldTypeLight ? DataShare.worldAmount : 0); } }
    int worldAmount = 4;
    int levelAmount = 11;
    int maxAllowedLevel = 10;
    const int turnSpeed = 4;

    // Keyboard level controls
    bool mouseSelectMode = false;
    const float keyRepeatDelay = 0.35f;
    const float keyRepeatRate = 0.075f;
    const float mouseLockDelay = 0.5f; 
    int savedDir = 0;
    bool allowKeyboard = true;
    bool allowKeyPress = true;
    // 0-10
    int levelSelect = 0;

    bool lockUpdateFunction = false;

    // Animation for turning the gun chamber
    Coroutine chamberTurn;
    IEnumerator IChamberTurn(int dir,bool playAnimation)
    {
        lockUpdateFunction = true;
        if(playAnimation)
        {
            
            PlayDirector(0);
            levelDisplay.anim.SetBool("Stats",false);
            yield return 0;
            yield return new WaitUntil(()=>!lockMenuSwitch);
        }
        float TargetAngle = 45 + 90*worldSelect;
        float StartAngle = Chamber.localEulerAngles.z;
        if(dir == 1 && StartAngle>TargetAngle)
        {
            TargetAngle+=360;
        }
        else if(dir == -1 && TargetAngle>StartAngle)
        {
            StartAngle+=360;
        }
        float progress = 0;

        // Change level values
        if(menuType == 1)
        UpdateLevels();

        // Change arrow position for new level set
        if(playAnimation)
        {
            ///print("Saved dir:" +savedDir);
            SetLevelSelect(savedDir == -1 ? 10 : 0,false);
        }

        DataShare.PlaySound("LS_Turn",false,0.1f,1);
        while(progress<1)
        {
            progress += Time.deltaTime*turnSpeed;
            float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
            Chamber.localEulerAngles = new Vector3(0,0,Mathf.Lerp(StartAngle,TargetAngle,mathStep));
            Background.localEulerAngles = Chamber.localEulerAngles;
            yield return 0;
        }
        if(playAnimation)
        {
            PlayDirector(1);
            levelDisplay.DisplayData(WorldSelect,levelSelect);

            levelDisplay.anim.SetBool("Stats",true);

            yield return 0;
            yield return new WaitUntil(()=>!lockMenuSwitch);
        }
        CancelLoopKeyPress();
        chamberTurn = null;
        lockUpdateFunction = false;
    }
    Coroutine switchWorldTypeCor;
    IEnumerator ISwitchWorldType()
    {
        lockUpdateFunction = true;
        currentWorldTypeLight = !currentWorldTypeLight;
        bool playAnimation = menuType == 1;
        levelDisplay.WorldSwitchEffect();
        CancelInvoke("LoopKeyPress");

        // Cosmetic
        if(playAnimation)
        {
            PlayDirector(0);
            levelDisplay.anim.SetBool("Stats",false);
            yield return 0;
            yield return new WaitUntil(()=>!lockMenuSwitch);
        }

        float progress = 0;
        float StartAngle = Chamber.localEulerAngles.z;
        float TargetAngle = StartAngle + 360;
        bool switched = false;

        DataShare.PlaySound("LS_SwitchWorld",false,0.1f,1);
        while(progress<1)
        {
            progress += Time.deltaTime*turnSpeed/2;

            volume.weight = Mathf.Clamp(currentWorldTypeLight ? (1 - progress) : progress, 0, 1);

            float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
            Chamber.localEulerAngles = new Vector3(0,0,Mathf.Lerp(StartAngle,TargetAngle,mathStep));
            Background.localEulerAngles = Chamber.localEulerAngles;

            if(progress > 0.5f && !switched)
            {
                switched = true;
                worldMapDisplay.sprite = worldMapSprites[currentWorldTypeLight ? 0 : 1];
            }
            yield return 0;
        }
        Chamber.localEulerAngles = Vector3.forward * StartAngle;
        Background.localEulerAngles = Chamber.localEulerAngles;

        if(playAnimation)
        {
            PlayDirector(1);
            levelDisplay.DisplayData(WorldSelect,levelSelect);

            levelDisplay.anim.SetBool("Stats",true);

            yield return 0;
            yield return new WaitUntil(()=>!lockMenuSwitch);
        }

        yield return 0;
        switchWorldTypeCor = null;
        lockUpdateFunction = false;
    }
    void PlayDirector(int animID)
    {
        DataShare.PlaySound(animID == 0 ? "LS_ZoomOut" : "LS_ZoomIn",false,0.1f,1);
        director.playableAsset = timelineAssets[animID];
        director.initialTime = 0;
        director.Play();
        lockMenuSwitch = true;
    }
    void EndAnim(PlayableDirector aDirector)
    {
        lockMenuSwitch = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetComponent<Camera>();

        CamControl.cameraTransform = cam.transform;
        director = GetComponent<PlayableDirector>();
        director.stopped += EndAnim;
        director.playableAsset = timelineAssets[menuType];
        director.initialTime = director.duration;
        director.Play();

        volume = transform.GetChild(1).GetComponent<Volume>();
        volume.weight = 0;
        
        Arrow = transform.GetChild(0).GetChild(1).GetChild(1).transform;
        Chamber = transform.GetChild(0).GetChild(1).GetChild(2).transform;

        transform.GetChild(0).GetChild(1).GetChild(3).gameObject.AddComponent<LevelIconCollider>().Init(this);

        Background = transform.GetChild(0).GetChild(1).GetChild(0).transform;

        print("Last loaded level: " + DataShare.lastLoadedLevel);

        if(DataShare.lastLoadedLevel.x>DataShare.worldAmount*2)
        {
            worldSelect = 0;
            levelSelect = 0;
        }
        else
        {
            worldSelect = (int)Mathf.Repeat(DataShare.lastLoadedLevel.x,DataShare.worldAmount);
            levelSelect = Mathf.Clamp(DataShare.lastLoadedLevel.y,0,maxAllowedLevel);
        }

        Chamber.localEulerAngles = new Vector3(0,0,45 + 90*worldSelect);
        Background.localEulerAngles = Chamber.localEulerAngles;
        Background.GetChild(0).localEulerAngles = Vector3.forward*-45;
        lastPos = MGInput.GetDpad(MGInput.controls.UI.MousePos);

        // Reset run counter if exiting a level and previous run time doesn't exist
        DataShare.InvalidateRunTimer();

        // If returning from stage
        PauseMenu.allowPause = true;

        // Check which worlds to reveal
        // In dark worlds all are visible since it appears only when all levels were beaten
        Transform holder = Background.GetChild(0);
        
        for (int i = 1; i < DataShare.worldAmount; i++)
        {
            holder.GetChild(i-1).gameObject.SetActive(!DataShare.GetWorldUnlock(i));
        }

        // World type switching
        worldMapDisplay = holder.GetComponent<Image>();
        canSwitchWorldType = DataShare.GetAllLightWorldsUnlocked();

        if(GameMaster.LastWorldMode == GameMaster.WorldMode.DarkMode && PlayerControl.currentLives < 5)
        {
            // Load as dark world
            currentWorldTypeLight = false;
            worldMapDisplay.sprite = worldMapSprites[1];
                
        }

        // Disable arrows if only one world unlocked
        if(!DataShare.GetMultipleWorldsUnlocked())
        {
            levelSelectButtons[0].gameObject.SetActive(false);
            levelSelectButtons[1].gameObject.SetActive(false);
        }
        levelDisplay.Init(this);
        levelDisplay.anim.SetBool("Chapt",menuType == 1 ? false : true);

        // Prepare level set
        if(menuType == 1) UpdateLevels();

        SetLevelSelect(levelSelect,false);

        Transition.RePositionCamera(cam,(Vector3)(Vector2)Transition.offsetVector+new Vector3(0,menuType == 0 ? 11.2f : 0,menuType == 0 ? -50 : 0));
        
        Destroy(GameObject.Find("PauseMenu"));
    }
    void OnDisable()
    {
        director.stopped -= EndAnim;
        allowKeyPress = true;
        CancelLoopKeyPress();
    }
    void UpdateLevels()
    {
        int[] array = DataShare.GetLevelValues(worldSelect);
        levelAmount = DataShare.levelAmount;
        worldAmount = DataShare.worldAmount;

        bool locked = false;
        maxAllowedLevel = levelIcons.Length;
        for(int i = 0;i<levelIcons.Length;i++)
        {
            if(array[i] != 0) locked = false;
            
            if(locked)
            {
                levelIcons[i].sprite = levelSprites[1]; // fill with locks
            }
            else
            {
                switch (array[i])
                {
                    default: // blank value
                    levelIcons[i].sprite = levelSprites[0];
                    locked = true;
                    break;

                    case 1: // normal clear
                    levelIcons[i].sprite = levelSprites[i<levelIcons.Length-1 ? 2 : 4]; // different sprite for boss level clear
                    break;

                    case 2: // special clear
                    levelIcons[i].sprite = levelSprites[3];
                    break;
                }
                maxAllowedLevel = i;
            }
        }
        levelSelect = Mathf.Clamp(levelSelect,0,maxAllowedLevel);
    }
    void GoTitle()
    {
        // Load level
        if(director.state != PlayState.Playing && MGInput.GetButtonDown(MGInput.controls.Player.Shoot))
        {
            this.enabled = false;
            DataShare.LoadSceneWithTransition("TitleScreen");
        }
    }
    void EnterLevel()
    {
        if(MGInput.GetButtonDown(MGInput.controls.Player.Jump))
        {
            LoadLevel();
        }
    }
    public void LoadLevel()
    {
        if(!this.enabled || menuType != 1) return;

        // Check lives range
        if(DataShare.GetAllLightWorldsUnlocked())
        {
            int multiplier = (currentWorldTypeLight ? 1 : -1);
            if(PlayerControl.currentLives * multiplier <= 0) PlayerControl.currentLives = 5 * multiplier;
        }

        // Load level
        if(director.state != PlayState.Playing)
        {
            if(worldSelect + levelSelect == 2) // Level 1
            {
                DataShare.StartRunTimer();
            }
            DataShare.PlaySound("LS_Enter",false,0.25f,1);
            this.enabled = false;
            DataShare.LoadLevel(WorldSelect,levelSelect,true);
        }
    }

    int SetLevelSelect(int value, bool sound)
    {
        if(value>maxAllowedLevel)
        {
            return 1; // clamped, dont go to next world
        }

        levelSelect = Mathf.Clamp(value,0,Mathf.Clamp(maxAllowedLevel,0,levelAmount-1));
        ///print("Level select: "+levelSelect);
        Arrow.localEulerAngles = new Vector3(0,0,Mathf.Lerp(RotationLimits.x,RotationLimits.y,(float)value/(levelAmount-1)));
        if(menuType == 1 && levelSelect == value)
        {
            levelDisplay.anim.Play("LS_Stats_In",1,0);
            levelDisplay.DisplayData(WorldSelect,levelSelect);

            levelDisplay.anim.SetBool("Stats",true);
            
        }
        else if(DataShare.GetMultipleWorldsUnlocked() && allowKeyPress)
        {
            levelDisplay.anim.SetBool("Stats",false);
        }

        value = levelSelect == value ? 0 : 1;

        if(value == 0 && sound) DataShare.PlaySound("LS_LevelArrow",Arrow.position,false);  

        return value; // Returns 1 if value had to be clamped
    }
    int GetNearestWorld(int dir)
    {
        int val = 0;
        int count = 0;
        do
        {
            val = (int)Mathf.Repeat(worldSelect+=dir,worldAmount);
            count++;
        }
        while(!DataShare.GetWorldUnlock(val) && count <= worldAmount);
        return val;
    }

    public void WorldSwitchKeyboard(int dir)
    {
        if(!allowKeyboard || chamberTurn!=null) return;
        WorldSwitch(dir);
        levelSelectButtons[(dir+1)/2].FakeOnClick();
    }
    public void WorldSwitch(int dir)
    {
        if(chamberTurn!=null) return;
        int oldSelect = worldSelect;
        worldSelect = GetNearestWorld(dir);
        if(oldSelect == worldSelect) return;

        print("Selected world: "+worldSelect);
        chamberTurn = StartCoroutine(IChamberTurn(dir,menuType == 1));
    }
    public void MenuToggle()
    {
        menuType = menuType == 0 ? 1 : 0;
        levelDisplay.anim.SetBool("Chapt",menuType == 1 ? false : true);
        if(menuType==1)
        {
            UpdateLevels();
            SetLevelSelect(levelSelect,false);
        }
        else
        {
            levelDisplay.anim.SetBool("Stats",false);
        }
        PlayDirector(menuType);
        CancelLoopKeyPress();
    }

    void MouseTracking(Vector3 pos)
    {
        if(lockMenuSwitch) return; // Lock arrow movements if in the middle of an animation
        pos.z = cam.nearClipPlane;
        pos = cam.ScreenToWorldPoint(pos);
        if(pos.y<-0.05f) return;
        pos.y = 0;
        pos.x = Mathf.Clamp(pos.x,-0.2f,0.2f);

        // Calculate the pointing position of the arrow
        float progress = Mathf.Abs(0.2f+pos.x)/0.2f/2;
        float rounded = Mathf.FloorToInt(progress*(levelAmount-1)+((progress > 0.5f) ? 0.7f : 0.8f));
        if((int)rounded == levelSelect) return;
        SetLevelSelect((int)rounded,true);
    }

    void LoopKeyPress()
    {
        if(chamberTurn!=null) return; // Lock arrow movements if in the middle of an animation
        int result = SetLevelSelect((int)levelSelect+savedDir,true);
        if(result == 1) // Stop looping if value has to be clamped
        {
            ///print("Cancel loop clamp");
            WorldSwitchKeyboard(savedDir);
        }
    }
    void CancelLoopKeyPress()
    {
        allowKeyPress = true;
        ///savedDir = 0;
        CancelInvoke("LoopKeyPress");
        ///print("Cancel loop");
    }
    void EnableKeyboard()
    {
        // Disable mouse mode
        allowKeyboard = true;
        ///print("Unlock keyboard");
    }

    void LevelSelectNavigation()
    {
        Vector3 mousePos = MGInput.GetDpad(MGInput.controls.UI.MousePos);
        
        if(!mouseSelectMode) mouseSelectMode = mousePos!=lastPos; // Enable on mouse movement

        if(mouseSelectMode)
        {   
            if(mousePos!=lastPos) // Mouse controls
            {
                if(!allowKeyPress) CancelLoopKeyPress();
                CancelInvoke("EnableKeyboard");

                MouseTracking(mousePos);
                lastPos = mousePos;
            }
            else // Timer to get out of mouse select mode
            {
                mouseSelectMode = false;
                allowKeyboard = false;
                Invoke("EnableKeyboard",mouseLockDelay);
            }
        }
        else if(allowKeyboard) // Keyboard controls
        {
            int dir = Mathf.RoundToInt(MGInput.GetDpadX(MGInput.controls.Player.Movement));
            if(dir != 0 && allowKeyPress)
            {
                allowKeyPress = false;
                savedDir = dir;

                if(SetLevelSelect((int)levelSelect+dir,true) == 1)
                {
                    WorldSwitchKeyboard(savedDir);
                }
                ///print("Loop");
                InvokeRepeating("LoopKeyPress",keyRepeatDelay,keyRepeatRate);
            }
            else if(dir != savedDir && !allowKeyPress)
            {
                CancelLoopKeyPress();
            }
        }
    }
    void WorldSelectNavigation()
    {
        int dir = Mathf.RoundToInt(MGInput.GetDpadX(MGInput.controls.Player.Movement));
        if(dir!=0) WorldSwitchKeyboard(dir);
    }
    void MenuSwitch()
    {
        // Jump to advance to world select, shoot to go back to level select
        if(MGInput.GetButtonDown(menuType == 0 ? MGInput.controls.Player.Jump : MGInput.controls.Player.Shoot))
        {
            MenuToggle();
        }
    }
    
    void SwitchWorldType()
    {
        if(!MGInput.GetButtonDown(MGInput.controls.Player.ExtendAim)) return;

        if(switchWorldTypeCor != null) return;

        switchWorldTypeCor = StartCoroutine(ISwitchWorldType());
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(lockUpdateFunction) return;
        if(canSwitchWorldType)
        {
            SwitchWorldType();
        }
        if(!lockMenuSwitch)
        {
            MenuSwitch();
        }
        if(menuType == 0)
        {
            GoTitle();
            WorldSelectNavigation();
        }
        else
        {
            EnterLevel();
            LevelSelectNavigation();
        }
    }
}
