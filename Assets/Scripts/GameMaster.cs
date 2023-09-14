using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using TMPro;

public class GameMaster : MonoBehaviour
{
    public enum WorldMode {LightMode, DarkMode};

    [Header ("Level Data")]
    [Space]

    [Tooltip("The current level ID (World, Level).")]
    [SerializeField] Vector2Int levelID = Vector2Int.one;

    [Tooltip("The direct next level to load, leave blank to load next level in the world automatically.")]
    [SerializeField] Vector2Int explicitNextLevel = Vector2Int.zero;
    [Tooltip ("Level type, this affects some enemy behaviour and their drops.")]
    public WorldMode worldMode = WorldMode.LightMode;
    public static WorldMode LastWorldMode = WorldMode.LightMode;
    [Space]
    public static bool DebugInfo = false;
    //Gore
    static GameObject[] GoreObjects;
    [Header ("Variables")]
    [Space]
    public short pooledGore = 5;

    // Bullets
    static BulletMovement[] BulletObjects;
    static ParticleSystemRenderer[] ImpactObjects;
    static GameObject[] keyblockParticles;
    public byte pooledBullets = 25;
    public byte pooledImpacts = 15;
    public byte pooledKeyParticles = 6;

    // Values
    public const byte maxHealth = 2;
    [SerializeField] GameObject LevelClearSample;
    [SerializeField] GameObject LevelIntroSample;
    [SerializeField] GameObject CrashScreen;
    public Material enemyRespawnMaterial;
    public Material shapeMaterial;
    public static GameMaster self;
    public static float TimerSubtract = 0;
    public static byte Deaths = 0;
    public static byte EnemiesKilled = 0;
    static byte TotalEnemies = 0;
    public static bool Special = false;
    public static bool Goal = false;
    public static byte superModeTime = 0;
    public const byte superModeStartTime = 10;
    static bool worldModeChanged = false;
    public static byte sceneLoadedStack = 0;

    //Tiles
    [Tooltip ("Strings included in tile names for exclusionary operations (their index position matters).")]
    public string[] blacklistStrings = new string[2] {"Spike","KeyLock"};
    Tilemap[] tilemaps = new Tilemap[2];

    void SpawnCrashText(bool black)
    {
        TextMeshProUGUI newtextObject = Instantiate(HUD.livesText);
        RectTransform rectTransform = newtextObject.GetComponent<RectTransform>();
        rectTransform.SetParent(HUD.livesText.transform.parent.parent);
        rectTransform.localPosition = new Vector3(Random.Range(-430,430),Random.Range(-250,250),0);
        rectTransform.localScale = Vector3.one;
        rectTransform.eulerAngles = Vector3.zero;
        rectTransform.sizeDelta = new Vector2(Random.Range(60,380),Random.Range(60,540));
        byte randAlignment = (byte)Random.Range(0,3);
        if(black)
        {
            newtextObject.color = Color.black;
            newtextObject.fontStyle = FontStyles.Normal;
        }

        switch(randAlignment)
        {
            default: newtextObject.alignment = TextAlignmentOptions.Left; break;
            case 1: newtextObject.alignment = TextAlignmentOptions.Center; break;
            case 2: newtextObject.alignment = TextAlignmentOptions.Right; break;
        }
        newtextObject.enableWordWrapping = true;
        newtextObject.richText = true;
        newtextObject.text = TextGenerate.GetCrash();
    }
    IEnumerator IGameOverCrash()
    {
        PauseMenu.allowPause = true;
        yield return new WaitForSeconds(0.11f);

        PlayerControl[] players = FindObjectsOfType<PlayerControl>();
        PlayerControl.freezePlayerInput = 1;
        foreach(PlayerControl player in players)
        {
            player.StopAllCoroutines();
            player.anim.speed = 0;
        }

        Camera cam = GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetComponent<Camera>();
        Canvas HUDCanvas = HUD.self.GetComponent<Canvas>();
        DataShare.StopAllSounds();
        DataShare.LoadMusic("");
        Destroy(PauseMenu.self.gameObject);
        
        HUD.SetHeadState(4);
        HUD.livesText.alignment =TextAlignmentOptions.Left;

        // Crash happens here
        DataShare.PlaySound(Resources.Load<AudioClip>("Audio/pre_crash"),false,0.2f,1f);
        byte crashTextAmount = (byte)Random.Range(3,15);
        while(crashTextAmount>0)
        {
            SpawnCrashText(true);
            crashTextAmount--;
        }
        crashTextAmount = (byte)Random.Range(4,8);
        while(crashTextAmount>0)
        {
            SpawnCrashText(false);
            crashTextAmount--;
        }
        yield return 0;
        Time.timeScale = 0;

        cam.GetComponent<UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera>().enabled = false;
        cam.rect = new Rect(0,0.1f,0.82f,1.31f);

        HUDCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        float TimeToWait = 2f;
        while(TimeToWait>0)
        {
            TimeToWait-=Time.fixedDeltaTime;
            yield return 0;
        }
        // Display random crash here
        Instantiate(CrashScreen,transform.position,Quaternion.identity);

        GameMaster.superModeTime = 0;
        PlayerControl.respawn = false;
        PlayerControl.SetHP(GameMaster.maxHealth,false);
        PlayerControl.SetLives(-PlayerControl.currentLives + 5,false);
        PlayerControl.freezePlayerInput = 0;
    }
    public static void Crash()
    {
        self.StartCoroutine(self.IGameOverCrash());
    }

