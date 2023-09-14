using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class MrMixBehaviour : BossGlobalStats
{
    #region vars
    SpriteTrailMain spriteTrailMain;
    [Space]
    [Header ("Mr Mix Values")]
    [Space]
    MrMixKeys mrMixKeys;
    [SerializeField] byte startPhaseIndex;
    [SerializeField] byte maxPhase = 2;
    byte phaseIndex = 0;

    GameObject SpeechBubble;
    BulletShooter bulletShooter;

    Transform comps;
    Animation shield;
    #endregion

    #region functions
    // Start is called before the first frame update
    protected override void Start()
    {
        Transform render = transform.GetChild(0);
        comps = transform.GetChild(1);
        spriteTrailMain = render.GetChild(1).GetComponent<SpriteTrailMain>();
        spriteTrailMain.SetSource(render.GetComponent<SpriteRenderer>());
        shield = render.GetChild(2).GetChild(0).GetComponent<Animation>();

        SpeechBubble = comps.GetChild(0).gameObject;
        mrMixKeys = comps.GetChild(1).GetComponent<MrMixKeys>();
        bulletShooter = comps.GetChild(2).GetComponent<BulletShooter>();



        phaseIndex = startPhaseIndex;
        mrMixKeys.phaseIndex = phaseIndex;
        Atk(phaseIndex);

        base.Start();


        mrMixKeys.wordCompleteEvent = WordDamage;
        enemyHit.failHitEvent += ShieldHit;
        startBattleCallback += PreAtk;
        endBattleCallback += BattleEndReset;
        postDeathCallback += CustomDeathAnim;
        spriteTrailMain.Activate();

        SpeechBubble.SetActive(false);
    }
    void ShieldHit()
    {
        DataShare.PlaySound("Shield",transform.position,false);
        shield.gameObject.SetActive(true);
        shield.Stop();
        shield.Play();
    }
    void WordDamage()
    {
        DataShare.PlaySound("MrMix_Damage",transform.position,false);
        enemyHit.HitFunc(transform,transform.position,10);
        if(CurrentHP==10)
        {
            ToggleFlipping(false);
            if(dir!=1)
            {
                Flip(-dir);
            }
            lockSpawn=true;
            if(comps.childCount>3)
            Destroy(comps.GetChild(3).gameObject);
            Atk(phaseIndex);

            SetAnimState(2,1);
            anim.speed = 0;
        }
        anim.SetFloat("MixSpeed",Mathf.Lerp(1,5,((float)(HP-currentHP))/100));
    }
    void Atk(int atkID)
    {

        switch(atkID)
        {
            default:    
                if(AtkLoopCor!=null)StopCoroutine(AtkLoopCor);
                if(AtkLoopCor2!=null)StopCoroutine(AtkLoopCor2);
            break;

            case 1: if(AtkLoopCor!=null)StopCoroutine(AtkLoopCor); AtkLoopCor = StartCoroutine(IAtk1()); break;
            case 2: if(AtkLoopCor2!=null)StopCoroutine(AtkLoopCor2); AtkLoopCor2 = StartCoroutine(IAtk2()); break;
        }
    }
    void PreAtk()
    {
        ToggleFlipping(true);
        StartWordLoop();
    }
    void BattleEndReset(Vector2 bulletPo)
    {
        anim.speed = 0;
        Destroy(SpeechBubble);
        enemyHit.failHitEvent -= ShieldHit;
        ToggleFlipping(false);
    }
    void CustomDeathAnim()
    {
        GetComponent<SortingGroup>().sortingOrder = -6;
        anim.speed = 1;
        spriteTrailMain.DeActivate();
    }

    void CurseSpit()
    {
        DataShare.PlaySound("MrMix_STFU",transform.position,false);
        SpeechBubble.SetActive(true);
    }
    public void PhotoSpit()
    {
        DataShare.PlaySound("MrMix_PhotoSpit",transform.position,false);
        bulletShooter.Shoot();
    }
    protected override void Flip(int dir)
    {
        if(dir == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = dir;
        transform.localScale = scale;
        this.dir = dir;
        comps.localScale = scale;
    }
    #endregion

    void StartWordLoop()
    {
        if(WordLoopCor!=null)StopCoroutine(WordLoopCor);
        WordLoopCor = StartCoroutine(IWordLoop());
    }
    Coroutine WordLoopCor;
    Coroutine AtkLoopCor,AtkLoopCor2;

    protected override IEnumerator IEndBattle(Vector2 bulletPos)
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
        DataShare.PlaySound("WindowsXP_Error",tr.position,false);
        yield return new WaitForSeconds(0.1f);
        if(postDeathCallback!=null)
        {
            postDeathCallback.Invoke();
        }
        else gameObject.SetActive(false);
        
        GoalObject.SetActive(true);
    }
    IEnumerator IWordLoop()
    {
        while(phaseIndex<=maxPhase)
        {
            // Loop words in sequence
            while(!mrMixKeys.ShowWord())
            {
                yield return 0;
                yield return new WaitUntil(() => !mrMixKeys.Active);
                yield return new WaitForSeconds(1f);
            }
            yield return new WaitForSeconds(1f);
            // Advance phase
            phaseIndex++;
            mrMixKeys.phaseIndex = phaseIndex;
            if(anim.GetInteger("Status") != 2)
            {
                Atk(phaseIndex);
            }
            else break;
        }
        DataShare.LoadMusic("");
        // Final one
        mrMixKeys.ShowWord();
        if(HP > 0)
        {
            yield return 0;
            yield return new WaitUntil(() => !mrMixKeys.Active);
            enemyHit.HitFunc(transform,transform.position,100);
        }
    }

    IEnumerator IAtk1() // Throwing curses
    {
        yield return new WaitForSeconds(1f);
        while(true)
        {
            if(anim.GetInteger("Status") == 2) yield break;
            SetAnimStateAndReturn(4,1);
            CurseSpit();
            yield return 0;
            yield return new WaitUntil(()=> AnimFinished());
            yield return new WaitUntil(()=> !SpeechBubble.activeInHierarchy || SpeechBubble == null);
            yield return new WaitForSeconds(1f+passiveAttackDelay);
        }
    }
    IEnumerator IAtk2() // Shooting images
    {
        yield return new WaitForSeconds(2f);
        while(true)
        {
            if(anim.GetInteger("Status") == 2) yield break;
            ToggleFlipping(true);
            // Charge up for a second
            SetAnimState(5,1);
            DataShare.PlaySound("MrMix_PhotoPre",transform.position,false);
            yield return new WaitForSeconds(1f);

            // Fire
            SetAnimStateAndReturn(6,1);

            yield return new WaitUntil(()=> AnimFinished());
            yield return 0;
            yield return new WaitForSeconds(2f+passiveAttackDelay);
        }
    }
}
