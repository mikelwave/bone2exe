using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.IO;
using System.Linq;
using UnityEngine.Networking;
using FluidMidi;

[System.Serializable]
class AudioSourceTransform : MonoBehaviour {
    public AudioSource audioSource;
    public Transform mainParent;

    IEnumerator ITurnOff()
    {
        yield return 0;
        if(audioSource.clip != null || !GetIsLooping)
        {
            //If not looping, don't check for time pause
            if(!GetIsLooping)
            {
                yield return new WaitUntil(()=>!GetIsPlaying);
            }
            else
            {
                // Check if time is stopped.
                while(GetIsPlaying)
                {
                    if(Time.timeScale == 0)
                    {
                        audioSource.Pause();
                        yield return new WaitUntil(()=>Time.timeScale != 0);
                        audioSource.UnPause();
                    }
                    yield return 0;
                }
            }
        }
        gameObject.SetActive(false);
    }
    IEnumerator ISetBack()
    {
        yield return 0;
        if(audioSource == null)
        {
            Destroy(this);
            yield break;
        }
        if(audioSource.isPlaying) yield break;
        transform.SetParent(mainParent);
        if(gameObject.activeInHierarchy)
        {
            yield return 0;
            gameObject.SetActive(false);
        }
    }
    void OnDisable()
    {
        #if UNITY_EDITOR
        if(DataShare.self.gameObject.activeInHierarchy)
        #endif
        DataShare.self.StartCoroutine(ISetBack());
    }
    public void Init(Transform parent,AudioMixerGroup mixer)
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1;
        audioSource.dopplerLevel = 0;
        audioSource.outputAudioMixerGroup = mixer;
        mainParent = parent;
        transform.SetParent(mainParent);
        gameObject.SetActive(false);
    }
    public void Play (AudioClip clip, Vector3 position, bool loop, float volume, float pitch)
    {
        if(clip == null || !audioSource.enabled) return;
        transform.position = position;
        transform.name = clip.name;
        audioSource.spatialBlend = 1;
        gameObject.SetActive(true);
        audioSource.loop = loop;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        if(!loop && gameObject.activeInHierarchy) audioSource.PlayOneShot(clip);
        else
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        if(gameObject.activeInHierarchy)
        StartCoroutine(ITurnOff());
    }
    public void Play (AudioClip clip, bool loop, float volume, float pitch)
    {
        if(clip == null) return;
        audioSource.spatialBlend = 0;
        transform.name = clip.name;
        gameObject.SetActive(true);
        audioSource.loop = loop;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        if(!loop) audioSource.PlayOneShot(clip);
        else
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        StartCoroutine(ITurnOff());
    }
    public void PlayAndFollow(AudioClip clip, Transform source, bool loop, float volume, float pitch)
    {
        if(clip == null) return;
        audioSource.spatialBlend = 1;
        transform.name = clip.name;
        transform.SetParent(source);
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(true);
        audioSource.loop = loop;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        if(!loop) audioSource.PlayOneShot(clip);
        else
        {
            audioSource.clip = clip;
            if(!gameObject.activeInHierarchy) return;
            audioSource.Play();
        }
        StartCoroutine(ITurnOff());
    }

    public void Stop()
    {
        audioSource.loop = false;
        audioSource.Stop();
    }
    public bool GetIsLooping { get { return audioSource.loop;}}
    public bool GetIsPlaying { get {return audioSource.isPlaying;}}
    public bool GetActive { get {return gameObject.activeInHierarchy;}}
}
class PlayedClip {
    public Transform transform;
    public AudioClip audioClip;
    public float distance;
    public int index = -1;