    public static string GetLevelTitle()
    {
        try
        {
            return DataShare.GetLevelName(self.levelID.x-1,self.levelID.y-1);
        }
        catch (System.IndexOutOfRangeException)
        {
            Debug.LogError("Level "+(self.levelID.x)+"-"+(self.levelID.x)+" does not exist.");
            throw;
        }
    }
    void GenerateGoreObjects()
    {
        if(pooledGore == 0) return;
        Transform holder = transform.GetChild(0);
        GameObject sample = holder.GetChild(0).gameObject;
        GoreObjects = new GameObject[pooledGore];
        GoreObjects[0] = sample;

        for(int i = 1;i<pooledGore;i++)
        {
            GameObject o = Instantiate(sample);
            o.SetActive(false);
            o.transform.SetParent(holder);
            GoreObjects[i] = o;
        }

    }
    void GenerateBulletObjects()
    {
        if(pooledBullets == 0) return;
        Transform holder = transform.GetChild(1);
        GameObject sample = holder.GetChild(0).gameObject;
        sample.SetActive(true);
        BulletObjects = new BulletMovement[pooledBullets];
        BulletObjects[0] = sample.GetComponent<BulletMovement>();
        BulletObjects[0].impact = SpawnImpact; //Assign impact script
        BulletObjects[0].Start();
        BulletObjects[0].SetParent(holder);

        for(int i = 1;i<pooledBullets;i++)
        {
            BulletMovement o = Instantiate(sample).GetComponent<BulletMovement>();
            o.impact = SpawnImpact; //Assign impact script
            o.Start();
            o.SetParent(holder);
            o.transform.SetParent(holder);
            BulletObjects[i] = o;
        }
        sample.SetActive(false);
        sample.transform.SetParent(holder);
    }  
    public static void CheckKeyGenerate()
    {
        if(keyblockParticles == null || keyblockParticles.Length == 0)
        {
            self.GenerateKeyParticles();
        }
        ///else print(keyblockParticles.Length);
    }
    void GenerateImpactObjects()
    {
        if(pooledImpacts == 0) return;
        Transform holder = transform.GetChild(2);
        GameObject sample = holder.GetChild(0).gameObject;
        ImpactObjects = new ParticleSystemRenderer[pooledImpacts];
        ImpactObjects[0] = sample.GetComponent<ParticleSystemRenderer>();

        for(int i = 1;i<pooledImpacts;i++)
        {
            GameObject o = Instantiate(sample);
            o.SetActive(false);
            o.transform.SetParent(holder);
            ImpactObjects[i] = o.GetComponent<ParticleSystemRenderer>();
        }

    }
    void GenerateKeyParticles()
    {
        Transform holder = transform.GetChild(3);
        GameObject sample = holder.GetChild(0).gameObject;
        keyblockParticles = new GameObject[pooledKeyParticles];
        keyblockParticles[0] = sample;
        sample.SetActive(false);

        for(int i = 1; i<pooledKeyParticles;i++)
        {
            GameObject o = Instantiate(sample);
            o.transform.SetParent(holder);
            o.SetActive(false);
            keyblockParticles[i] = o;
        }
    }
    public static void ShowResults()
    {
        Transform obj = Instantiate(self.LevelClearSample).transform;

        // Generate text
        obj.GetChild(0).GetComponent<TextMeshProUGUI>().text = TextGenerate.GetWin();
        obj.GetChild(1).GetComponent<TextMeshProUGUI>().text = GetLevelTitle();
        obj.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>().text = TextGenerate.GetAnyKey();
        obj.GetChild(obj.childCount-1).gameObject.SetActive(Special);
        if(!Special) obj.GetComponent<AnimationSoundPlayer>().soundStrings[4] = string.Empty;

        // Time
        float time = Time.timeSinceLevelLoad-TimerSubtract;
        TimerSubtract = 0;
        string formattedTime = "SUCKS";
        if(time<=600)
        {
            formattedTime = string.Format("{0:#0}:{1:00}.{2:00}",
            Mathf.Floor(time / 60), //Minutes
            Mathf.Floor(time) % 60, //Seconds
            Mathf.Floor((time * 100) % 100)); //Miliseconds
            ///print(time+", formatted: "+formattedTime);
        }

        // Set time
        obj.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = formattedTime;
        // Set deaths
        obj.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = Deaths.ToString(Deaths >= 100 ? "000" : "00");
        // Set kills
        byte percentKill = (byte)Mathf.Floor(((float)EnemiesKilled/(float)TotalEnemies)*100);
        
        print("Total enemies: "+TotalEnemies+", killed: "+EnemiesKilled+", percent: "+percentKill);
        obj.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = percentKill.ToString()+'%';
        
        SaveStats(time,percentKill);
        
        self.StartCoroutine(NextLevel(obj.GetComponent<Animator>(),true));

    }
    // Final boss save stats
    public static void SaveStats()
    {
        float time = Time.timeSinceLevelLoad-TimerSubtract;
        byte percentKill = (byte)Mathf.Floor(((float)EnemiesKilled/(float)TotalEnemies)*100);

        LevelStats stats = new LevelStats(Deaths,percentKill,time,Special);

        DataShare.IncreaseRunTimer(time);

        if(self.levelID.y >= DataShare.levelAmount-1)
        {
            #if UNITY_EDITOR
            Debug.Log("All worlds unlocked");
            #endif
            DataShare.self.UnlockAllWorlds();
        }
        
        // Save run time
        DataShare.FinishRunTimer();

        //Save results to the level stats
        if(DataShare.SetLevelStats(stats, self.levelID))
        {
            Debug.Log("NEW RECORD!");
        }

        DataShare.SetLevelValue((byte)(Special ? 2 : 1),self.levelID);
        Reset();
    }
    public static void SaveStats(float time, byte percentKill)
    {
        LevelStats stats = new LevelStats(Deaths,percentKill,time,Special);

        DataShare.IncreaseRunTimer(time);

        //Save results to the level stats
        if(DataShare.SetLevelStats(stats, self.levelID))
        {
            Debug.Log("NEW RECORD!");
        }
        DataShare.totalDeaths+=Deaths;
        DataShare.SetLevelValue((byte)(Special ? 2 : 1),self.levelID);
        Reset();
    }
    // Change to dark or light world
    public static void WorldModeCheck(bool onDeath)
    {
        //print("World mode check: "+ self.worldMode + " Player lives: " + PlayerControl.currentLives + " Eligible: "+
        //(self.worldMode == WorldMode.LightMode && PlayerControl.currentLives-(onDeath ? 1 : 0) < 0 ||
         //self.worldMode == WorldMode.DarkMode && PlayerControl.currentLives-(onDeath ? 1 : 0) >= 5));
        
        // Only attempt to switch worlds if in light world and at <0 lives or in dark world with >=0 lives
        bool DarkMode = self.worldMode == WorldMode.DarkMode;
        if(!DarkMode && PlayerControl.currentLives-(onDeath ? 1 : 0) < 0
        || DarkMode && PlayerControl.currentLives-(onDeath ? 1 : 0) >= 0)
        {
            int oldId = self.levelID.x;
            self.levelID.x = DarkMode ? oldId - DataShare.worldAmount : oldId + DataShare.worldAmount;
            worldModeChanged = oldId != self.levelID.x;
        }
    }
    public static bool ShouldGoToDarkWorld()
    {
        ///print("Current lives: "+PlayerControl.currentLives);
        if(PlayerControl.currentLives-(PlayerControl.respawn ? 1 : 0) < 0) return true;
        else if (PlayerControl.currentLives-(PlayerControl.respawn ? 1 : 0) >= 0) return false;
        else return self.worldMode == WorldMode.DarkMode;
    }
    public static void ReloadLevel(bool onDeath)
    {
        if(onDeath) DataShare.IncreaseRunTimer(Time.timeSinceLevelLoad);
        Special = false;
        int ID = self.levelID.x;
        if(self.levelID.x-1>=DataShare.worldAmount*2)
        {
            bool darkWorld = ShouldGoToDarkWorld();
            self.levelID.x = DataShare.worldAmount*2+1 + (darkWorld ? 1 : 0);
            worldModeChanged = ID != self.levelID.x;
        }
        else WorldModeCheck(onDeath);
        DataShare.LoadLevel(self.levelID.x-1,self.levelID.y-1,worldModeChanged);
    }
    public static void ReloadLevelCrash()
    {
        Special = false;
        int ID = self.levelID.x;
        if(self.levelID.x-1>=DataShare.worldAmount*2)
        {
            bool darkWorld = ShouldGoToDarkWorld();
            self.levelID.x = DataShare.worldAmount*2+1 + (darkWorld ? 1 : 0);
            worldModeChanged = ID != self.levelID.x;
        }
        else WorldModeCheck(false); // Force light world
        DataShare.LoadLevel(self.levelID.x-1,self.levelID.y-1,false);
    }
    static IEnumerator NextLevel(Animator anim,bool repositionCamera)
    {
        yield return 0;
        yield return 0;
        yield return new WaitUntil(()=>anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f);
        yield return new WaitUntil(()=>MGInput.AnyKey());
        anim.SetTrigger("Press");
        DataShare.PlaySound("Menu_Select",false,0.2f,1);

        // Wait until animation ends
        yield return 0;
        yield return new WaitUntil(()=>anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);

        // Load level
        float unscaledWait = 0.5f;
        while(unscaledWait>0)
        {
            unscaledWait-=Time.unscaledDeltaTime;
            yield return 0;
        }
        if(self.explicitNextLevel != Vector2Int.zero)
        {
            self.levelID = self.explicitNextLevel;
            self.levelID.y-=1;
        }
        WorldModeCheck(false);
        DataShare.LoadNextLevel(self.levelID.x-1,self.levelID.y-1);
    }
    
