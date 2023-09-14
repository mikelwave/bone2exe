using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : Menu
{
    Menu main;
    [SerializeField] OptionsGraphics optionsGraphics;
    bool dirButtonUp = true;
    [SerializeField] Selectable[] optionsElements;
    [SerializeField] Scrollbar scrollbar;
    [SerializeField] Vector2 scrollBarHeights = new Vector2(400,100);
    float boxHeight;
    RectTransform rectTransform;
    [SerializeField] MenuArrows[] midiElements;

    string[] soundfontFiles; // Untrimmed locations of soundfonts

    float contentHeight = 0;
    const float appearSpeed = 2.5f;
    Camera cam;
    int dirY = 0;


    IEnumerator ITurnOff()
    {
        Vector2 sizeDelta = rectTransform.sizeDelta;
        float progress = 0;
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime*appearSpeed;
            sizeDelta.y = Mathf.Lerp(boxHeight,0,Mathf.SmoothStep(0,1,progress));
            rectTransform.sizeDelta = sizeDelta;
            yield return 0;
        }
        Menu.self = main;
        ToggleButtons(true);
        main.OverwriteBoilAnimationUI();
        main.RestoreControl();
        DataShare.SaveSettingsToFile();
        Destroy(gameObject);
    }
    IEnumerator ITurnOn()
    {
        Vector2 sizeDelta = rectTransform.sizeDelta;
        sizeDelta.y = 0;
        rectTransform.sizeDelta = sizeDelta;
        WireNavigation();

        float progress = 0;
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime*appearSpeed;
            sizeDelta.y = Mathf.Lerp(0,boxHeight,Mathf.SmoothStep(0,1,progress));
            rectTransform.sizeDelta = sizeDelta;
            yield return 0;
        }
        sizeDelta.y = boxHeight;
        rectTransform.sizeDelta = sizeDelta;
        OpenMenu();
    }
    void WireNavigation()
    {
        Selectable lastAvailable = optionsElements[0];
        bool reAssign = false;
        for (int i = 1; i < optionsElements.Length; i++)
        {
            if(optionsElements[i].interactable)
            {
                if(reAssign)
                {
                    var nav = optionsElements[i].navigation;
                    nav.selectOnUp = lastAvailable;
                    optionsElements[i].navigation = nav;

                    nav = lastAvailable.navigation;
                    nav.selectOnDown = optionsElements[i];
                    lastAvailable.navigation = nav;
                }
                lastAvailable = optionsElements[i];
            }
            // Non-interactable
            else
            {
                reAssign = true;
                var nav = optionsElements[i].navigation;
                nav.mode = Navigation.Mode.None;
                optionsElements[i].navigation = nav;
            }
        }
    }
    void ScrollBarAutoScroll(Vector3 lastSelectionPos)
    {
        if(dirY == 0) return;

        int divider = (cam.pixelHeight/DataShare.NativeResolution.y);
        lastSelectionPos /= divider;

        float toAdd = 0;
        float distance = 0;
        if(lastSelectionPos.y>scrollBarHeights.x)
        {
            distance = Mathf.Abs(lastSelectionPos.y-scrollBarHeights.x);
            //print("Difference: "+distance);
            toAdd = distance/contentHeight;
        }
        else if(lastSelectionPos.y<scrollBarHeights.y)
        {
            distance = Mathf.Abs(lastSelectionPos.y-scrollBarHeights.y);
            //print("Difference: "+distance);
            toAdd = -(distance/contentHeight);
        }

        scrollbar.value = Mathf.Clamp(scrollbar.value+toAdd,0,1);
    }
    void OnEnable()
    {
        cam = GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetComponent<Camera>();
        main = Menu.self;
        rectTransform = transform.GetChild(0).GetComponent<RectTransform>();
        boxHeight = rectTransform.sizeDelta.y;
        if(OptionsGraphics.self == null) optionsGraphics.Init();
        Checksoundfonts();
        if(scrollbar != null)
        {
            contentHeight = scrollbar.transform.parent.GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
            ///print("Scrollbar size: " + contentHeight);
            selectionCallback = ScrollBarAutoScroll;
        }

        // Turn off main menu buttons
        ToggleButtons(false);
        Menu.self = this;
        StartCoroutine(ITurnOn());
        
    }
    protected override void UpdateControl()
    {
        base.UpdateControl();

        int dir = MGInput.GetDpadXRaw(MGInput.controls.Player.Movement);
        dirY = MGInput.GetDpadYRaw(MGInput.controls.Player.Movement);
        if(dirY==0 && scrollbar != null)
        {
            int scroll = (int)Mathf.Clamp(MGInput.GetDpad(MGInput.controls.UI.MouseScroll).y,-1,1);
            if(scroll!=0)
            {
                scrollbar.value = Mathf.Clamp(scrollbar.value+(float)scroll/4,0,1);
            }

        }

        if(dirButtonUp && dir!=0)
        {
            dirButtonUp = false;
            dirPressCallback?.Invoke(dir);
        }
        else if(!dirButtonUp && dir == 0)
        {
            dirButtonUp = true;
        }
    }
    void Checksoundfonts()
    {
        if (Directory.Exists(Application.streamingAssetsPath))
        {
            string[] files = Directory.GetFiles(Application.streamingAssetsPath, "*.sf2", SearchOption.AllDirectories);
            soundfontFiles = new string[files.Length];
            files.CopyTo(soundfontFiles,0);
            for (int i = 0; i < files.Length; i++)
            {
                soundfontFiles[i] = soundfontFiles[i].Substring(Application.streamingAssetsPath.Length);
            }
            // Trim file names
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Substring(files[i].LastIndexOf('\\')+1);
                files[i] = files[i].Remove(files[i].LastIndexOf('.'));
            }

            /*print("Soundfonts present: "+ files.Length);
            foreach (string item in files)
            {
                print(item);
            }*/

            if(files.Length == 1)
            {
                // Disable selection if one soundfont present
                GameSettings.SetSettingInt("Soundfont",0);
                midiElements[1].SetInteractable(false);
                if(GameSettings.self.Soundfont.Get()!=0)
                {
                    DataShare.synthesizer.SetSoundfontPath(soundfontFiles[0]);
                    GameSettings.self.Soundfont.Set(0);
                    if(DataShare.trueMidiMusic) DataShare.ResetMidiTrack();
                }
            }
            if(files.Length>=1)
            {
                // Create a list of soundfonts
                midiElements[1].OptionsText = files;
                if(!DataShare.synthesizer.Exists())
                {
                    int value = Mathf.Clamp(GameSettings.self.Soundfont.Get(),0,soundfontFiles.Length-1);
                    DataShare.synthesizer.SetSoundfontPath(soundfontFiles[value]);
                    GameSettings.self.Soundfont.Set(value);
                    if(DataShare.trueMidiMusic) DataShare.ResetMidiTrack();
                }
            }
            else if(files.Length == 0)
            {
                // Force recorded
                GameSettings.SetSettingInt("MusicType",0);
                if(DataShare.trueMidiMusic) DataShare.SwitchMusicMode(false);
                midiElements[0].SetInteractable(false);
                midiElements[1].SetInteractable(false);
                midiElements[1].OptionsText = new string[]{"None"};
            }

        }
    }


    // Music slider
    public void MusicSlider(float value)
    {
        if(!GameSettings.self.MusicToggle.Get()) value = 0;
        DataShare.SetMusicVolume(value);
    }
    // Music toggle
    public void MusicToggle()
    {
        bool value = GameSettings.self.MusicToggle.Get();
        if(value)
        {
            DataShare.SetMusicVolume(GameSettings.self.MusicVolume.Get());
        }
        else DataShare.SetMusicVolume(0);
    }
    // SFX slider
    public void SFXSlider(float value)
    {
        if(!GameSettings.self.SFXToggle.Get()) value = 0;
        DataShare.SetSFXVolume(value);
    }
    // SFX toggle
    public void SFXToggle()
    {
        bool value = GameSettings.self.SFXToggle.Get();
        if(value)
        {
            DataShare.SetSFXVolume(GameSettings.self.SFXVolume.Get());
        }
        else DataShare.SetSFXVolume(0);
    }

    // Music type (only appear if soundfonts exist in the game files)
    public void MusicType()
    {
        int value = GameSettings.self.MusicType.Get();
        DataShare.SwitchMusicMode(value == 0 ? false : true);
    }
    // Soundfont select (only appear if more than 1 soundfont exists)
    public void Soundfont()
    {
        int value = GameSettings.self.Soundfont.Get();
        DataShare.synthesizer.SetSoundfontPath(soundfontFiles[value]);
        if(DataShare.trueMidiMusic) DataShare.ResetMidiTrack();
    }
    // Resolution arrows
    public void Resolution()
    {
        int value = GameSettings.self.Resolution.Get();
        DataShare.SetResolution(value);
    }
    // Fullscreen toggle
    public void Fullscreen()
    {
        bool value = GameSettings.self.Fullscreen.Get();
        Screen.fullScreen = value;
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }
    // Diagonal aim toggle
    public void DiagonalAim()
    {
        int value = GameSettings.self.DiagonalAim.Get();
        DataShare.aimControlHold = value == 0 ? false : true;
    }
    // Moving backgrounds toggle
    public void MovingBackgrounds()
    {
        bool value = GameSettings.self.MovingBG.Get();
        DataShare.movingBGs = value;
        DataShare.onMovingBGsSwitch?.Invoke();
    }
    // Screenshake toggle
    public void Screenshake()
    {
        bool value = GameSettings.self.Screenshake.Get();
        DataShare.screenshake = value;
    }

    // Pixel scaling toggle
    public void PixelScaling()
    {
        bool value = GameSettings.self.PixelScaling.Get();
        DataShare.pixelScaling = value;
        DataShare.TogglePixelPerfectScaling(value);
    }

    // VSync toggle
    public void VSync()
    {
        int value = GameSettings.self.VSync.Get();
        QualitySettings.vSyncCount = value;
    }

    // Back button
    public void Back()
    {
        ///print("Back");
        LocalToggleButtons(false);
        StartCoroutine(ITurnOff());
    }
    // Apply
    public void Apply()
    {
        ///print("Apply");
        applyCallback?.Invoke();
        applyCallback = null;
    }
}
