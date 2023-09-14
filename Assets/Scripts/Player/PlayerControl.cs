using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public static bool godmode = false;
    public static int currentHealth = 2;
    public static int currentLives = 5;
    public static readonly Vector2Int LivesRange = new Vector2Int(-10,15);
    float currentSpeed;
    [Header ("Speeds")]
    [Space]
    public float speedAdditive = 0.1f;
    public float airSpeedAdditive = 2f;
    public float wallJumpSpeedAdditive = 1f;
    public float wallJumpPressedSpeedAdditive = 4f;
    public float climbSpeed = 20;
    bool slowSpeedup = false;
    [Header ("Jumps")]
    [Space]
    public int jumpAmount = 2;
    public float jumpForce = 1;
    public float jumpReleaseReduct = 2;
    public float wallJumpStrength = 5;
    public bool grounded = false;
    //bool onSSGround = false;
    bool walled = false;
    int LadderVal = 0;
    public int GetLadderVal { get{ return LadderVal;}} // Read only
    Rigidbody2D rb;
    public static BoxCollider2D col;
    VelocityCap speedCap;
    [Header ("Misc")]
    [Space]
    public bool DebugLineDraw = false;
    bool wallJump = false;
    int jumpsRemaining = 0;
    public int JumpsRemaining { get {return jumpsRemaining;} set {jumpsRemaining = value;}}
    int lastXpos = 0;
    int xPos = 0;
    int facing = 0;
    float downDoubleTapDelay = 0;
    float jumpBuffer = 0;
    float ladderBuffer = 0;
    float jumpThreshold = 0;
    int walledSide = 0;
    float roundedVelocity = 0;
    public float YVelocity { get { return roundedVelocity; }}
    float GravityScale = 0;

    //Unfreeze if value is 0
    public static byte freezePlayerInput = 0;
    public static bool respawn = false;
    public static byte checkPointVal = 0;

    delegate void ShootDelegate();
    ShootDelegate shootDelegate;

    public delegate void PlayerFixedUpdateControl(bool OnLadder, bool grounded);
    public delegate void PlayerFixedUpdate();
    public PlayerFixedUpdate playerFixedUpdate;
    public PlayerFixedUpdateControl playerFixedUpdateControl;
    public static bool freezeJump;
    bool jumpFreezeBypass = false;
    bool lockLadderUp = false;
    
    [HideInInspector]
    public Animator anim;
    CamTransition camTransition;
    CamControl camControl;
    Vector2 prevPos = Vector2.one*-99;

    //Renderers
    SpriteRenderer bodyRenderer,headRenderer,GunRenderer;
    [SerializeField] GameObject Gibs;
    [SerializeField] GameObject JumpingDust;
    [SerializeField] GameObject FlameHead;
    [SerializeField] GameObject TrumpetEffect;
    public static ParticleSystem JumpingDustParticleSystem;

    [Tooltip("Vertical offset from the bottom to draw debug information with.")]
    public float GUIDataOffset = 50;
    int loopingSoundID = -1;

    public delegate void JumpEvent();
    public JumpEvent jumpEvent;

    delegate void LivesAddEvent(bool resetTimer);
    static LivesAddEvent livesAddEvent;
    void StopParticles()
    {
        JumpingDustParticleSystem.Stop(false,ParticleSystemStopBehavior.StopEmitting);
        var main = JumpingDustParticleSystem.main;
        main.loop = false;
    }
    void ShowParticles(int side)
    {
        if(Time.timeSinceLevelLoad<0.1f)return;
        if(JumpingDustParticleSystem.isPlaying) JumpingDustParticleSystem.Stop(false,ParticleSystemStopBehavior.StopEmitting);

        Transform tr = JumpingDustParticleSystem.transform;
        Vector3 pos = tr.localPosition;
        pos.x = side * 0.3f;
        tr.localPosition = pos;

        var main = JumpingDustParticleSystem.main;
        main.loop = walled;

        var emission = JumpingDustParticleSystem.emission;
        emission.rateOverTime = walled ? 25 : 100;  

        JumpingDustParticleSystem.Play(false);
    }
    public static void DecPlayerFreeze()
    {
        if(freezePlayerInput>0)
        freezePlayerInput--;
    }
    public static void SetHP(int amount, bool additive=true)
    {
        if(additive)
        {
            if(CheatInput.LockHealth) return;
            amount+=currentHealth;
        }

        currentHealth = Mathf.Clamp(amount,-1,GameMaster.maxHealth);
        print("Current health: "+currentHealth);
        HUD.SetHealthDisplay(currentHealth,additive);
    }
    public void FindSpawn()
    {
        Transform spawnPoint = null;
        List <Transform> spawnPoints = new List<Transform>();
        GameObject[] objs = GameObject.FindGameObjectsWithTag("PlayerSpawn");

        for(int i = 0; i<objs.Length; i++)
            spawnPoints.Add(objs[i].transform);

        if(spawnPoints.Count > 1)
        {
            // Sort alphabetically
            spawnPoints = spawnPoints.OrderBy(o=>o.name).ToList();
            spawnPoint = spawnPoints[Mathf.Clamp((int)checkPointVal,0,spawnPoints.Count-1)];
        }
        else if (spawnPoints.Count == 1) spawnPoint = spawnPoints[0];

        if(spawnPoint != null)
        {
            Vector3 pos = spawnPoint.position;
            pos.y = Mathf.Floor(pos.y)+0.55f;
            transform.position = pos;
            Destroy(spawnPoint.gameObject);
        }
    }
    IEnumerator IRespawn()
    {
        ScreenEffects.FadeScreen(4,false,Color.black);
        rb.velocity = Vector2.zero;
        anim.SetFloat("HorSpeed",0);
        anim.SetTrigger("Respawn");
        DataShare.PlaySound("Player_respawn",transform,false);
        yield return 0;
        yield return new WaitUntil(()=>anim.GetCurrentAnimatorStateInfo(1).normalizedTime >= 0.865f);
        DecPlayerFreeze();
        GameMaster.TimerSubtract = Time.timeSinceLevelLoad;
        respawn = false;
        PauseMenu.allowPause = true;
    }
    void Awake()
    {
        if(godmode) godmode = false;
        GameMaster.superModeTime = 0;

        livesAddEvent = EngageSuperMode;

        freezePlayerInput = (byte)(respawn ? 1 : 0);
        anim = transform.GetChild(0).GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if(respawn) StartCoroutine(IRespawn());
    }
    // Start is called before the first frame update
    void Start()
    {
        // Set lives counter
        SetLives(respawn ? -1 : 0,respawn);

        // Set visuals for bones
        HUD.SetHealthDisplay(currentHealth,false);

        // Set head icon
        HUD.SetHeadState(currentLives == LivesRange.y ? 1 : currentLives == LivesRange.x ? 3 : 0);

        col = GetComponent<BoxCollider2D>();
        GravityScale = rb.gravityScale;
        speedCap = GetComponent<VelocityCap>();
        facing = (int)transform.localScale.x;

        // Assign shooter script
        BulletShooter b = transform.GetChild(0).GetChild(1).GetComponent<BulletShooter>();
        shootDelegate += b.Shoot;
        playerFixedUpdate +=b.AimUpdate;
        b.shootEvent = ShootEvent;
        camTransition = transform.GetChild(1).GetComponent<CamTransition>();
        camControl = GameObject.FindWithTag("MainCamera").GetComponent<CamControl>();

        // Assign renderers
        bodyRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        headRenderer = bodyRenderer.transform.GetChild(0).GetComponent<SpriteRenderer>();
        GunRenderer = bodyRenderer.transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();

        // Set the particle system
        Transform jumpingDust = Instantiate(JumpingDust,bodyRenderer.transform.position,Quaternion.identity).transform;
        jumpingDust.SetParent(bodyRenderer.transform);
        jumpingDust.transform.localPosition = new Vector3(0,-0.3f,0);
        JumpingDustParticleSystem = jumpingDust.GetComponent<ParticleSystem>();

        freezeJump = false;
    }
    void ToggleRenderers(bool toggle)
    {
        bodyRenderer.enabled = toggle;
        headRenderer.enabled = toggle;
        GunRenderer.enabled = toggle;
    }
    bool ShootEvent()
    {
        //anim.Play("Playergun_shoot",0);
        anim.SetTrigger("Shoot");
        return true;
    }
    
    void OnDisable()
    {
        if(GameMaster.superModeTime != 0)
        {
            if(!respawn)
            DataShare.LoadMusic("EmpoweredEnd",false);
            GameMaster.superModeTime = 0;
        }
    }
    IEnumerator IMusicRestore(string musicToRestore)
    {
        yield return new WaitUntil(() => GameMaster.superModeTime<=3);
        DataShare.MusicLooping = false;

        yield return new WaitUntil(() =>!DataShare.MusicIsPlaying);
        DataShare.LoadMusic("EmpoweredEnd",false);

        int i = 10;
        while(i>0)
        {
            i--;
            yield return 0;
        }
        yield return new WaitUntil(() =>!DataShare.MusicIsPlaying);
        DataShare.LoadMusic(musicToRestore);
    }
    void SetRendererMaterial(Material mat)
    {
        bodyRenderer.material = mat;
        headRenderer.material = mat;
        GunRenderer.material = mat;
    }
    public IEnumerator ISuperModeTime(bool resetTimer)
    {
        if(PauseMenu.paused) yield return new WaitUntil(()=>!PauseMenu.paused);

        string savedMusic = DataShare.MusicNowPlaying;

        GameObject flameHeadObj = Instantiate(FlameHead,headRenderer.transform.position,Quaternion.identity);
        flameHeadObj.transform.SetParent(headRenderer.transform);
        flameHeadObj.SetActive(false);

        GameObject energyObj = flameHeadObj.transform.GetChild(0).gameObject;
        Transform energyTr = energyObj.transform;
        energyTr.SetParent(transform.GetChild(0));
        energyTr.localPosition = new Vector3(0.5f,0.5f,0);
        energyTr.localScale = Vector3.one;
        energyObj.SetActive(false);

        anim.SetBool("Hurt",false);
        Material material = bodyRenderer.material;
        Material flashMaterial = GameMaster.self.shapeMaterial;
        
        ToggleRenderers(true);
        // Set timer
        if(resetTimer)
        {
            GameMaster.superModeTime = GameMaster.superModeStartTime;
            DataShare.LoadMusic("");

            // Animation
            rb.constraints =  RigidbodyConstraints2D.FreezeAll;
            freezePlayerInput=1;
            Time.timeScale = 0;
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("Trumpet");

            float targetTime = 0;
            while(targetTime<0.5f)
            {
                targetTime+=Time.unscaledDeltaTime;
                yield return 0;
            }
            targetTime = 0;

            DataShare.PlaySound("Trumpet",transform.position,false);
            SpriteRenderer trumpetTr = Instantiate(TrumpetEffect,transform.position-Vector3.up/2,Quaternion.identity).GetComponent<SpriteRenderer>();
            trumpetTr.flipX = transform.localScale.x < 0;
            SetRendererMaterial(flashMaterial);

            targetTime = 0;
            while(targetTime<0.1f)
            {
                targetTime+=Time.unscaledDeltaTime;
                yield return 0;
            }

            if(jumpsRemaining == 0) jumpsRemaining = 1;
            Time.timeScale = DataShare.GameSpeed;
            SetRendererMaterial(material);
            flameHeadObj.SetActive(true);
            energyObj.SetActive(true);
            SetHP(GameMaster.maxHealth,false);
            rb.velocity = Vector2.zero;
            rb.constraints =  RigidbodyConstraints2D.FreezeRotation;
            DecPlayerFreeze();
        }
        else
        {
            flameHeadObj.SetActive(true);
            yield return 0;
        }
        StartCoroutine(IMusicRestore(savedMusic));
        // Set god mode
        godmode = true;
        HUD.SetHeadState(2);

        DataShare.LoadMusic("Empowered");
        while(GameMaster.superModeTime>0)
        {
            yield return new WaitForSeconds(1f);
            GameMaster.superModeTime--;
            SetLives(-1,true);
        }

        //Revert values
        Destroy(flameHeadObj);
        Destroy(energyObj);
        godmode = false;

        HUD.SetHeadState(0);
        SetHP(GameMaster.maxHealth+1,true);
        hurtCor = StartCoroutine(IBlink());
    }
    void EngageSuperMode(bool resetTimer = true)
    {
        if(transform == null) return;

        if(GameMaster.superModeTime != 0)
        {
            GameMaster.superModeTime = (byte)Mathf.Clamp(++GameMaster.superModeTime,0,GameMaster.superModeStartTime);
            return;
        }
        if(hurtCor != null) StopCoroutine(hurtCor);
        hurtCor = StartCoroutine(ISuperModeTime(resetTimer));
    }
    public static void SetLives(int toAdd,bool animate,bool additive = true)
    {
        toAdd += additive ? currentLives : 0;

        // Super mode start
        if(toAdd > LivesRange.y)
        {
            currentLives = LivesRange.y;
            if(HUD.self != null)
            {
                HUD.SetLivesDisplay(currentLives,false);
                PlayerControl.livesAddEvent?.Invoke(true);
            }
            return;
        }

        if(CheatInput.LockLives && additive)
        {
            HUD.SetLivesDisplay(currentLives,animate);
            return;
        }

        // Pre super mode face
        else if(toAdd == LivesRange.y)
        {
            HUD.SetHeadState(1);
        }

        // Pre crash face/crash
        else if(toAdd <= LivesRange.x)
        {
            if(toAdd == LivesRange.x)
            HUD.SetHeadState(3);
        }
        else
        {
            // return to normal
            if(HUD.headStateInt % 2 == 1
            && toAdd == Mathf.Clamp(toAdd,LivesRange.x+1,LivesRange.y-1))
            HUD.SetHeadState(0);
        }

        currentLives = Mathf.Clamp(toAdd,LivesRange.x,LivesRange.y);
        HUD.SetLivesDisplay(currentLives,animate);
    }
    static void ReloadScene()
    {
        GameMaster.ReloadLevel(respawn);
    }
    public static void Death(byte DeathAmount)
    {
        freezePlayerInput = 1;
        GameMaster.Deaths+=DeathAmount;
        
        if(DeathAmount>=1)
        {
            SetLives(-(DeathAmount-1),false);
        }
        if(currentLives>LivesRange.x || CheatInput.LockLives)
        {
            currentHealth = GameMaster.maxHealth;
            respawn = true;
            PauseMenu.allowPause = false;
            ReloadScene();
        }
        else
        {
            GameMaster.Crash();
        }
    }
    Coroutine hurtCor;
    IEnumerator IBlink()
    {
        float blinkTime = 1f;
        float blinkDuration = 0.075f;
        bool visible = true;
        while(blinkTime>0)
        {
            visible = !visible;
            ToggleRenderers(visible);
            yield return new WaitForSeconds(blinkDuration);
            blinkTime-=blinkDuration;
        }
        ToggleRenderers(true);
        prevPos = Vector2.one*-99;

        hurtCor = null;
        if(LadderVal!=2)
        {
            col.enabled = false;
            col.enabled = true;
        }
    }
    IEnumerator IHurtDur(Vector2 dir)
    {
        UnWall();
        ExitLadderMode(false);

        rb.velocity = new Vector2(Mathf.Clamp(dir.x*100,-1,1)*2,0);
        rb.AddForce(Vector2.up*jumpForce/2,ForceMode2D.Impulse);
        UpdateRoundedVelocity();

        anim.SetBool("Hurt",true);
        DataShare.PlaySound("Player_hit",transform.position,false);
        freezePlayerInput++;
        jumpBuffer = -1;
        jumpThreshold = -1;

        SetHP(CheatInput.OneHitDeaths ? -GameMaster.maxHealth-1 : -1);
        HUD.SetHealthDisplay(currentHealth,true);

        yield return 0;
        
        // Allow player to double jump out of the pain state if they have health
        if(currentHealth >= 0)
        {
            jumpsRemaining = jumpAmount-1;
            yield return new WaitUntil(()=>grounded || roundedVelocity<0);
            if(!grounded) jumpFreezeBypass = true;
            yield return new WaitUntil(()=>grounded || roundedVelocity>0);
        }
        else yield return new WaitUntil(()=>grounded);

        if(currentHealth>=0)
        {
            jumpFreezeBypass = false;
            anim.SetBool("Hurt",false);
            DecPlayerFreeze();
            hurtCor=StartCoroutine(IBlink());
        }
        //Death
        else
        {
            gameObject.layer = 2;
            rb.velocity = new Vector2(0,rb.velocity.y);
            anim.SetBool("Die",true);
            int ID = DataShare.PlaySound("Player_die_pre",transform.position,false);
            yield return 0;
            yield return new WaitUntil(()=>anim.GetCurrentAnimatorStateInfo(1).normalizedTime >= 0.865f);
            DataShare.StopSound(ID);
            CamControl.ShakeCamera(0.5f,0.25f);
            Instantiate(Gibs,transform.position,Quaternion.identity);
            DataShare.PlaySound("Player_die_main",transform.position,false);
            
            if(currentLives>LivesRange.x || CheatInput.LockLives) yield return new WaitForSeconds(2f);
            Death(1);
        }
    }
    IEnumerator IBounce()
    {
        yield return new WaitForSeconds(0.1f);
        hurtCor = null;
    }
    Coroutine wallJumpLock;
    IEnumerator IWallJumpFlipLock()
    {
        byte wait = 10;
        while(wait>0)
        {
            wait--;
            yield return 0;
        }
        wallJump = false;
    }
    public void HurtEvent(Vector2 dir)
    {
        if(hurtCor!=null || godmode || CheatInput.GodMode)return;
        hurtCor = StartCoroutine(IHurtDur(dir));
    }
    public void Flip(int dir)
    {
        if(dir == 0 || wallJump) return;
        Vector3 scale = transform.localScale;
        scale.x = dir;
        transform.localScale = scale;
        facing = dir;
    }
    void ClimbMovement()
    {
        int dir = MGInput.GetDpadYRaw(MGInput.controls.Player.Movement);
        int Xdir = MGInput.GetDpadXRaw(MGInput.controls.Player.Movement);
        if(lockLadderUp && dir == 1)
        {
            dir = 0;
        }
        if(Xdir!=0) Flip(Xdir);
        rb.velocity = Vector2.zero;
        if(dir!=0)
        {
            if(roundedVelocity != 0 && loopingSoundID == -1)
                loopingSoundID = DataShare.PlaySound("Ladder_climb_loop",transform,true);

            rb.AddForce(Vector2.up*dir*climbSpeed,ForceMode2D.Impulse);

            if(roundedVelocity == 0)
            {
                StoploopingSound();
            }
        }
        else if(dir == 0) StoploopingSound();

    } 
    void XMovement(Vector2 velo,int xPos)
    {
        if(!wallJump && Mathf.Abs(rb.velocity.x)<0.1f)currentSpeed = 0;
        rb.velocity = new Vector2(0,velo.y);

        //If on the ground and starting to move from a standstill, increase player speed slower.
        if(!slowSpeedup && grounded && currentSpeed == 0 && xPos != 0 && lastXpos == 0)
            slowSpeedup = true;

        else if(slowSpeedup && (!grounded || xPos != lastXpos))
            slowSpeedup = false;

        if(xPos!=0)
        {
            float curSpeedAdditive = speedAdditive;
            curSpeedAdditive /= (slowSpeedup) ? 2 : 1;

            Vector2 HorizontalSpeed = speedCap.HoritzontalCap;
            currentSpeed = Mathf.Clamp(currentSpeed+xPos*(wallJump ? wallJumpPressedSpeedAdditive : curSpeedAdditive),HorizontalSpeed.x,
            HorizontalSpeed.y);
        }
        else
        {
            float usedAdditive = grounded ? speedAdditive : wallJump ? wallJumpSpeedAdditive : airSpeedAdditive;
            currentSpeed = Mathf.MoveTowards(currentSpeed,0,usedAdditive);
        }
        Vector2 force = Vector2.right*currentSpeed;
        force.y = 0;
        rb.AddForce(force,ForceMode2D.Impulse);
    }
    public void Bounce()
    {
        float divider = MGInput.GetButton(MGInput.controls.Player.Jump) ? 1 : 1.25f;
        rb.velocity = new Vector2(rb.velocity.x,0);
        rb.AddForce(Vector2.up*jumpForce/divider,ForceMode2D.Impulse);
        UpdateRoundedVelocity();
        jumpsRemaining = jumpAmount-1;

        if(hurtCor!=null)return;
        hurtCor = StartCoroutine(IBounce());
    }
    void DecJumpsRemaining()
    {
        jumpsRemaining--;
        if(jumpsRemaining == 0 && GameMaster.superModeTime!=0)
        jumpsRemaining = 1;
    }
    void Jump(Vector2 velo,bool automatic)
    {
        // Whether the jump action is allowed
        bool JumpPress = (automatic || (MGInput.GetButtonDown(MGInput.controls.Player.Jump) || jumpBuffer > 0));

        // Whether the jump should be only buffered
        if(JumpPress)
        {
            ///print("Jump press: "+JumpPress);
            if(freezeJump || (!grounded && jumpsRemaining <= 0 && !walled))
            {
                ///print("Jump buffer only");
                JumpPress = false;
            }
            if(jumpBuffer<=0) jumpBuffer = 0.15f;
        }

        if(jumpThreshold>0)
        {
            jumpThreshold-=Time.deltaTime;
            if(jumpsRemaining > 0 && jumpThreshold <= 0 && roundedVelocity < 0) DecJumpsRemaining();
        }

        if(jumpAmount > 0 && JumpPress && grounded)
        {
            if(jumpsRemaining != jumpAmount) SetGround(false);
            DecJumpsRemaining();
        }

        // Jumping element
        if(jumpBuffer >= 0 && JumpPress)
        {
            bool wasOnLadder = LadderVal == 2;
            bool jumpSound = false;
            if(!grounded)
            {
                if(jumpsRemaining != jumpAmount && jumpThreshold <= 0 && !walled && !wasOnLadder)
                {
                    anim.SetTrigger("DoubleJump");
                    jumpSound = true;
                    DataShare.PlaySound("Player_doublejump",transform,false);
                }
                DecJumpsRemaining();
            }
            
            ExitLadderMode(false);
            camTransition.ResetBoxCollider();

            jumpThreshold = -1;
            jumpBuffer = -1;

            rb.velocity = new Vector2(velo.x,0);
            UnGround();
            if(jumpBuffer >= 0 && jumpsRemaining == jumpAmount)
            {
                DecJumpsRemaining();
            }

            if(!jumpSound) DataShare.PlaySound("Player_jump",transform,false);

            if(wasOnLadder) ladderBuffer = 0.25f;
            //Fall through
            if(wasOnLadder && MGInput.GetDpadYRaw(MGInput.controls.Player.Movement)==-1)
            {
                FallThroughSS(!wasOnLadder);
            }
            else
            {
                jumpEvent?.Invoke();
                rb.AddForce(Vector2.up*jumpForce,ForceMode2D.Impulse);
                UpdateRoundedVelocity();
            }
            wallJump = false;
            
            if(walled)
            {
                ///print("Wall jump");
                wallJump = true;
                jumpsRemaining = jumpAmount-1;
                Vector2 HorizontalSpeed = speedCap.HoritzontalCap;
                currentSpeed = Mathf.Clamp(currentSpeed-walledSide*wallJumpStrength,HorizontalSpeed.x,
                HorizontalSpeed.y);
                UnWall();
                if(wallJumpLock!=null) StopCoroutine(wallJumpLock);
                wallJumpLock = StartCoroutine(IWallJumpFlipLock());
            }
            ShowParticles(0);
        }
        
        // Jump release
        if(roundedVelocity>0 && MGInput.GetButtonUp(MGInput.controls.Player.Jump))
        {
            velo = rb.velocity;
            velo.y/=jumpReleaseReduct;
            rb.velocity = velo;
        }
        if(jumpBuffer>=0)
        {
            if(!grounded) jumpBuffer-=Time.deltaTime;
            else jumpBuffer = -1;
        }
    }   
    void ShootControl()
    {
        if(MGInput.GetButton(MGInput.controls.Player.Shoot))
        {
            shootDelegate?.Invoke();
        }
    }
    Coroutine LadderTopCollisionControlCor;
    IEnumerator ILadderTopCollisionControl(float Ypos)
    {
        float Y = transform.position.y;
        Ypos-=2f;
        Physics2D.IgnoreCollision(col,ssCol,true);
        yield return 0;
        if(transform.position.y == Y && LadderVal == 0)
        {
            Physics2D.IgnoreCollision(col,ssCol,false);
            yield break;
        }
        yield return new WaitUntil(()=>transform.position.y<Ypos||transform.position.y>Ypos+3);
        Physics2D.IgnoreCollision(col,ssCol,false);
    }
    CompositeCollider2D ssCol;
    void FallThroughSS(bool startCor)
    {
        if(ssCol==null)ssCol = GameObject.FindWithTag("SSMap").GetComponent<CompositeCollider2D>();
        //Revert collision when player is low enough
        if(LadderTopCollisionControlCor!=null)StopCoroutine(LadderTopCollisionControlCor);
        if(startCor)LadderTopCollisionControlCor = StartCoroutine(ILadderTopCollisionControl(transform.position.y));
        else Physics2D.IgnoreCollision(col,ssCol,false);
    }
    void LadderMode()
    {
        if(ladderBuffer>0 && LadderVal == 1) return;
        int axis = MGInput.GetDpadYRaw(MGInput.controls.Player.Movement);
        if(grounded && LadderVal == 1 && axis == -1) return;

        if((LadderVal == 3 && axis == 1) || axis == 0) return;

        if(LadderVal == 3)
        {
            FallThroughSS(true);
        }

        LadderVal = 2;
        ladderBuffer = 0.25f;
        anim.SetBool("OnLadder",true);

        // Position to snap to
        Vector3 position = transform.position;
        position.x = Mathf.Ceil(position.x)-0.5f;
        rb.MovePosition(position);

        // Freeze velocity
        rb.gravityScale = 0;
        currentSpeed = 0;
        rb.velocity = Vector2.zero;

        // Values to assign
        jumpsRemaining = jumpAmount;
    }
    void ExitLadderMode(bool floorBelow)
    {
        if(LadderVal==0)return;

        if(LadderVal == 2)
        {
            StoploopingSound();
            bool pressingDown = MGInput.GetDpadYRaw(MGInput.controls.Player.Movement)==-1;
            anim.SetBool("OnLadder",false);
            
            // Round pos
            Vector3 pos = transform.position;

            pos.y = Mathf.Round(pos.y)-(!pressingDown ? 0 : 0.085f); // A fixed value resulting from unity's physics calculations, this might cause problems if your collider is a custom size.
            transform.position = pos;

            jumpsRemaining = jumpAmount-1;
            // Restore gravity
            rb.gravityScale = GravityScale;
            if(!pressingDown)
            {
                rb.velocity = Vector2.zero;
            }
            FallThroughSS(false);
        }
        LadderVal = floorBelow ? 3 : 0;
    }
    void UnWall()
    {
        walled = false;
        if(!grounded) jumpThreshold = 0.05f;
        anim.SetBool("WallSlide",walled);
        StoploopingSound();
        StopParticles();
    }
    void StoploopingSound()
    {
        if(loopingSoundID == -1) return;
        DataShare.StopSound(loopingSoundID);
        loopingSoundID = -1;
    }

    bool downPressed = false;
    void DoubleTapDown()
    {
        if(MGInput.GetDpadYRaw(MGInput.controls.Player.Movement) != -1)
        {
            downPressed = false;
            return;
        }
        if(!downPressed)
        {
            downPressed = true;
            if(downDoubleTapDelay<=0)downDoubleTapDelay = 0.2f;
            else FallThroughSS(true);
        }
    }
    void CheckInput()
    {
        if(MGInput.controls == null) return;

        Vector2 velo = rb.velocity;
        xPos = freezePlayerInput == 0 ? MGInput.GetDpadXRaw(MGInput.controls.Player.Movement) : 0;

        if(LadderVal!=2)
        {
            XMovement(velo,xPos);
            lastXpos = xPos;
        }
        else ClimbMovement();

        if(freezePlayerInput == 0 || jumpFreezeBypass)
        {
            Jump(velo,false);

            if(freezePlayerInput != 0) return;

            //LadderVal == 1 or 3
            if(LadderVal%2==1) LadderMode();

            ShootControl();

            // Jumping down semi solid platforms is not a bad feature.
            ///if(grounded && onSSGround) DoubleTapDown();
        }
    }
    void UpdateRoundedVelocity()
    {
        roundedVelocity = Mathf.Round(rb.velocity.y * 10) / 10;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(!PauseMenu.paused)
        {
            CheckInput();
            playerFixedUpdateControl?.Invoke(LadderVal == 2,grounded || walled);
            if(freezePlayerInput != 0) return; 
            playerFixedUpdate?.Invoke();
        }
    }
    void LateUpdate()
    {
        if(MGInput.GetButtonDown(MGInput.controls.Player.Pause))
        {
            PauseMenu.Pause(!PauseMenu.paused);
        }
        if(PauseMenu.paused) return;
        // Disable grounded when in air
        UpdateRoundedVelocity();
        float deltaTime = Time.deltaTime;
        if(ladderBuffer>0)
        {
            ladderBuffer-=deltaTime;
        }
        if(downDoubleTapDelay>0)downDoubleTapDelay-=deltaTime;

        // When walking off a platform, apply a threshold in which jump button can still be pressed without double jumping
        if(grounded && roundedVelocity!=0)
        {
            if(roundedVelocity<0)
            {
                if(roundedVelocity>0)
                {
                    Time.timeScale = 0;
                }
                jumpThreshold = 0.1f;
            }
            UnGround();
        }
        else if(!grounded && walled && roundedVelocity == 0)
        {
            SetGround(true);
        }
        if(roundedVelocity<0 && !CamControl.midTransition && OutOfBoundsCheckCor == null)
        {
            wallJump = false;
            CheckOutOfBounds(false);
        }
        anim.SetFloat("HorSpeed",Mathf.Abs(rb.velocity.x));
        anim.SetFloat("VerSpeed",roundedVelocity);

        // Set facing direction
        if(freezePlayerInput != 0) return;

        // Flip player
        int XDir = MGInput.GetDpadXRaw(MGInput.controls.Player.Movement);
        if(XDir!=0)
        {
            if(walled)
            {
                float roundedX = Mathf.Round(rb.velocity.x * 10) / 10;
                if(roundedX != 0)
                {
                    int newDir = rb.velocity.x > 0 ? 1 : -1;
                    if(newDir != facing)
                    {
                        Flip(newDir);
                    }
                }
            }
            else
            {
                Flip(XDir);
            }
        }
    }
    
    Coroutine OutOfBoundsCheckCor;
    IEnumerator IOutOfBoundsSecondCheck()
    {
        yield return new WaitForSeconds(0.25f);
        print("Second check");
        camTransition.CompareGridPos(transform.position);
        CheckOutOfBounds(true);
        yield return 0;
        OutOfBoundsCheckCor = null;
    }
    
    void CheckOutOfBounds(bool kill)
    {
        Vector3 pos = transform.position;
        float offset = 0;

        // Second check is for true position
        if(!kill && pos.y>camTransition.GetCameraY())
        {
            offset = -10;
        }
        if((kill && !camTransition.CheckInBounds(pos+Vector3.up*offset))|| (!kill && !camTransition.CompareGridPos(pos+Vector3.up*offset)))
        {
            // Check if in acceptable tile, if not determine out of bounds and die
            if(!camControl.Evaluate(camTransition.GetGridPos))
            {
                if(!kill)
                {
                    print("Out of bounds 1");
                    OutOfBoundsCheckCor = StartCoroutine(IOutOfBoundsSecondCheck());
                }
                else
                {
                    if(CamControl.midTransition) return;
                    print("Out of bounds 2");
                    Death(1);
                }                
            }
        }
    }
    void UnGround()
    {
        grounded = false;
        anim.SetBool("Grounded",grounded);
        //onSSGround = false; 
    }
    void SetGround(bool exitLadder)
    {
        if(!grounded && Time.timeSinceLevelLoad > 0.1f && LadderVal == 0) DataShare.PlaySound("Player_land",transform.position,false);
        PlayerControl.freezeJump = false;
        prevPos = Vector2.one*-99;

        UnWall();
        wallJump = false;
        walledSide = 0;

        if(LadderVal == 0) ShowParticles(0);
        if(exitLadder) ExitLadderMode(false);

        grounded = true;
        //onSSGround = true;
        jumpsRemaining = jumpAmount;
        anim.ResetTrigger("DoubleJump");
        anim.SetBool("Grounded",grounded);
    }
    void GroundDetect(ContactPoint2D[] points,Vector3 pos,string groundTag)
    {
        //if(groundTag.Contains("ss"))onSSGround = true;
        ///else onSSGround = false;

        if(grounded)
        {
            return;
        }
        foreach(ContactPoint2D c in points)
        {
            if(c.point.y>pos.y-0.9f)
            {
                if(points.Length==2)
                break;
                else continue;
            }
            if(DebugLineDraw)
            {
                ///print(c.point);
                Debug.DrawLine(pos,c.point,Color.red,1f);
            }
            SetGround(true);
            break;
        }
    }
    void WallDetect(ContactPoint2D[] points, Vector3 pos)
    {
        //walled = sign(x) = sign(x) and abs(y - y) > 1
        if(points.Length<=1)
        {
            UnWall();
            return;
        }
        if(!walled)
        {
            Vector2[] positions = new Vector2[points.Length];
            for(int i = 0;i<positions.Length;i++)
            {
                positions[i] = points[i].point-(Vector2)pos;
                if(DebugLineDraw)
                {
                    ///print(positions[i]);
                    Debug.DrawLine(pos,(Vector2)pos+positions[i],Color.green,1f);
                }
            }
            int wallDir = (int)Mathf.Sign(positions[0].x);
            if(!DataShare.autoSlide && lastXpos != wallDir)
            {
                return;
            }
            ///print(positions[0].x+" + "+positions[1].x+" = "+(positions[0].x + positions[1].x));
            walled = (Mathf.Sign(positions[0].x) == Mathf.Sign(positions[1].x) && Mathf.Abs(positions[0].y - positions[1].y) > 0.5f) ? true : false;
            if(walled && !grounded)
            {
                if(roundedVelocity < 0)
                {
                    DataShare.PlaySound("Player_walljump_start",transform.position,false);
                    loopingSoundID = DataShare.PlaySound("Player_walljump_loop",transform,true);
                    //print("Fall velocity: "+roundedVelocity);
                    walledSide = wallDir;
                    jumpsRemaining = jumpAmount;
                    Flip(-walledSide);
                    ShowParticles(-1);
                    anim.SetBool("WallSlide",walled);
                }
            }
        }
    }
    int collisionCount = 0;
    void TileResponse(ContactPoint2D[] points, string otherTag)
    {
        collisionCount = points.Length;
        //Wall collision: if 2 collisions have the same X value or there's more than 2 collisions

        Vector2 pos = transform.position;
        int Length = points.Length<2 ? points.Length : 2;
        float subTilePos = Mathf.Repeat(pos.x,1);
        int flipDir = -1;

        //Floor them first, then operate on those values
        Vector2 pointPos = new Vector2(Mathf.Round(points[0].point.x-0.5f),Mathf.Floor(points[0].point.y));
        Vector2 pointPos2 = pointPos;
        Vector2 playerPos = pointPos;
        if(collisionCount>1) pointPos2 = new Vector2(Mathf.Round(points[1].point.x-0.5f),Mathf.Floor(points[1].point.y));

        #if UNITY_EDITOR
        if(DebugLineDraw)
        {
            Debug.DrawLine(pos,pointPos,Color.cyan);
            Debug.DrawLine(pos,pointPos,Color.green);
        }
        #endif

        Vector2 offset = Vector2.zero;

        //Wall collision, if vertical distance between points is 1 tile
        if(Mathf.Abs(pointPos.y - pointPos2.y)>=1)
        {
            offset.x = subTilePos >= 0.5f ? 1 : -1;
            flipDir = 1;

            //More collisions case
            if(collisionCount>2)
            {
                if(subTilePos >= 0.5f)
                {
                    pointPos2 += new Vector2(-1,-2);
                }

                else
                {
                    pointPos += new Vector2(1,-2);
                }
            }
        }
        //Ground collision, when y values are equal
        else if(pointPos.y == pointPos2.y)
        {
            offset.y = pos.y-points[0].point.y>0 ? -1 : 1;

            //More collisions case
            if(collisionCount>3)
            {
                pointPos2 += new Vector2(subTilePos >= 0.5f ? 1 : -1,1);
            }
            else
            {
                pointPos.x = Mathf.Round(pos.x-0.5f);
                pointPos2.x = pointPos.x;
            }
        }

        // Add offsets
        pointPos += offset;
        pointPos2 += offset;

        if(walled)
        {
            pointPos.x = playerPos.x - facing;
            pointPos2.x = pointPos.x;

            if(pointPos2.y+pointPos.y>1)
            {
                if(pointPos.y<pointPos2.y)
                pointPos2.y = pointPos.y+1;
                else pointPos2.y = pointPos.y-1;
            }
        }

        // Take damage
        Vector2 PlayerTilePos = playerPos;

        int firstResult = GameMaster.GetTileResult(PlayerTilePos,new Vector3Int((int)pointPos.x,(int)pointPos.y,0),otherTag);

        if(firstResult == 0) // If standing on edge of tile, check tile next to it
        {
            int multiplier = pos.x - Mathf.Floor(pos.x) > 0.5f ? 1 : -1;
            if(!walled)
            {
                pointPos2 += Vector2.right * multiplier;
            }
            flipDir = multiplier * -(int)transform.localScale.x;
        }
        int secondResult = -1;
        if(firstResult != 1)
        {
            secondResult = GameMaster.GetTileResult(walled ? new Vector2(playerPos.x,pointPos2.y) : PlayerTilePos,new Vector3Int((int)pointPos2.x,(int)pointPos2.y,0),otherTag);
            if(secondResult == 0)
            {
                pointPos2.y+=1;
                secondResult = GameMaster.GetTileResult(walled ? new Vector2(playerPos.x,pointPos2.y) : PlayerTilePos,new Vector3Int((int)pointPos2.x,(int)pointPos2.y,0),otherTag);
            }
        }

        if(firstResult == 1)
        {
            HurtEvent(Vector2.right * transform.localScale.x * flipDir);
        }
        else if(pointPos2 != pointPos && secondResult == 1)
        {
            HurtEvent(Vector2.right * transform.localScale.x * flipDir);
        }

        #if UNITY_EDITOR
        if(DebugLineDraw)
        {
            Debug.DrawLine(pos,pointPos+(Vector2.one/2),Color.yellow);
            Debug.DrawLine(pos,pointPos2+(Vector2.one/2),Color.cyan);
        }
        #endif
    }
    void OnCollisionStay2D(Collision2D other)
    {
        ContactPoint2D[] points = other.contacts;
        if(roundedVelocity<=0)
        {
            Vector3 pos = transform.position;
            if(!grounded)
            {
                if(!other.transform.name.Contains("SS") && freezePlayerInput == 0 && roundedVelocity<0)
                WallDetect(points,pos);
            }
            if(points.Length>=2)
            GroundDetect(points,pos,other.gameObject.tag.ToLower());
        }
        Vector2 curRoundedPos = (Vector2)transform.position;
        //curRoundedPos.x = Mathf.Round(curRoundedPos.x-0.5f);
        curRoundedPos.y = Mathf.Round(curRoundedPos.y);
        if(prevPos==curRoundedPos || hurtCor!=null) return;
        
        if(other.gameObject.tag != "SSMap" ||(other.gameObject.tag == "SSMap" && grounded))
        TileResponse(points,other.gameObject.tag);

        prevPos = curRoundedPos;
    }
    void OnCollisionExit2D(Collision2D other)
    {
        collisionCount = 0;
        if(walled)
        {
            UnWall();
        }
        //if(other.gameObject.tag != "SSMap") onSSGround = false;
    }  
    void OnTriggerStay2D(Collider2D other)
    {
        if(LadderVal == 0 && other.tag=="Climbable" && ladderBuffer<=0)
        {
            Vector3 pos = transform.position;
            Vector3 ladderPos = other.ClosestPoint(pos);
            pos.x = ladderPos.x;
            float distance = Mathf.Round(Vector3.Distance(pos,ladderPos) * 10) / 10;
            LadderVal = distance>0.8f ? 3 : 1;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag=="Climbable")
        {
            if(MGInput.GetDpadYRaw(MGInput.controls.Player.Movement) != 1)
            {
                ExitLadderMode(false);
            }
            else if(LadderVal == 2)
            {
                Vector3 playerPos = transform.position;
                //Check if ground below, unladder if so
                RaycastHit2D hit = Physics2D.Raycast(playerPos,Vector2.down,1.2f,128);
                if(hit.collider!=null)
                {
                    Vector2 pos = hit.point;
                    pos.x = playerPos.x;
                    pos.y = hit.point.y+1;
                    transform.position = pos;
                    ExitLadderMode(true);

                }

                else
                {
                    lockLadderUp = true;
                }
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Climbable")
        {
            lockLadderUp = false;
        }
    }
    #if UNITY_EDITOR
    void OnGUI()
    {
        if(!GameMaster.DebugInfo) return;
        GUI.Label(new Rect(10, 10+GUIDataOffset, 300, 20), "Press P to toggle debug info.");
        GUI.Label(new Rect(10, 25+GUIDataOffset, 300, 20), "Velo: "+rb.velocity + " RoundedY: "+roundedVelocity+", Speed: "+currentSpeed);
        GUI.Label(new Rect(10, 40+GUIDataOffset, 200, 20), "Grounded: "+grounded);//+(onSSGround?", Semi-Solid":""));
        GUI.Label(new Rect(10, 55+GUIDataOffset, 100, 20), "Walled: "+walled);
        GUI.Label(new Rect(10, 70+GUIDataOffset, 150, 20), "Wall jump: "+wallJump);
        GUI.Label(new Rect(10, 85+GUIDataOffset, 200, 20), "Wall side: "+walledSide);
        GUI.Label(new Rect(10, 100+GUIDataOffset, 100, 20),"Jumps: "+jumpsRemaining);
        GUI.Label(new Rect(10, 115+GUIDataOffset, 300, 20),"Jump buffer: "+jumpBuffer);
        GUI.Label(new Rect(10, 130+GUIDataOffset, 300, 20),"Jump threshold: "+jumpThreshold);
        GUI.Label(new Rect(10, 145+GUIDataOffset, 200, 20),"Ladder val: "+LadderVal + (lockLadderUp ? ", locked moving up" : ""));
        GUI.Label(new Rect(10, 160+GUIDataOffset, 200, 20),"Ladder buffer: "+ladderBuffer);
        GUI.Label(new Rect(10, 175+GUIDataOffset, 300, 20),"Double tap down buffer: "+downDoubleTapDelay);
        GUI.Label(new Rect(10, 190+GUIDataOffset, 300, 20),"Collision count: "+collisionCount);
        GUI.Label(new Rect(10, 205+GUIDataOffset, 300, 20),"Slow speedup: "+slowSpeedup);
    }
    #endif
}