    IEnumerator LoadNewScene()
    {
        yield return 0;
        float unscaledWait = 0.01f;
        while(unscaledWait>0)
        {
            unscaledWait-=Time.unscaledDeltaTime;
            yield return 0;
        }
        Time.timeScale = DataShare.GameSpeed;
        Transition.TransitionEvent(false);
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded-=OnSceneLoaded;
        sceneLoadedStack = (byte)Mathf.Clamp(sceneLoadedStack-=1,0,255);

        if(CheatInput.CheatVal % 2 == 1)
        {
            // Decrease cheat stack by a non persistent value on level end
            CheatInput.DecreaseCheatLevel(1);
        }
        if(self == null)
        {
            print("No gm found");
            return;
        }
        TimerSubtract = 0;
        Goal = false;
        print("GM onsceneloaded");
        Camera cam = GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetComponent<Camera>();
        Transition.RePositionCamera(cam,cam.transform.position+Transition.offsetVector);
        DataShare.self.StartCoroutine(LoadNewScene());
    }
    public static void Reset()
    {
        PlayerControl.checkPointVal = 0;
        superModeTime = 0;
        Deaths = 0;
        EnemiesKilled = 0;
        TotalEnemies = 0;
        Special = false;
    }
    // Generic gore
    public static void SpawnGore(Vector3 position)
    {
        foreach(GameObject o in GoreObjects)
        {
            if(!o.activeInHierarchy)
            {
                o.transform.position = position;
                o.SetActive(true);
                break;
            }
        }
    }
    // Generic bullets
    public static void SpawnBullet(Vector3 position, Quaternion rotation,int dir)
    {
        foreach(BulletMovement o in BulletObjects)
        {
            if(!o.gameObject.activeInHierarchy)
            {
                o.transform.position = position;
                o.transform.rotation = rotation;
                o.Spawn(position,rotation,dir);
                break;
            }
        }
    }
    public static void SpawnKeyParticles(Vector3 position)
    {
        for(int i = 0;i<keyblockParticles.Length;i++)
        {
            if(!keyblockParticles[i].gameObject.activeInHierarchy)
            {
                DataShare.PlaySound("Keyblock_break",position,false);
                keyblockParticles[i].transform.position = position;
                keyblockParticles[i].SetActive(true);
                return;
            }
        }
        DataShare.PlaySound("Keyblock_break",position,false);
        keyblockParticles[0].transform.position = position;
        keyblockParticles[0].SetActive(true);
    }
    public static Transform GetClosestTransform (string tag, Vector3 position)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);

        if(targets.Length == 1) return targets[0].transform;
        if(targets.Length == 0) return null;

        foreach(GameObject potentialTarget in targets)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if(dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
            }
        }
     
        return bestTarget;
    }
    public void SpawnImpact(Transform t,Renderer renderer)
    {
        foreach(ParticleSystemRenderer o in ImpactObjects)
        {
            if(!o.gameObject.activeInHierarchy)
            {
                if(renderer!=null)
                {
                    o.sortingLayerID = renderer.sortingLayerID;
                    o.sortingOrder = renderer.sortingOrder-1;
                }
                Transform tr = o.transform;
                tr.rotation = t.rotation;
                tr.position = t.position - (t.right*t.localScale.x)*0.25f;
                tr.localScale = t.localScale;
                o.gameObject.SetActive(true);
                break;
            }
        }
    }
    void CountEnemies()
    {
        EnemiesKilled = 0;
        TotalEnemies = (byte)GameObject.Find("Enemies").transform.childCount;
    }
    void FindTilemaps()
    {
        tilemaps[0] = GameObject.FindWithTag("MainMap").GetComponent<Tilemap>();
        tilemaps[1] = tilemaps[0].transform.parent.GetChild(1).GetComponent<Tilemap>();
    }
    string GetTileName(Vector3Int pos, Tilemap tilemap)
    {
        TileBase tile = tilemap.GetTile(pos);
        if(tile!=null)
        return tilemap.GetTile(pos).name;
        else return "";
    }
    bool SetTile(Vector3Int position,Tilemap t,string Compare,TileBase newTile)
    {
        if(GetTileName(position,t) == Compare)
        {
            t.SetTile(position,newTile);
            return true;
        }
        return false;
    }
    IEnumerator ChainReactionDelay(Vector3Int position)
    {
        yield return new WaitForSeconds(0.1f);
        LockChainReaction(position);
    }
    void ChainReactionCheck(Vector3Int position,Tilemap t,string tileName)
    {
        if(self.SetTile(position,t,tileName,null))
        {
            //Chain reaction
            self.StartCoroutine(self.ChainReactionDelay(position));
            SpawnKeyParticles(position+(Vector3)(Vector2.one/2));
        }
    }
    public static void StartChainReaction(Vector3Int position)
    {
        Tilemap t = self.tilemaps[0];
        ///print(t+" "+position);
        string keyLockTileName = "KeyLockTile";
        
        self.ChainReactionCheck(position,t,keyLockTileName); //Left

    }
    static void LockChainReaction(Vector3Int position)
    {
        Tilemap t = self.tilemaps[0];
        string keyLockTileName = "KeyLockTile";
        
        // Check left, right, top and bottom of tile for adjascent
        self.ChainReactionCheck(position-Vector3Int.right,t,keyLockTileName); //Left
        self.ChainReactionCheck(position+Vector3Int.right,t,keyLockTileName); //Right
        self.ChainReactionCheck(position+Vector3Int.up,t,keyLockTileName); //Up
        self.ChainReactionCheck(position-Vector3Int.up,t,keyLockTileName); //Down
    }
    public static int GetTileResult(Vector3 other, Vector3Int pos, string tag)
    {
        // 0 - Empty, 1 - Spike, 2 - Keylock, -1 - Other
        Tilemap usedTilemap = self.tilemaps[tag.Contains("SS") ? 1 : 0];
        TileBase t = usedTilemap.GetTile(pos);
        if(t == null)
        {
            return 0;
        }
        // Compare with given tiles
        else
        {
            for(int i = 0;i<self.blacklistStrings.Length;i++)
            {
                if(t.name.Contains(self.blacklistStrings[i]))
                {
                    //Check rotation
                    if(t.name.Contains("AngleSpike"))
                    {
                        float rotation = usedTilemap.GetTransformMatrix(pos).rotation.eulerAngles.z;
                        Vector2 direction = (pos - other).normalized;

                        ///print(other+" " + direction + " " + rotation);
                        ///print(pos + " " + t.name + " "+rotation + " "+other+ " "+direction);

                        switch(rotation)
                        {
                            default: return 0;

                            case 0:
                                if(direction.y == -1) return i+1;
                                else return 0; 

                            case 180:
                                if(direction.y == 1) return i+1;
                                else return 0; 

                            case 90:
                                if(direction == Vector2.right) return i+1;
                                else return 0; 
                            case 270:
                                if(direction == Vector2.left) return i+1;
                                else return 0;
                            
                        }
                    }
                    return i+1;
                }
            }
            return -1;
        }
    }
    
    IEnumerator IAllowPauseCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        PauseMenu.allowPause = true;
    }
    // Start is called before the first frame update
    void Awake()
    {
        self = this;
        LastWorldMode = worldMode;

        print("Added gm on scene loaded");
        if(sceneLoadedStack == 0)
        {
            sceneLoadedStack = (byte)Mathf.Clamp(sceneLoadedStack+=1,0,255);
            SceneManager.sceneLoaded+=self.OnSceneLoaded;
        }

        GenerateGoreObjects();
        GenerateBulletObjects();
        GenerateImpactObjects();

        keyblockParticles = new GameObject[0];
        CountEnemies();
        FindTilemaps();

        if(levelID == Vector2Int.zero) return;
        if(worldModeChanged || (Deaths==0 && LevelIntroSample!=null))
        {
            worldModeChanged = false;
            Instantiate(LevelIntroSample);
            StartCoroutine(IAllowPauseCooldown());
        }
    }
    void Start()
    {
        PauseMenu.SetLevelName();
    }
    #if UNITY_EDITOR
    void FixedUpdate()
    {
        if(MGInput.GetKeyDown(Keyboard.current.pKey))
        {
            DebugInfo = !DebugInfo;
        }
    }
    #endif
}