    public PlayedClip(Transform transform,AudioClip clip, float distance, int index)
    {
        this.transform = transform;
        this.audioClip = clip;
        this.distance = distance;
        this.index = index;
    }
}
public class DataShare : MonoBehaviour
{
    #region vars
    //Chapters
    [SerializeField]
    World[] worlds = new World[worldAmount];
    [SerializeField] LevelNames[] levelNames = new LevelNames[worldAmount*2];
    public static DataShare self;
    public const int levelAmount = 11;
    public const int worldAmount = 4;
    public const float bulletLifeMinContact = 0.35f;
    public static Vector2Int NativeResolution = new Vector2Int(960,540);
    [SerializeField]
    MonoBehaviour[] ScriptOrder;
    [SerializeField] AudioMixerGroup SFXMixer;
    [SerializeField] AudioMixerGroup MusicMixer;
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] GameObject SaveCanvas;
    //[SerializeField] Object[] audioclips;
    [SerializeField] AudioClip[] audioclips;

    //Sounds
    AudioSourceTransform[] audioSourceTransforms;
    static List <PlayedClip> AudioclipStringsFrame;
    Transform audioHolder;

    AudioSource musicSource;

    // FluidMidi
    GameObject fluidMidi;
    SongPlayer songPlayer;
    public static Synthesizer synthesizer;

    public static float GameSpeed = 1.0f;

    public static bool autoSlide = true;
    public static Vector2Int lastLoadedLevel = new Vector2Int(0,0);

    public delegate void SoundsLoadedCallback();
    public static SoundsLoadedCallback soundsLoadedCallback;
    static string musicNowPlaying = "";

    delegate void MusicLoadType(string songName, bool loop = true);
    static MusicLoadType musicLoadType;

    // Statistics
    public static int totalDeaths = 0;
    public static double totalGameTime = 0;
    public static double newRunTotalGameTime = -1;
    public static bool hadRun = false;

    public const int specialsMax = 41;

    // Total run time functions
    public static void StartRunTimer()
    {  
        if(hadRun)
        {
            Debug.Log("Allowing run counter to start, had cheats before.");
            if(totalGameTime == -1 && CheatInput.CheatVal == 0) totalGameTime = 0;
            return;
        }
        else
        {
            Debug.Log("Allowing run counter to start, had previous run.");
            newRunTotalGameTime = 0;
        }
    }
    public static void FinishRunTimer()
    {
        if(!hadRun)
        {
            hadRun = true;
        }
        else
        {
            if(newRunTotalGameTime != -1 && newRunTotalGameTime < totalGameTime) totalGameTime = newRunTotalGameTime;
        }
        Debug.Log("Run finished successfully, new best run time: "+totalGameTime);
    }
    public static void IncreaseRunTimer(float toAdd)
    {
        if(CheatInput.CheatVal > 0 && totalGameTime != -1)
        {
            DataShare.InvalidateRunTimer();
            return;
        }

        if(!hadRun)
        totalGameTime += toAdd;
        else if(newRunTotalGameTime != -1) newRunTotalGameTime += toAdd;
        Debug.Log("Run so far: "+(hadRun?newRunTotalGameTime:totalGameTime));
    }
    public static void InvalidateRunTimer()
    {
        if(hadRun)
        {
            Debug.Log("Resetting run time counter");
            newRunTotalGameTime = -1;
        }
        else if(CheatInput.CheatVal > 0)
        {
            Debug.Log("Run does not count because of cheats");
            totalGameTime = -1;
        }
    }


    // Options
    #region settings
    public static bool trueMidiMusic = false;
    public static bool screenshake = true;
    public static bool pixelScaling = true;
    public static bool movingBGs = true;
    public static bool aimControlHold = false;

    public delegate void OnMovingBGsSwitch();
    public static OnMovingBGsSwitch onMovingBGsSwitch;

    public static void SetResolution(int resolutionIndex)
    {
        Vector2Int newResolution = new Vector2Int(960,540);
        switch(resolutionIndex)
        {
            default: break;
            case 1: newResolution = new Vector2Int(1280,720); break;
            case 2: newResolution = new Vector2Int(1600,900); break;
            case 3: newResolution = new Vector2Int(1920,1080); break;
            case 4: newResolution = new Vector2Int(2560,1440); break;
            case 5: newResolution = new Vector2Int(2880,1620); break;
            case 6: newResolution = new Vector2Int(3840,2160); break;
        }
        ///print("New resolution: "+newResolution);
        
        Screen.SetResolution(newResolution.x,newResolution.y,GameSettings.self.Fullscreen.Get());
    }

    public static void TogglePixelPerfectScaling(bool toggle)
    {
        UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera[] cameras = FindObjectsOfType<UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera>();
        ///print("Cams found: "+cameras.Length);
        foreach(var cam in cameras)
        {
            cam.enabled = toggle;
            if(!toggle)
            {
                Camera camera = cam.GetComponent<Camera>();
                camera.rect = new Rect(0,0,1,1);
                ///print(camera.gameObject.name+" "+camera.rect);
            }
        }
    }

    #endregion

    #endregion // vars

    #region Audio
    bool musicSystemLoaded = false;
    public static bool songLoaded = false;
    public const int audioFilesLength = 135;
    //Create and initialize music system
    void LoadMusicSystem()
    {
        musicNowPlaying = "";
        GameObject obj = new GameObject("MusicSystem");
        Transform objTr = obj.transform;
        objTr.SetParent(transform);
        objTr.localPosition = Vector3.zero;

        musicSource = obj.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.spatialBlend = 0;
        musicSource.dopplerLevel = 0;
        musicSource.outputAudioMixerGroup = MusicMixer;
        print("Music source loaded: "+(musicSource!=null));

        Transform fluidMidiTr = transform.GetChild(1);
        fluidMidi = fluidMidiTr.gameObject;
        synthesizer = fluidMidiTr.GetComponent<Synthesizer>();

        // Switch music loading type
        musicLoadType = trueMidiMusic ? LoadMusicMidi : LoadMusicRecorded;

        // Set synth soundfont
        InitializeSoundfont();
        musicSystemLoaded = true;
    }
    void InitializeSoundfont()
    {
        string[] files = Directory.GetFiles(Application.streamingAssetsPath, "*.sf2", SearchOption.AllDirectories);
        if(files.Length == 0)
        {
            Debug.Log("No soundfonts found on initialization");
            trueMidiMusic = false;
            SwitchMusicMode(false);
            return;
        }
        int val = Mathf.Clamp(GameSettings.self.Soundfont.Get(),0,files.Length-1);
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = files[i].Substring(Application.streamingAssetsPath.Length);
        }
        synthesizer.SetSoundfontPath(files[val]);
    }
    static Coroutine LoadSongCor;
    IEnumerator ILoadSong(string path, string songName, bool loop)
    {
        AudioClip song;
        print("Song path:\n"+path+songName);
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path+songName+".ogg", AudioType.OGGVORBIS))
        {
            yield return www.SendWebRequest();

            try
            {
                song = DownloadHandlerAudioClip.GetContent(www);
            }
            catch (System.InvalidOperationException)
            {
                Debug.Log("Song with name "+songName+" not found, aborting.");
                songLoaded = true;
                yield break;
            }
            song.name = songName;
        }
        if(musicSource.isPlaying)musicSource.Stop();

        musicSource.clip = song;
        musicSource.loop = loop;
        musicSource.Play();
        musicNowPlaying = musicSource.clip.name;
        songLoaded = true;
        if(loop) yield break;

        yield return 0;
        yield return new WaitUntil(()=>!musicSource.isPlaying);
        musicSource.clip = null;
        musicNowPlaying = "";
    }
    IEnumerator ILoadSongMidi(string path, string songName, bool loop, int tick = 0)
    {
        print("Song path (MIDI):\n"+path+songName);
        fluidMidi.SetActive(false);
        if(songPlayer != null)
        Destroy(songPlayer);
        musicNowPlaying = songName;

        SongPlayer.songPath = path;
        songPlayer = fluidMidi.AddComponent<SongPlayer>();


        // Volume
        float volume = 0;
        audioMixer.GetFloat("MusicVolume", out volume);
        songPlayer.Gain = volume <= -80 ? 0 : ((volume+20)*0.02f);

        // Looping
        ToggleInt loopInt = new ToggleInt(loop, 0);
        songPlayer.Loop = loopInt;

        // Assign synthesizer
        songPlayer.synthesizer = synthesizer;
        fluidMidi.SetActive(true);

        songPlayer.Play(tick);
        // Set start tick
        if(tick != 0)
        {
            yield return 0;
            yield return new WaitUntil(()=>songPlayer.IsReady);
            yield return 0;
            yield return 0;
            songPlayer.Seek(tick);
        }
        songLoaded = true;
        // Wait until music isn't playing if track is not looping
        if(loop) yield break;

        yield return 0;
        yield return new WaitUntil(()=>!songPlayer.IsPlaying);
        Destroy(songPlayer);
        musicNowPlaying = "";
    }
    IEnumerator IWaitUntilMusicSystemLoaded(string songName, bool loop = true)
    {
        songLoaded = false;
        if(!musicSystemLoaded) yield return new WaitUntil(()=>musicSystemLoaded);
        musicLoadType?.Invoke(songName,loop);
    }

    // Values that are consistent with currently selected music mode
    public static bool MusicLooping
    { 
        get
        {
            // Ogg
            if(!trueMidiMusic)
            return self.musicSource.loop;

            // True midi
            else
            {
                return self.songPlayer != null || self.songPlayer.Loop.Enabled;
            }
        }
        set
        {
            // Ogg
            if(!trueMidiMusic)
            self.musicSource.loop = value;

            // True midi
            else
            {
                if(self.songPlayer != null)
                {
                    ToggleInt loopInt = self.songPlayer.Loop;
                    loopInt.Enabled = value;
                    self.songPlayer.SetLoop(loopInt);
                }
            }
        }
    }
    public static bool MusicIsPlaying
    { 
        get
        {
            // Ogg
            if(!trueMidiMusic)
            return self.musicSource.isPlaying;

            // True midi
            else
            {
                return self.songPlayer == null || self.songPlayer.IsPlaying;
            }
        }
    }
    public static string MusicNowPlaying { get {return musicNowPlaying;}}
    public static float MusicLength
    {
        get
        {
            // Ogg
            if(!trueMidiMusic)
            return self.musicSource.clip.length;
            else return 0;
        }
    }
    
    // Switch music mode that is playing
    public static void SwitchMusicMode(bool trueMidi)
    {
        trueMidiMusic = trueMidi;
        // Reload music
        string savedMusicPlaying = musicNowPlaying;
        
        if(self.fluidMidi != null)
        {
            self.fluidMidi.SetActive(false);
            self.fluidMidi.SetActive(trueMidi);
        }

        // Stop currently playing music
        LoadMusic("");

        // Switch music loading type
        musicLoadType = trueMidi ? self.LoadMusicMidi : self.LoadMusicRecorded;
        musicNowPlaying = "";

        // Reload music with new system
        LoadMusic(savedMusicPlaying);
    }
    public static void ResetMidiTrack()
    {
        if(self.songPlayer == null) return;
        int tick = self.songPlayer.Ticks;
        bool loop = MusicLooping;
        string savedMusicPlaying = musicNowPlaying;
        string path = Application.streamingAssetsPath+"/TrueMidi/"+savedMusicPlaying+".mid";

        // Stop currently playing music
        musicNowPlaying = "";

        self.StartCoroutine(self.ILoadSongMidi(path,savedMusicPlaying,loop,tick));

    }
    public static void LoadMusic(string songName, bool loop = true)
    {
        self.StartCoroutine(self.IWaitUntilMusicSystemLoaded(songName,loop));
    }
    public static void SetMusicVolume(float volume)
    {
        self.audioMixer.SetFloat("MusicVolume",volume != 0 ? Mathf.Lerp(-20,-10,volume) : -80);

        // Volume range must be from 0 to 1
        if(trueMidiMusic && self.songPlayer != null)
        {
            self.songPlayer.Gain = Mathf.Lerp(0,0.2f,volume);
        }
    }
    public static void SetSFXVolume(float volume)
    {
        self.audioMixer.SetFloat("SFXVolume",volume != 0 ? Mathf.Lerp(-20,15,volume) : -80);
        float val;
        self.audioMixer.GetFloat("SFXVolume", out val);
    }
    void LoadMusicMidi(string songName, bool loop = true)
    {
        ///print("Loading music: "+songName);
        string path = Application.streamingAssetsPath+"/TrueMidi/"+songName+".mid";

        // Stop Song if no string entered.
        if(songName == "")
        {
            if(songPlayer != null && songPlayer.IsPlaying)
            {
                Destroy(songPlayer);
                fluidMidi.SetActive(false);
            }
            musicNowPlaying = "";
            print("Stopping music (MIDI).");
            return;
        }
        // Abort if currently playing song has same name.
        if(MusicNowPlaying == songName)
        {
            print("Abort");
            if(songPlayer != null && !songPlayer.IsPlaying)
            {
                songPlayer.Play();
            }
            return;
        }
        StartCoroutine(ILoadSongMidi(path,songName,loop));
    }
    void LoadMusicRecorded(string songName, bool loop = true)
    {
        ///print("Loading music: "+songName);
        // Stop Song if no string entered.
        if(songName == "")
        {
            if(MusicNowPlaying != "")
            {
                self.musicSource.Stop();
                self.StopCoroutine(LoadSongCor);
            }
            musicNowPlaying = "";
            print("Stopping music");
            return;
        }

        // Abort if currently playing song has same name.
        if(MusicNowPlaying == songName)
        {
            ///print("Abort");
            if(!musicSource.isPlaying)
            {
                musicSource.Play();
            }
            return;
        }

        string path = Application.streamingAssetsPath+"/RecordedMidi/";
        if(LoadSongCor != null) StopCoroutine(LoadSongCor);
        LoadSongCor = StartCoroutine(ILoadSong(path,songName,loop));
        ///print("Music recorded finished");
    }
    /*void LoadSounds()
    {
        audioclips = Resources.LoadAll("Sounds", typeof(AudioClip));
    }*/
    IEnumerator ILoadSounds(string path)
    {
        //Get all files with the .wav filename
        string[] filePaths = Directory.GetFiles(path, "*.wav");
        int audioMax = (int)Mathf.Min(audioFilesLength,filePaths.Length);
        audioclips = new AudioClip[filePaths.Length];
        int pathLength = path.Length;
        for(int i = 0;i<audioclips.Length;i++)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePaths[i], AudioType.WAV))
            {
                yield return www.SendWebRequest();

                audioclips[i] = DownloadHandlerAudioClip.GetContent(www);
                //Cut out file path and file extension from sound name
                audioclips[i].name = filePaths[i].Substring(pathLength,filePaths[i].Length - pathLength - 4);
            }
        }
        yield return 0;
        soundsLoadedCallback?.Invoke();
    }
    void LoadSounds()
    {
        AudioclipStringsFrame = new List<PlayedClip>();
        string path = Application.streamingAssetsPath+"/Sounds/";
        StartCoroutine(ILoadSounds(path));
    }
    
    void GenerateSoundSources()
    {
        GameObject[] objects = new GameObject[10];
        audioSourceTransforms = new AudioSourceTransform[objects.Length];
        audioHolder = new GameObject("SoundHolder").transform;
        audioHolder.SetParent(transform);
        for(int i = 0;i<10;i++)
        {
            objects[i] = new GameObject("AudioSource");
            AudioSourceTransform astr = objects[i].AddComponent<AudioSourceTransform>();
            astr.Init(audioHolder,SFXMixer);
            audioSourceTransforms[i] = astr;
        }
    }
    AudioSourceTransform FetchAudioSourceTransform(int ID)
    {
        if(audioSourceTransforms[ID] == null)
        {
            GameObject obj = new GameObject("AudioSource");
            AudioSourceTransform astr = obj.AddComponent<AudioSourceTransform>();
            audioSourceTransforms[ID] = astr;
            astr.Init(audioHolder,SFXMixer);
        }
        return audioSourceTransforms[ID];
    }
    #endregion // Audio region
    
    #region SaveData
    [SerializeField] GameKey gameKey;
    GameSettings gameSettings;
    byte[] EncryptedString;
    public static void SetEncryptedKey()
    {
        self.EncryptedString = self.gameKey.Encrypt(self.gameKey.GetGameID);
        //Debug.Log("Encrypted string: "+self.EncryptedString);
    }
    public byte[] GetEncryptedKey()
    {
        if(EncryptedString == null) SetEncryptedKey();
        return EncryptedString;
    }
    public bool IsValidKey(string encryptedkey)
    {
        ///print(gameKey.EncryptString(gameKey.UnencryptedString)+ '\n' +encryptedkey);
        //return gameKey.EncryptString(gameKey.UnencryptedString) == encryptedkey;
        return gameKey.Decrypt(encryptedkey) == gameKey.GetGameID;
    }
    public static void LoadFromFile()
    {
        World[] saveworlds = SaveLoadData.Load("save1");
        if(saveworlds != null) self.worlds = saveworlds;

        saveworlds = null;
    }
    public static void SaveToFile()
    {
        SaveLoadData.Save("save1",self.worlds);
        SaveCanvasAppear.Insert = "data";
        Instantiate(self.SaveCanvas);
    }
    public static void ShowText(string text)
    {
        SaveCanvasAppear.SaveText = false;
        SaveCanvasAppear.Insert += text;
        Instantiate(self.SaveCanvas);
    }
    public static void SaveSettingsToFile()
    {
        SaveLoadData.SaveSettings("settings");
        SaveCanvasAppear.Insert = "settings";
        Instantiate(self.SaveCanvas);
    }
    // Load settings
    IEnumerator ILoadSettings()
    {
        gameSettings = SaveLoadData.LoadSettings("settings");
        gameSettings.Init();

        yield return new WaitUntil(()=>SaveLoadData.loadComplete);

        // Music
        SetMusicVolume(GameSettings.self.MusicToggle.Get() ? GameSettings.self.MusicVolume.Get() : 0);
        
        // Music type
        trueMidiMusic = GameSettings.self.MusicType.Get() == 0 ? false : true;

        // SFX
        SetSFXVolume(GameSettings.self.SFXToggle.Get() ? GameSettings.self.SFXVolume.Get() : 0);
        
        // Resolution and fullscreen
        SetResolution(GameSettings.self.Resolution.Get());

        // Gameplay
        aimControlHold = GameSettings.self.DiagonalAim.Get() == 0 ? false : true;
        screenshake = GameSettings.self.Screenshake.Get();
        movingBGs = GameSettings.self.MovingBG.Get();

        if(GameSettings.self.PixelScaling == null)
        {
            GameSettings.self.PixelScaling = new Setting<bool>(true,"PixelScaling");
        }
        pixelScaling = GameSettings.self.PixelScaling.Get();
        if(!pixelScaling) TogglePixelPerfectScaling(false);

        // VSync
        if(GameSettings.self.VSync == null)
        {
            GameSettings.self.VSync = new Setting<int>(QualitySettings.vSyncCount,"VSync");
        }
        else
        {
            QualitySettings.vSyncCount = GameSettings.self.VSync.Get();
        }

        gameSettings.LoadArray();

        // Soundfont check
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            trueMidiMusic = false;
        }
        LoadMusicSystem();
    }
    void SettingsInit()
    {
        StartCoroutine(ILoadSettings());
    }

    #endregion //SaveData
    void OnEnable()
    {
        foreach(MonoBehaviour c in ScriptOrder)
        {
            c.Invoke("Init",0);
        }
    }
    public static bool FindCopy(string name, GameObject org)
    {
        foreach (GameObject dup in GameObject.FindGameObjectsWithTag (name))
        {
			if (dup.Equals(org))
            {
                //print("found self");
				continue;
            }
            else
            {
                print("Found copy of "+name);
                Destroy(org);
                if(name == "DataShare")
                self.GetComponent<MGInput>().OnEnable();
                return true;
            }
		}
        DontDestroyOnLoad(org);
        return false;
    }
   
    // Start is called before the first frame update
    void Awake()
    {
        #if UNITY_EDITOR
        Fps.updateRoomName();
        #endif

        if(FindCopy("DataShare",this.gameObject)) return;
        
        self = this;
        musicNowPlaying = string.Empty;
        print("Datashare initialized");

        SettingsInit();
        Application.targetFrameRate = 60;


        LoadSounds();
        GenerateSoundSources();

        print("Target framerate: "+Application.targetFrameRate);
        if(worlds.Length!=0) worlds[0].SetUnlocked(true);

        //Check world level counts
        /*string s = "";
        for(int i = 0;i<worlds.Length;i++)
        {
            int count = worlds[i].LevelCount();
            s += "World "+i+" Level count: "+count+'\n';
        }
        Debug.Log(s);*/
    }
    void LateUpdate()
    {
        AudioclipStringsFrame.Clear();
        if(trueMidiMusic && songPlayer != null)
        {
            if(Application.isFocused && songPlayer.IsPaused)
            songPlayer.Resume();

            else if(!Application.isFocused && songPlayer.IsPlaying)
            songPlayer.Pause();
        }
    }
    public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = DataShare.GameSpeed;
        Camera cam = GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetComponent<Camera>();
        Transition.RePositionCamera(cam,cam.transform.position+Transition.offsetVector);
        Transition.TransitionEvent(false);
        if(!pixelScaling) TogglePixelPerfectScaling(false);
        SceneManager.sceneLoaded-=OnSceneLoaded;
    }
    #region Levels
    IEnumerator ILevelSelectLoad(string name)
    {
        float delay = 0.5f;
        while(delay>0)
        {
            delay -= Time.unscaledDeltaTime;
            yield return 0;
        }
        SceneManager.sceneLoaded+=OnSceneLoaded;
        LoadScene(name);
        System.GC.Collect();
    }
    public static void LoadSceneWithTransition(string name)
    {
        Camera cam = GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetComponent<Camera>();
        Transition.TransitionEvent(true,cam);
        self.StartCoroutine(self.ILevelSelectLoad(name));
    }
    public static void LoadScene(string name)
    {
        try
        {
            SceneManager.LoadScene(name,LoadSceneMode.Single);
        }
        catch (System.Exception)
        {
            Debug.LogError("Scene "+name+" does not exist.");
            throw;
        }
    }
    public static void LoadLevel(string name)
    {
        if(name != "")
        {
            SceneManager.LoadScene(name,LoadSceneMode.Single);
            System.GC.Collect();
        }
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public static void LoadLevel(int worldID,int levelID, bool withTransition)
    {
        string name = self.levelNames[worldID].GetLevelScene(levelID);
        if(name != "")
        {
            Debug.Log("Loading Level "+(worldID+1)+"-"+(levelID+1)+"...");
            if(worldID<DataShare.worldAmount*2)
            {
                lastLoadedLevel = new Vector2Int(worldID,levelID);
            }
            if(withTransition)
            LoadSceneWithTransition(name);
            else LoadScene(name);
        }
    }
    public static void LoadNextLevel(int worldID,int levelID)
    {
        levelID++;
        // Go to next world
        if(levelID>levelAmount-1)
        {
            levelID = 0;
            worldID++;

            int lightWorldID = worldID - (GameMaster.self.worldMode == GameMaster.WorldMode.DarkMode ? DataShare.worldAmount : 0);
            bool worldWasUnlocked = GetWorldUnlock(lightWorldID);
            
            #if UNITY_EDITOR
            Debug.Log("World " +(lightWorldID+1)+ " unlocked");
            #endif


            DataShare.self.UnlockWorld(lightWorldID);
            // Go to level select on first run to show the next world artwork
            if(!worldWasUnlocked)
            {
                SceneManager.LoadScene("LevelSelect",LoadSceneMode.Single);
                LevelSelect.menuType = 0;
                System.GC.Collect();
                SceneManager.sceneLoaded+=OnSceneLoaded;
                lastLoadedLevel = new Vector2Int(lightWorldID,levelID);
                return;
            }
        }
        string name;
        try
        {
            name = self.levelNames[worldID].GetLevelScene(levelID);
        }
        catch (System.IndexOutOfRangeException)
        {
            Debug.LogError("Level "+(worldID+1)+"-"+(levelID+1)+" does not exist. Returning to main menu.");
            SceneManager.LoadScene("TitleScreen",LoadSceneMode.Single);
            System.GC.Collect();
            throw;
        }
        Debug.Log("Loading Level "+(worldID+1)+"-"+(levelID+1)+"...");
        lastLoadedLevel = new Vector2Int(worldID,levelID);
        SceneManager.LoadScene(name,LoadSceneMode.Single);
        System.GC.Collect();
    }

    public void UnlockWorld(int worldID)
    {
        worlds[worldID].SetUnlocked(true);
    }
    public void UnlockAllWorlds()
    {
        foreach (var world in worlds)
        {
            world.SetUnlocked(true);
        }
    }
    //0 - any% 1 - 100%
    public static LevelStats[] GetLevelStats(int worldID, int levelID)
    {
        return self.worlds[worldID].GetLevelStats(levelID);
    }
    public static bool SetLevelStats(LevelStats stats,Vector2Int levelID)
    {
        if(CheatInput.CheatVal > 0) return false;
        return self.worlds[levelID.x-1].SetLevelStats(stats, levelID.y-1);
    }

    public static string GetLevelName(int worldID, int levelID)
    {
        return self.levelNames[worldID].GetLevelName(levelID);
    }
    //Get all level marks
    public static int[] GetLevelValues(int worldID)
    {
        int[] array = self.worlds[worldID].GetAllCompletions();

        for (int i = 0; i < array.Length; i++)
        {
            int darkWorldValue = self.worlds[worldID+DataShare.worldAmount].GetCompletion(i);
            if(array[i] < darkWorldValue) array[i] = darkWorldValue;
        }

        return array;
    }
    public static int GetSpecialCollectCount()
    {
        int count = 0;
        for (int i = 0; i < worldAmount; i++)
        {
            int[] arr = self.worlds[i].GetAllCompletions();
            int[] darkArr = self.worlds[i+DataShare.worldAmount].GetAllCompletions();
            for (int k = 0; k < arr.Length; k++)
            {
                if(arr[k]<darkArr[k]) arr[k] = darkArr[k];
                if(arr[k]==2)
                {
                    ///print("World "+i+", Arr "+k+": "+arr[k]);
                    count++;
                }
            }
        }

        int ID = worldAmount*2; // Special world ID
        // Add special count for bonus world
        if(self.worlds.Length>=ID + 2)
        {
            int[] arr = self.worlds[ID].GetAllCompletions();
            int[] darkArr = self.worlds[ID+1].GetAllCompletions();
            for (int k = 0; k < arr.Length; k++)
            {
                if(arr[k]<darkArr[k]) arr[k] = darkArr[k];
                if(arr[k]==2)
                {
                    ///print("Special world, Arr "+k+": "+arr[k]);
                    count++;
                }
            }
        }

        return count;
    }
    public static void SetLevelValue(byte completionMark, Vector2Int levelID)
    {
        if(CheatInput.CheatVal > 0) completionMark = (byte)Mathf.Clamp(completionMark,0,1);

        self.worlds[levelID.x-1].SetCompletion(levelID.y-1,completionMark);
        SaveToFile();
    }
    public static int GetLevelValue(int worldID,int levelID)
    {
        return self.worlds[worldID].GetCompletion(levelID);
    }
    public static int GetWorldCount()
    {
        return self.worlds.Length;
    }
    public static int GetLevelCount(int worldID)
    {
        return self.worlds[worldID].LevelCount();
    }
    
    // Get world marks
    public static bool GetWorldUnlock(int worldID)
    {
        return self.worlds[worldID].GetUnlocked();
    }
    public static bool GetMultipleWorldsUnlocked()
    {
        int length = Mathf.Min(self.worlds.Length,worldAmount);

        for(int i = 1; i<length;i++)
        {
            if(DataShare.GetWorldUnlock(i)) return true;
        }
        return false;
    }
    public static bool GetAllLightWorldsUnlocked()
    {
        for(int i = 1; i<worldAmount*2;i++)
        {
            if(!DataShare.GetWorldUnlock(i)) return false;
        }
        return true;
    }
    public static bool[] GetWhichWorldsUnlocked()
    {
        bool[] arr = new bool[self.worlds.Length];
        int ID = worldAmount*2;
        int min = Mathf.Min(self.worlds.Length,ID);
        for (int i = 0; i < min; i++)
        {
            arr[i] = self.worlds[i].HasACompletion();
        }
        // Special world check
        if(self.worlds.Length>=ID + 2)
        {
            arr[ID] = self.worlds[ID].HasACompletion();
            arr[ID+1] = self.worlds[ID+1].HasACompletion();
        }
        return arr;
    }
    #endregion
    #region sounds
    AudioClip GetSound(string s)
    {
        if(CheatInput.SoundPlayOffset == -1)
            return audioclips[CheatInput.rand.Next(audioFilesLength)];

        AudioClip clip;
        for (int i = 0; i < audioclips.Length; i++)
        {
            clip = audioclips[i];
            if(clip==null)
            {
                return null;
            }
            if(clip.name == s)
            {
                string name = clip.name;

                if(CheatInput.SoundPlayOffset != 0)
                {
                    return audioclips[(int)Mathf.Repeat(i+CheatInput.SoundPlayOffset,audioFilesLength)];
                }

                return clip;
            }
        }
        return null;
    }
    AudioClip GetSound(int id)
    {
        if(Mathf.Clamp(id,0,audioclips.Length) != id) return null;

        if(CheatInput.SoundPlayOffset != 0)
        {
            if(CheatInput.SoundPlayOffset != -1)
            id = (int)Mathf.Repeat(id+CheatInput.SoundPlayOffset,audioFilesLength);

            else id = CheatInput.rand.Next(audioFilesLength);
        }

        //string name = audioclips[id].name;

        return audioclips[id];
    }
    public static int GetSoundID(string s)
    {
        for(int i = 0; i< self.audioclips.Length; i++)
        {
            if(self.audioclips[i]==null)
            {
                return -1;
            }
            if(self.audioclips[i].name == s)
            {
                return i;
            }
        }
        return -1;
    }
    //Play without follow, int ID is fetched for stopping looping sounds, otherwise unused
    public static int PlaySound(string clip, Vector3 pos, bool loop, float volume, float pitch)
    {
        AudioClip audioClip = self.GetSound(clip);
        float distance = Vector2.Distance(pos,CamControl.cameraTransform.position);

        // Check if sound playing already exists
        if(AudioclipStringsFrame.Count != 0)
        {
            PlayedClip matchingClip = AudioclipStringsFrame.FirstOrDefault(i => i.audioClip == audioClip);
            if(matchingClip != null)
            {     
                if(matchingClip.distance > distance)
                {
                    matchingClip.transform.position = pos;
                }
                return matchingClip.index;
            }
        }

        for(int i = 0;i<self.audioSourceTransforms.Length;i++)
        {
            AudioSourceTransform a = self.FetchAudioSourceTransform(i);
            if(!a.GetActive)
            {
                a.Play(audioClip,pos,loop,volume,pitch);
                return i;
            }
        }
        return -1;
    }
    public static int PlaySound(string clip, bool loop, float volume, float pitch)
    {
        for(int i = 0;i<self.audioSourceTransforms.Length;i++)
        {
            AudioSourceTransform a = self.FetchAudioSourceTransform(i);
            if(!a.GetActive)
            {
                a.Play(self.GetSound(clip),loop,volume,pitch);
                return i;
            }
        }
        return -1;
    }
    public static int PlaySound(AudioClip clip, bool loop, float volume, float pitch)
    {

        for(int i = 0;i<self.audioSourceTransforms.Length;i++)
        {
            AudioSourceTransform a = self.FetchAudioSourceTransform(i);
            if(!a.GetActive)
            {
                a.Play(clip,loop,volume,pitch);
                return i;
            }
        }
        return -1;
    }
    public static int PlaySound(string clip, Transform source, bool loop, float volume, float pitch)
    {
        for(int i = 0;i<self.audioSourceTransforms.Length;i++)
        {
            AudioSourceTransform a = self.FetchAudioSourceTransform(i);
            if(!a.GetActive)
            {
                a.PlayAndFollow(self.GetSound(clip),source,loop,volume,pitch);
                return i;
            }
        }
        return -1;
    }
    public static int PlaySound(string clip, Vector3 pos, bool loop)
    {
        AudioClip audioClip = self.GetSound(clip);
        float distance = Vector2.Distance(pos,CamControl.cameraTransform.position);
        

        // Check if sound playing already exists
        if(AudioclipStringsFrame.Count != 0)
        {
            PlayedClip matchingClip = AudioclipStringsFrame.FirstOrDefault(i => i.audioClip == audioClip);
            if(matchingClip != null)
            {             
                if(matchingClip.distance > distance)
                {
                    matchingClip.transform.position = pos;
                }
                return matchingClip.index;
            }
        }
        for(int i = 0;i<self.audioSourceTransforms.Length;i++)
        {
            AudioSourceTransform a = self.FetchAudioSourceTransform(i);
            if(!a.GetActive)
            {
                a.Play(audioClip,pos,loop,1,1);
                AudioclipStringsFrame.Add(new PlayedClip(a.transform,audioClip,distance,i));
                return i;
            }
        }
        return -1;
    }
    public static int PlaySound(string clip, Transform source, bool loop)
    {
        for(int i = 0;i<self.audioSourceTransforms.Length;i++)
        {
            AudioSourceTransform a = self.FetchAudioSourceTransform(i);
            if(!a.GetActive)
            {
                a.PlayAndFollow(self.GetSound(clip),source,loop,1,1);
                return i;
            }
        }
        return -1;
    }
    public static int PlaySound(int clip, Vector3 pos, bool loop, float volume, float pitch)
    {
        AudioClip audioClip = self.GetSound(clip);
        float distance = Vector2.Distance(pos,CamControl.cameraTransform.position);

        // Check if sound playing already exists
        if(AudioclipStringsFrame.Count != 0)
        {
            PlayedClip matchingClip = AudioclipStringsFrame.FirstOrDefault(i => i.audioClip == audioClip);
            if(matchingClip != null)
            {  
                if(matchingClip.distance > distance)
                {
                    matchingClip.transform.position = pos;
                }
                return matchingClip.index;
            }
        }

        for(int i = 0;i<self.audioSourceTransforms.Length;i++)
        {
            AudioSourceTransform a = self.FetchAudioSourceTransform(i);
            if(!a.GetActive)
            {
                a.Play(audioClip,pos,loop,volume,pitch);
                return i;
            }
        }
        return -1;
    }
    public static int PlaySound(int clip, bool loop, float volume, float pitch)
    {
        for(int i = 0;i<self.audioSourceTransforms.Length;i++)
        {
            AudioSourceTransform a = self.FetchAudioSourceTransform(i);
            if(!a.GetActive)
            {
                a.Play(self.GetSound(clip),loop,volume,pitch);
                return i;
            }
        }
        return -1;
    }
    public static int PlaySound(int clip, Transform source, bool loop, float volume, float pitch)
    {
        for(int i = 0;i<self.audioSourceTransforms.Length;i++)
        {
            AudioSourceTransform a = self.FetchAudioSourceTransform(i);
            if(!a.GetActive)
            {
                a.PlayAndFollow(self.GetSound(clip),source,loop,volume,pitch);
                return i;
            }
        }
        return -1;
    }
    public static int PlaySound(int clip, Vector3 pos, bool loop)
    {
        AudioClip audioClip = self.GetSound(clip);
        float distance = Vector2.Distance(pos,CamControl.cameraTransform.position);

        // Check if sound playing already exists
        if(AudioclipStringsFrame.Count != 0)
        {
            PlayedClip matchingClip = AudioclipStringsFrame.FirstOrDefault(i => i.audioClip == audioClip);
            if(matchingClip != null)
            {
                if(matchingClip.distance > distance)
                {
                    matchingClip.transform.position = pos;
                }
                return matchingClip.index;
            }
        }

        for(int i = 0;i<self.audioSourceTransforms.Length;i++)
        {
            AudioSourceTransform a = self.FetchAudioSourceTransform(i);
            if(!a.GetActive)
            {
                a.Play(audioClip,pos,loop,1,1);
                return i;
            }
        }
        return -1;
    }
    public static int PlaySound(int clip, Transform source, bool loop)
    {
        for(int i = 0;i<self.audioSourceTransforms.Length;i++)
        {
            AudioSourceTransform a = self.FetchAudioSourceTransform(i);
            if(!a.GetActive)
            {
                a.PlayAndFollow(self.GetSound(clip),source,loop,1,1);
                return i;
            }
        }
        return -1;
    }
    public static void StopSound(int ID)
    {
        if(ID != Mathf.Clamp(ID,0,self.audioSourceTransforms.Length-1)) return;
        AudioSourceTransform a = self.FetchAudioSourceTransform(ID);
        if(a.GetActive)
        {
            a.Stop();
        }
    }
    public static void StopAllSounds()
    {
        foreach (AudioSourceTransform a in self.audioSourceTransforms)
        {
            if(a.GetActive)
            {
                a.Stop();
            }
        }
    }
    #endregion
}
