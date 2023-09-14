using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[System.Serializable]
// Script controls attack behaviour and which attacks are performed
public class BossGlobalStats : EnemyGlobalStats
{
    #region vars
    protected delegate void EarlyStartBattleCallback();
    protected EarlyStartBattleCallback earlyStartBattleCallback;
    protected delegate void StartBattleCallback();
    protected StartBattleCallback startBattleCallback;
    protected delegate void EndBattleCallback(Vector2 bulletPos);
    protected EndBattleCallback endBattleCallback;
    protected delegate void PostDeathCallback();
    protected PostDeathCallback postDeathCallback;

    [Space]
    [Header("Boss variables")]
    [Space]
    [SerializeField] protected GameObject GoalObject;
    [SerializeField] protected DialogueSystem dialogueSystem;
    [SerializeField] protected string BossFightMusic;
    public UnityEvent[] attacks;
    AttackPickerRandom attackPickerRandom;
    protected EnemyHit enemyHit;
    Coroutine SetStateReturn;
    Coroutine CurAtkCoroutine;
    [SerializeField] protected float passiveAttackDelay = 0;

    [Space]
    [Header("Death effects")]
    [Space]
    // Effects
    [SerializeField] protected GameObject explosion;
    [SerializeField] GameObject invertColors;
    [SerializeField] GameObject ringStrike;
    [SerializeField] protected float explosionLoopRadius = 0.3f;
    [Tooltip("How much the sprite should offset during death explosions")]
    [SerializeField] protected Vector2 shakeRadius = new Vector2(1,1);
    [Space]
    [Header("UI")]
    [Space]
    // UI
    [SerializeField] GameObject BossBarObject;
    BossHealthbar bossHealthbar;
    [SerializeField] Sprite[] bossHealthBarIcons = new Sprite[2];

    // For damage
    [Space]
    [Header("Damage")]
    [Space]
    [SerializeField] Material[] materials = new Material[2];
    protected SpriteRenderer spriteRenderer;
    byte blinkVal;
    public const byte maxBlinkVal = 2;

    protected int dir = 1;
    protected Transform target;
    protected int loopSoundID = -1;

    // For custom events after boss convo
    public delegate void CustomStartBattleCallback();
    public CustomStartBattleCallback customStartBattleCallback;
    #endregion
    #region main
    void ExecuteCustomDelegate()
    {
        customStartBattleCallback?.Invoke();
        dialogueSystem.postConvoEvent -= ExecuteCustomDelegate;
    }

