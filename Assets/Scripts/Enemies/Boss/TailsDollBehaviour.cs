using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class TailsDollBehaviour : BossGlobalStats
{
    #region main
    delegate void FixedUpdateEvent();
    FixedUpdateEvent fixedUpdateEvent;
    Coroutine CurAtkCoroutine;
    Rigidbody2D rb;
    Transform render;
    BulletShooter[] bulletShooters;
    Vector3 homePosition;
    Laser laser;
    Animation raceShield;
    RotateToTarget rotateToTarget;
    [SerializeField] float raceEndX = 0;
    [SerializeField] float raceSpeed = 10;
    [SerializeField] float atk2Range = 10;
    [SerializeField] float atk2Speed = 1;
    ParticleSystem[] particleSystems;
    SpriteTrailMain spriteTrailMain;

    GameObject Lightning;
    static bool IntroFightDamage = false;
    // Start is called before the first frame update
    protected override void Start()
    {
        render = transform.GetChild(0);
        spriteTrailMain = render.GetChild(1).GetComponent<SpriteTrailMain>();
        spriteTrailMain.SetSource(render.GetComponent<SpriteRenderer>());
        dir = (int)Mathf.RoundToInt(transform.localScale.x);
        particleSystems = new ParticleSystem[2];
        particleSystems[0] = render.GetChild(3).GetComponent<ParticleSystem>();
        particleSystems[1] = particleSystems[0].transform.GetChild(0).GetComponent<ParticleSystem>();

        homePosition = transform.position;
        // Get a list of bullet shooters ordered by their ID value.
        bulletShooters = transform.GetChild(0).GetChild(0).GetComponents<BulletShooter>().OrderBy(x => x.GetComponent<BulletShooter>().ID).ToArray();
        Transform target = transform.GetChild(0);

        laser = render.GetChild(2).GetComponent<Laser>();
        laser.laserUpdateCallback = UpdateLasers;
        try
        {
            Lightning = transform.GetChild(0).GetChild(5).gameObject;
        }
        catch (System.Exception)
        {
            Lightning = null;
        }
        if(raceEndX != 0)
        {
            IntroFightDamage = false;
            customStartBattleCallback += RaceStart;
            fixedUpdateEvent = CheckWonRace;
            enemyHit.failHitEvent = ShieldAppear;
            raceShield = render.GetChild(4).GetComponent<Animation>();
        }
        if(!IntroFightDamage || raceEndX != 0)
        {
            Destroy(Lightning);
        }


        base.Start();

        startBattleCallback += PreAtk;
        endBattleCallback += BattleEndReset;
        postDeathCallback += CustomDeathAnim;
        ButtonFunctions.levelSelectEvent += ResetFightFlag;

        spriteTrailMain.Activate();

    }
    void ResetFightFlag()
    {
        ButtonFunctions.levelSelectEvent -= ResetFightFlag;
        IntroFightDamage = false;
    }
    void CheckWonRace()
    {
        if(GameMaster.Goal)
        {
            print("Won race");
            IntroFightDamage = true;
            fixedUpdateEvent = null;
        }
    }
    void ShieldAppear()
    {
        print("Shield appear here");
        DataShare.PlaySound("Shield",transform.position,false);
        raceShield.gameObject.SetActive(true);
        raceShield.Stop();
        raceShield.Play();
    }
    void FixedUpdate()
    {
        fixedUpdateEvent?.Invoke();
    }
    void OnDestroy()
    {
        StopAllCoroutines();
    }
    void RaceStart()
    {
        DataShare.LoadMusic(BossFightMusic);
        StartCoroutine(IRace());
    }
    // Called from laser script
    void UpdateLasers()
    {
        particleSystems[0].transform.localEulerAngles = laser.transform.localEulerAngles;
        foreach(ParticleSystem p in particleSystems)
        {
            var shape = p.shape;
            Vector3 scale = shape.scale;
            Vector3 position = shape.position;
            scale.x = Mathf.Clamp(laser.distance,0,25);
            position.x = scale.x/2;

            shape.scale = scale;
            shape.position = position;
        }
    }
    void PreAtk()
    {
        StartCor(IPreAtk());
    }
    public void Atk1()
    {
        StartCor(IAtk1());
    }
    public void Atk2()
    {
        StartCor(IAtk2());
    }
    public void Atk3()
    {
        StartCor(IAtk3());
    }
    public void AnimFlip()
    {
        Flip(-dir);
    }
    public void Shoot(int ID)
    {
        bulletShooters[ID].Shoot(12,1);
    }
    public void Shoot()
    {
        bulletShooters[1].Shoot();
    }
    void BattleEndReset(Vector2 bulletPos)
    {
        IntroFightDamage = false;
        laser.gameObject.SetActive(false);
        particleSystems[0].Stop(true);
        spriteTrailMain.DeActivate();
        GetComponent<SortingGroup>().sortingOrder = 5;
        fixedUpdateEvent = null;

        transform.localEulerAngles = Vector3.zero;
        anim.speed = 0;
        ToggleFlipping(false);
        if(dir!=1)
        {
            Flip(-dir);
        }
    }
    void CustomDeathAnim()
    {
        anim.speed = 1;
        spriteTrailMain.Activate();
        DataShare.PlaySound("Whoosh3",transform.position,false,1,1);
    }
    #endregion

    #region IEnumerators
    IEnumerator IRace()
    {
        yield return new WaitForSeconds(1f);
        PlayerControl.DecPlayerFreeze();
        GameMaster.TimerSubtract = Time.timeSinceLevelLoad;

        // Animation start
        SetAnimStateAndReturn(4,0);
        yield return 0;
        yield return new WaitUntil(()=> AnimFinished());

        // Animation loop
        SetAnimState(5,0);

        float progress = 0;
        float startPos = transform.position.x;
        while(progress<1)
        {
            progress += Time.deltaTime * raceSpeed;
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(startPos,raceEndX,progress);
            transform.position = pos;
            yield return 0;
        }
        // Animation end
        fixedUpdateEvent = null;
        SetAnimStateAndReturn(7,0);
        yield return 0;
        yield return new WaitUntil(()=> AnimFinished());
        spriteTrailMain.DeActivate();
        enemyHit.canBeTouched = false;
        ToggleFlipping(true);
        SetAnimState(14,0);
    }
    IEnumerator IPreAtk()
    {
        if(IntroFightDamage)
        {
            PlayerControl.freezePlayerInput++;
            yield return new WaitForSeconds(0.5f);
            Lightning.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            enemyHit.HitFunc(transform,transform.position,(byte)Mathf.RoundToInt(HP/3));
            SetAnimStateAndReturn(15,0,false);
            yield return 0;
            yield return new WaitUntil(()=> AnimFinished());
            PlayerControl.DecPlayerFreeze();
            yield return new WaitForSeconds(passiveAttackDelay);

        }
        else
        {
            yield return new WaitForSeconds(2f+passiveAttackDelay);
        }
        NextAttack(false);
    }
    IEnumerator IAtk1() // Laser attack
    {
        ToggleFlipping(false);
        if(dir!=1)
        {
            Flip(-dir);
        }
        // Intro
        SetAnimStateAndReturn(8,0,false);
        yield return 0;
        DataShare.PlaySound("TailsDoll_Atk1Intro",transform.position,false,1,1);
        yield return new WaitForSeconds(0.35f);
        DataShare.PlaySound("TailsDoll_GemGlow",transform.position,false,1,1);
        yield return new WaitUntil(()=> AnimFinished());

        // Loop
        if(rotateToTarget==null) rotateToTarget = laser.GetComponent<RotateToTarget>();
        SetAnimState(9,0);
        bool light = type == Type.Light;
        byte count = (byte)(light ? 3 : 4);
        float wait = light ? 1f : 0.75f - (((float)(HP-currentHP)/HP) * 0.3f);

        DataShare.StopSound(loopSoundID);
        loopSoundID = DataShare.PlaySound("TailsDoll_LaserLoop",transform,true,0.5f,1f);
        particleSystems[0].gameObject.SetActive(true);
        
        while(count>0)
        {
            yield return 0;
            yield return new WaitUntil(() => !laser.gameObject.activeInHierarchy);
            laser.gameObject.SetActive(true);
            rotateToTarget.ToggleRotating = true;
            laser.Toggle(false);

            particleSystems[0].Stop(true);
            yield return 0;
            particleSystems[0].Play(true);

            yield return new WaitForSeconds(wait);
            rotateToTarget.ToggleRotating = false;
            DataShare.PlaySound("TailsDoll_LaserHit",transform,false,1f,1f);
            laser.Toggle(true);

            yield return new WaitForSeconds(0.5f);
            laser.Despawn();
            count--;
        }
        DataShare.StopSound(loopSoundID);
        particleSystems[0].Stop(true);
        yield return new WaitForSeconds(1f);
        particleSystems[0].gameObject.SetActive(false);

        // End
        SetAnimStateAndReturn(10,0,false);
        yield return 0;
        yield return new WaitUntil(()=> AnimFinished());

        ToggleFlipping(true);
        yield return new WaitForSeconds(0.4f+passiveAttackDelay);
        NextAttack();
    }
    IEnumerator IAtk2()
    {
        ToggleFlipping(false);
        if(dir!=1)
        {
            Flip(-dir);
        }
        int count = 5;
        float progress = 0;
        // Go right (10), left (-10), right (home)
        while(count>0)
        {
            if(count == 5)
            {
                // Animation start
                SetAnimStateAndReturn(4,0);
                yield return 0;
                yield return new WaitUntil(()=> AnimFinished());
            }
            else
            {
                // Turn
                SetAnimStateAndReturn(6,0,false);
                yield return 0;
                yield return new WaitUntil(()=> AnimFinished());
            }
            float speed = atk2Speed;
            if(count != 5 && count != 1)
            {
                DataShare.PlaySound("Ghost_spit",transform.position,false,1,1);
                Shoot(0);
            }
            else speed*=2;
            // Animation loop
            SetAnimState(5,0);
            progress = 0;
            Vector3 pos = transform.position;
            float startX = pos.x;
            float endPoint = count == 1 ? homePosition.x : atk2Range*dir;
            DataShare.PlaySound("Whoosh"+(int)Random.Range(1,3),transform.position,false,1,1);
            while(progress<1)
            {
                progress+=Time.deltaTime*speed;
                float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
                pos.x = Mathf.Lerp(startX,endPoint,mathStep);
                transform.position = pos;
                yield return 0;
            }
            // Wait until at destination position
            count--;

        }

        //Animation end
        SetAnimStateAndReturn(7,0);
        yield return 0;
        yield return new WaitUntil(()=> AnimFinished());
        ToggleFlipping(true);
        yield return new WaitForSeconds(0.4f+passiveAttackDelay);
        Atk1();

    }
    IEnumerator IAtk3()
    {
        ToggleFlipping(false);
        DataShare.PlaySound("TailsDoll_Atk3",transform.position,false,1,1);
        //Start
        SetAnimStateAndReturn(11,0);
        yield return 0;
        yield return new WaitUntil(()=> AnimFinished());
        //Loop
        SetAnimStateAndReturn(12,0);
        yield return new WaitForSeconds(0.5f);
        //Start shooting
        bool light = type == Type.Light;
        int bulletsToSpawn = light ? 10 : 13;
        float wait = light ? 0.5f : 0.4f;
        while(bulletsToSpawn>0)
        {
            DataShare.PlaySound("TailsDoll_BulletSpawn",transform.position,false,1,Random.Range(0.9f,1));
            Shoot();
            yield return new WaitForSeconds(wait);
            bulletsToSpawn--;
        }
        SetAnimStateAndReturn(13,0);
        yield return 0;
        yield return new WaitUntil(()=> AnimFinished());

        yield return 0;
        yield return new WaitForSeconds(1f+passiveAttackDelay);
        Atk1();
    }
    #endregion
}