    protected void ToggleFlipping(bool toggle)
    {
        CancelInvoke("FacePlayer");
        if(toggle) InvokeRepeating("FacePlayer",0,0.3f);
    }
    void Awake()
    {
        attackPickerRandom = GetComponent<AttackPickerRandom>();
        target = GameObject.FindWithTag("Player").transform;
        GetComponent<GenericAdapter>().hasHitAnimation = false;
        enemyHit = transform.GetChild(0).GetComponent<EnemyHit>();
        enemyHit.canBeTouched = false;
        enemyHit.posthitEvent += BossHitEvent;
        spriteRenderer = enemyHit.transform.GetComponent<SpriteRenderer>();
        materials[0] = spriteRenderer.material;
        Init();

        if(GoalObject != null) GoalObject.SetActive(false);

        else
        {
            // Attempt to find a goal object on scene
            Debug.Log("No goal object assigned to boss.");
            GoalObject = GameObject.Find("GoalBoss");
            if(GoalObject == null) Debug.Log("No boss unlocking goal on scene.");
        }
    }
    IEnumerator IStartDelay()
    {
        yield return new WaitUntil(() => Active);
        if(dialogueSystem == null || PlayerControl.respawn)
        {
            if(customStartBattleCallback!=null)
            {
                PlayerControl.freezePlayerInput++;
                enemyHit.canBeTouched = true;
                enemyHit.canTakeDamage = false;
                customStartBattleCallback.Invoke();
            }

            else StartBattle();
        }
        else
        {
            if(customStartBattleCallback == null)
            {
                dialogueSystem.postConvoEvent = StartBattle;
            }

            else
            {
                PlayerControl.freezePlayerInput++;
                enemyHit.canBeTouched = true;
                enemyHit.canTakeDamage = false;
                if(dialogueSystem != null)
                {
                    dialogueSystem.postConvoEvent = ExecuteCustomDelegate;
                }
            }

            dialogueSystem.gameObject.SetActive(true);
            DialogueSystem.StartConvo(0);
        }
        if(!PlayerControl.respawn && DataShare.MusicNowPlaying != BossFightMusic) DataShare.LoadMusic("");

    }
    protected override void Start()
    {
        base.Start();
        StartCoroutine(IStartDelay());
    }
    protected void FacePlayer()
    {
        Flip(transform.position.x>=target.position.x ? 1 : -1);
    }
    protected virtual void Flip(int dir)
    {
        if(dir == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = dir;
        transform.localScale = scale;
        this.dir = dir;
    }
    void HitBlink()
    {
        blinkVal++;
        spriteRenderer.material = materials[blinkVal%2==0?0:1];
        if(blinkVal>=maxBlinkVal)CancelInvoke("HitBlink");
    }
    protected void StartCor(IEnumerator Ienumerator)
    {
        StopCor();
        CurAtkCoroutine = StartCoroutine(Ienumerator);
    }
    protected void StopCor()
    {
        if(CurAtkCoroutine!=null) StopCoroutine(CurAtkCoroutine);
    }
    // Hit behaviour
    void BossHitEvent(bool lethal, Vector2 bulletPos)
    {
        ///print("HP: "+HP+" Current HP: "+currentHP);
        blinkVal = 0;
        CancelInvoke("HitBlink");
        InvokeRepeating("HitBlink",0,0.1f);
        float value = (float)(HP-currentHP)/HP;

        // Destroy health bar
        if(currentHP<=0)
        {
            DataShare.PlaySound("BossBar_Break",false,0.2f,1);
            EndBattle(bulletPos);
            bossHealthbar.Outro(10f);
        }
        else
        {
            bossHealthbar.BarAnim(1-value,10f);
        }
    }
    public void StartBattle()
    {
        if(dialogueSystem != null) dialogueSystem.postConvoEvent -= StartBattle;
        StartCoroutine(IStartBattle());
    }
    public void EndBattle(Vector2 bulletPos)
    {
        DataShare.LoadMusic("");
        if(loopSoundID != -1)
        {
            DataShare.StopSound(loopSoundID);
        } 
        lockSpawn = true;
        enemyHit.canBeTouched = false;
        if(SetStateReturn != null)StopCoroutine(SetStateReturn);
        StopAllCoroutines();
        StartCoroutine(IFinalStrike(bulletPos));

        // Destroy all enemy bullets
        BulletMovement[] bulletMovements = FindObjectsOfType<BulletMovement>();
        foreach (BulletMovement bullet in bulletMovements)
        {
            if(bullet.tag == "EnemyBullet")
            {
                bullet.DisableBullet(false,null,true,false,false,false);
            }
        }
        StartCoroutine(IEndBattle(bulletPos));
        
    }
    // Perform next attack
    public void NextAttack()
    {
        attacks[attackPickerRandom.GetNextAttack(true)]?.Invoke();
    }
    public virtual void NextAttack(bool increment)
    {
        attacks[attackPickerRandom.GetNextAttack(increment)]?.Invoke();
    }

    // If coroutine is null, animation restore ended
    public bool AnimFinished()
    {
        return SetStateReturn == null;
    }
    // Time based variant
    public bool AnimFinishedTime(int layer)
    {
        return anim.GetCurrentAnimatorStateInfo(layer).normalizedTime >= 1;
    }

    // Set anim state and keep it
    public void SetAnimState(int state, int layer)
    {
        ///print("Anim set state "+state);
        anim.SetInteger("Status",state);
    }
    // Set anim state and later set it to 0
    public void SetAnimStateAndReturn(int state, int layer)
    {
        ///print("Anim set state and return "+state);
        if(SetStateReturn!=null) StopCoroutine(SetStateReturn);
        SetStateReturn = StartCoroutine(IShortSetState(state,layer,false));
    }
    // Set anim state and later set it to previous state
    public void SetAnimStateAndReturn(int state, int layer, bool restore)
    {
        if(SetStateReturn!=null) StopCoroutine(SetStateReturn);
        SetStateReturn = StartCoroutine(IShortSetState(state,layer,restore));
    }
    #endregion
    #region IEnumerators
    // Brief hit pause
    /*IEnumerator IHitBlink()
    {
        Time.timeScale = 0;
        if(hitPauseTime>0.02f)
        {
            float progress = 0;
            while(progress < hitPauseTime)
            {
                progress+=Time.unscaledDeltaTime;
                yield return 0;
            }
        }
        else yield return 0;
        Time.timeScale = DataShare.GameSpeed;
    }*/
    IEnumerator IShortSetState(int state,int layer, bool restore)
    {
        int lastState = restore ? anim.GetInteger("Status") : 0;
        anim.SetInteger("Status",state);
        int counter = 5;
        while(counter>0)
        {
            counter--;
            yield return 0;
        }
        yield return new WaitUntil(()=> AnimFinishedTime(layer));
        anim.SetInteger("Status",lastState);
        SetStateReturn = null;
    }
    IEnumerator IStartBattle()
    {
        PlayerControl.freezePlayerInput++;
        yield return 0;
        DataShare.LoadMusic(BossFightMusic);
        //  Spawn health bar
        bossHealthbar = Instantiate(BossBarObject).GetComponent<BossHealthbar>();
        bossHealthbar.Init(bossHealthBarIcons);
        DataShare.PlaySound("BossBar_Appear",false,0.2f,1);

        // Invoke setup boss behaviour
        earlyStartBattleCallback?.Invoke();

        yield return new WaitForSeconds(1f);
        PlayerControl.DecPlayerFreeze();
        GameMaster.TimerSubtract = Time.timeSinceLevelLoad;

        // Invoke boss behaviour
        startBattleCallback?.Invoke();
        enemyHit.canBeTouched = true;
    }
    protected virtual IEnumerator IEndBattle(Vector2 bulletPos)
    {
        PlayerControl.godmode = true;
        yield return 0;
        yield return new WaitUntil(()=> Time.timeScale != 0);

        endBattleCallback?.Invoke(bulletPos);
        anim.SetInteger("Status",2);

        Transform tr = spriteRenderer.transform;
        ParticleSystem particleSystem = Instantiate(explosion,tr.position,Quaternion.identity).GetComponent<ParticleSystem>();
        Transform particleSystemTransform = particleSystem.transform;
        particleSystemTransform.SetParent(transform);
        var shape = particleSystem.shape;
        shape.radius = explosionLoopRadius;
        particleSystem.Play(false);
        var emission = particleSystem.emission;
        yield return 0;
        Shake shake = tr.gameObject.AddComponent<Shake>();
        shake.Set(shakeRadius.x,shakeRadius.y,1f/emission.rateOverTime.constant/2f);
        yield return new WaitForSeconds(3f);
        Destroy(shake);
        particleSystem.Stop();

        // Big Explosion
        yield return new WaitForSeconds(0.75f);
        particleSystem = particleSystem.transform.GetChild(0).GetComponent<ParticleSystem>();
        particleSystem.Play(false);
        particleSystemTransform.SetParent(null);
        CamControl.ShakeCamera(0.6f,0.35f);
        yield return new WaitForSeconds(0.1f);
        if(postDeathCallback!=null)
        {
            postDeathCallback.Invoke();
        }
        else gameObject.SetActive(false);

        if(GoalObject != null)
        {
            if(GoalObject.scene != gameObject.scene)
            {
                GameObject obj = Instantiate(GoalObject);
                GoalObject = obj;
            }
            GoalObject.SetActive(true);
        }
    }
    IEnumerator IFinalStrike(Vector2 bulletPos)
    {
        PlayerControl.freezePlayerInput++;
        Time.timeScale = 0;
        Volume volume = Instantiate(invertColors,Vector3.zero,Quaternion.identity).GetComponent<Volume>();
        volume.gameObject.SetActive(true);
        Instantiate(ringStrike,bulletPos,Quaternion.identity);
        DataShare.PlaySound("Boss_finalhit",transform.position,false);
        
        VolumeProfile profile = volume.profile;
        profile.TryGet<ChromaticAberration>(out var ch);

        float progress = 0;
        float speed = 8f;
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*speed;
            ch.intensity.value = Mathf.Lerp(0,0.6f,progress);
            yield return 0;
        }
        progress = 0;
        speed = 1.5f;
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*speed;
            ch.intensity.value = Mathf.Lerp(0.6f,0,progress);
            yield return 0;
        }
        yield return 0;

        progress = 0;
        while(progress<0.5f)
        {
            progress+=Time.unscaledDeltaTime;
            yield return 0;
        }

        // Restore time
        PlayerControl.DecPlayerFreeze();
        DataShare.PlaySound("Boss_death",transform.position,false);
        progress = 0;
        speed = 2;
        profile.TryGet<ColorLookup>(out var cl);
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*speed;
            cl.contribution.value = Mathf.Lerp(1,0,progress);
            Time.timeScale = Mathf.Lerp(0,DataShare.GameSpeed,progress);
            yield return 0;
        }
        Destroy(volume.gameObject);
    }
    #endregion
}
