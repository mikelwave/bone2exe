using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BloodyMarryScript : BossGlobalStats
{
    #region vars
    Rigidbody2D rb;
    SpriteTrailMain spriteTrailMain;
    Transform render;
    [SerializeField] float DashSpeed = 10;
    [SerializeField] Sprite[] warningSprites;
    SpriteRenderer warning;
    Transform SMGFlash;
    BulletShooter[] bulletShooters;
    [SerializeField] GameObject Satchel;
    Vector2Int satchelThrowDistance;
    float satchelHeight = 0;

    bool OnGround = true;
    #endregion

    #region functions
    // Start is called before the first frame update
    protected override void Start()
    {
        if(!PlayerControl.respawn && DataShare.MusicNowPlaying != BossFightMusic) DataShare.LoadMusic("");
        rb = GetComponent<Rigidbody2D>();
        render = transform.GetChild(0);
        warning = transform.GetChild(1).GetComponent<SpriteRenderer>();
        SMGFlash = transform.GetChild(2);
        SMGFlash.GetComponent<DisableEvent>().mainParent = transform;
        spriteTrailMain = render.GetChild(1).GetComponent<SpriteTrailMain>();
        spriteTrailMain.SetSource(render.GetComponent<SpriteRenderer>());

        // Get a list of bullet shooters ordered by their ID value.
        bulletShooters = transform.GetChild(0).GetChild(0).GetComponents<BulletShooter>().OrderBy(x => x.GetComponent<BulletShooter>().ID).ToArray();

        earlyStartBattleCallback += Setup;
        endBattleCallback += BattleEndReset;
        postDeathCallback += CustomDeathAnim;

        Vector2 pos = transform.position;
        satchelHeight = pos.y;

        // Define the limits of throwing satchels

        RaycastHit2D left = Physics2D.Raycast(pos,Vector2.left,30f,128);
        if(left.point == null) satchelThrowDistance.x = -30;
        else satchelThrowDistance.x = Mathf.RoundToInt(left.point.x + 4f);

        RaycastHit2D right = Physics2D.Raycast(pos,Vector2.right,30f,128);
        if(right.point == null) satchelThrowDistance.y = 30;
        else satchelThrowDistance.y = Mathf.RoundToInt(right.point.x - 4f);

        base.Start();
    }
    void Setup()
    {
        PlayerControl.checkPointVal = 1;
        StartCoroutine(ISetup());
    }
    void BattleEndReset(Vector2 bulletPos)
    {
        PauseMenu.allowPause = false;
        GetComponent<SortingGroup>().sortingOrder = 0;
        Physics2D.IgnoreCollision(GetComponent<BoxCollider2D>(),GameObject.Find("SSMap").GetComponent<CompositeCollider2D>(),true);
        transform.localEulerAngles = Vector3.zero;
        rb.gravityScale = 10;
        rb.velocity = Vector2.zero;
        rb.velocity = new Vector2((transform.position.x-bulletPos.x)*8f,20);

        anim.speed = 0;
        ToggleFlipping(false);
        spriteTrailMain.DeActivate();
    }
    void CustomDeathAnim()
    {
        anim.speed = 1;
        anim.SetInteger("Status",0);
    }
    public void Shoot(int index)
    {
        if(index == 0)DataShare.PlaySound("BM_SMG",transform.position,false);
        bulletShooters[index].Shoot();
    }
    public void ThrowSatchel()
    {
        MoveToPoint[] satchels = new MoveToPoint[3];
        Vector3 spawnPoint = bulletShooters[0].transform.position;
        List<int> XPositions = new List<int>();

        // Generate horizontally without repeat
        for (int i = 0; i < satchels.Length; i++)
        {
            int xPos = Random.Range(satchelThrowDistance.x,satchelThrowDistance.y);

            while(XPositions.Contains(xPos))
            {
                xPos = Random.Range(satchelThrowDistance.x,satchelThrowDistance.y);
            }
            XPositions.Add(xPos);
        }
        DataShare.PlaySound("BM_SatchelThrow",transform.position,false);
        for(int i = 0 ; i<satchels.Length; i++)
        {
            Vector2 endPos = new Vector2(XPositions[i]-0.5f,satchelHeight);
            satchels[i] = Instantiate(Satchel,spawnPoint,Quaternion.identity).GetComponent<MoveToPoint>();
            satchels[i].targetPoint = endPos;
        }

    }
    public void Atk1()
    {
        StartCor(OnGround ? IGrndAtk1() : IAirAtk1());
    }
    public void Atk2()
    {
        SMGFlash.gameObject.SetActive(true);
        SMGFlash.SetParent(target);
        SMGFlash.localPosition = Vector3.zero;
        DataShare.PlaySound("BM_ReticleAppear",SMGFlash,false);
        StartCor(OnGround ? IGrndAtk2() : IAirAtk2());
    }
    public void Atk3()
    {
        StartCor(OnGround ? IGrndAtk3() : IAirAtk3());
    }
    public void Dash()
    {
        StartCor(IDash());
    }
    #endregion

    #region IEnumerators
    protected override IEnumerator IEndBattle(Vector2 bulletPos)
    {
        PlayerControl.godmode = true;
        // Stop timer here, because there is no goal to access
        GameMaster.SaveStats();
        SpawnObject[] objects = FindObjectsOfType<SpawnObject>();
        foreach (SpawnObject obj in objects)
        {
            if(obj.tag == "EnemyBullet")
            {
                Destroy(obj.gameObject);
            }
        }
        SatchelTick[] satchels = FindObjectsOfType<SatchelTick>();
        foreach (SatchelTick obj in satchels)
        {
            if(obj.tag == "EnemyBullet")
            {
                Destroy(obj.gameObject);
            }
        }
        yield return 0;
        yield return new WaitUntil(()=> Time.timeScale != 0);

        endBattleCallback?.Invoke(bulletPos);
        anim.SetInteger("Status",2);

        Transform tr = spriteRenderer.transform;
        ParticleSystem particleSystem = Instantiate(explosion,tr.position+Vector3.up*2,Quaternion.identity).GetComponent<ParticleSystem>();
        particleSystem.Play(false);
        CamControl.ShakeCamera(0.6f,0.35f);
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => Mathf.Round(rb.velocity.y * 100) / 100 == 0);

        if(postDeathCallback!=null)
        {
            postDeathCallback.Invoke();
        }
        else gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);
        
        if(GoalObject != null)
        {
            if(GoalObject.scene != gameObject.scene)
            {
                GameObject obj = Instantiate(GoalObject);
                GoalObject = obj;
                obj.SetActive(true);
            }
            else GoalObject.SetActive(true);
        }
    }
    IEnumerator ISetup()
    {
        yield return new WaitForSeconds(0.5f);
        SetAnimStateAndReturn(4,0);
        yield return new WaitForSeconds(0.4f);
        target = bulletShooters[0].GetComponent<BulletPlayerTracker>().target;
        DataShare.PlaySound("BM_Ready",transform.position,false);
        yield return 0;
        yield return new WaitUntil(()=> AnimFinished());

        yield return new WaitForSeconds(passiveAttackDelay);
        NextAttack(false);
    }
    IEnumerator IDashLerp(int animState,Vector2 direction, Vector2 endPosOffset)
    {
        Vector2 pos = transform.position;
        Vector2 endPos = pos;

        SetAnimState(animState,0);
        yield return 0;
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos,direction,Mathf.Infinity,128);
        RaycastHit2D hit = new RaycastHit2D();
        foreach(RaycastHit2D h in hits)
        {
            if(h.collider.tag == "SSMap")continue;
            else
            {
                hit = h;
                break;
            }
        }
        if(hit.point != null)
        {
            Debug.DrawLine(pos,hit.point,Color.cyan,3f);
            if(direction.y != 0)
            {
                endPos = (new Vector2(hit.point.x,Mathf.Round(hit.point.y)+0.5f) + endPosOffset) - ((direction.y < 0 ? 0.5f*dir : 0) * Vector2.right);
            }
            else endPos = hit.point + endPosOffset;
        }
        // Fail safe cancel of a dash
        else
        {
            SetAnimState(4,0);
            yield break;
        }

        float Distance = Vector2.Distance(pos,endPos) + (direction.y == 0 ? 0 : 0.5f);

        // Select the correct sprite index based on direction vector
        warning.sprite = warningSprites[(int)(
                (Mathf.Abs(direction.x) * (direction.x + 2))
               + Mathf.Abs(direction.y) * (direction.y + 1))];

        warning.transform.position = pos + (new Vector2(2,3) * direction + (direction.y == 1 ? Vector2.zero : Vector2.up));
        warning.flipX = transform.localScale.x == -1;
        warning.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        spriteTrailMain.Activate();

        DataShare.PlaySound("BM_Dash",transform.position,false);
        float progress = 0;
        while(progress<1)
        {
            progress += Time.deltaTime*(DashSpeed/Distance);
            float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
            transform.position = Vector2.Lerp(pos,endPos,progress > 0.5f ? progress : mathStep);
            yield return 0;
        }
        if(direction.x != 0) Flip(-dir);

        spriteTrailMain.DeActivate();
        transform.position = endPos;
        SetAnimState(4,0);
    }
    // Dash attack
    IEnumerator IDash()
    {
        ToggleFlipping(false);
        yield return 0;
        int DashAmount = (type == Type.Light ? 3 + (OnGround ? 0 : 1) : 1 + (OnGround ? 1 : 0));
    
        // Horizontal dashes
        while(DashAmount>1)
        {
            DataShare.PlaySound("BM_DashWarnHor",transform.position,false);
            StartCoroutine(IDashLerp(OnGround ? 8 : 9,Vector2.right*-transform.localScale.x,(Vector2.right*0.65f*transform.localScale.x)));
            yield return 0;
            yield return new WaitUntil(() => anim.GetInteger("Status") == 4);
            DashAmount--;
            yield return new WaitForSeconds(0.20f);
        }

        // Final vertical dash
        yield return new WaitForSeconds(0.25f);
        DataShare.PlaySound("BM_DashWarnVer",transform.position,false);
        StartCoroutine(IDashLerp(OnGround ? 10 : 11,Vector2.up*(OnGround ? 1 : -1),(OnGround ? Vector2.up*-2.5f : Vector2.zero)));
        yield return 0;
        yield return new WaitUntil(() => anim.GetInteger("Status") == 4);
        OnGround = !OnGround;

        yield return new WaitForSeconds(0.5f + passiveAttackDelay);
        NextAttack();

    }
    // Satchel attack
    IEnumerator IGrndAtk1()
    {
        ToggleFlipping(false);
        yield return 0;
        Flip(transform.position.x >= 0 ? 1 : -1);

        SetAnimStateAndReturn(5,0);

        yield return new WaitUntil(()=> AnimFinished());
        yield return 0;
        NextAttack();
    }

    // Bullet spray
    IEnumerator IGrndAtk2()
    {
        DataShare.PlaySound("BM_SMGWarn",transform.position,false);
        yield return new WaitForSeconds(0.75f);
        ToggleFlipping(true);
        SetAnimState(6,0);
        yield return new WaitForSeconds(0.75f);
        yield return 0;
        SetAnimState(4,0);
        ToggleFlipping(false);
        yield return new WaitForSeconds(passiveAttackDelay);
        NextAttack();
    }

    // Bullet arc
    IEnumerator IGrndAtk3()
    {
        DataShare.PlaySound("BM_BulletArc",transform.position,false);
        ToggleFlipping(true);
        yield return 0;
        ToggleFlipping(false);
        SetAnimStateAndReturn(7,0);
        yield return new WaitForSeconds(0.3f);
        spriteTrailMain.Activate();
        yield return new WaitForSeconds(0.3f);
        spriteTrailMain.DeActivate();
        yield return 0;
        yield return new WaitUntil(()=> AnimFinished());
        NextAttack();
    }

    // Satchel attack
    IEnumerator IAirAtk1()
    {
        ToggleFlipping(false);
        yield return 0;
        Flip(transform.position.x >= 0 ? 1 : -1);

        SetAnimStateAndReturn(13,0);

        yield return new WaitUntil(()=> AnimFinished());
        yield return 0;
        NextAttack();
    }

    // Bullet spray
    IEnumerator IAirAtk2()
    {
        DataShare.PlaySound("BM_SMGWarn",transform.position,false);
        yield return new WaitForSeconds(0.75f);
        ToggleFlipping(false);
        yield return 0;
        SetAnimState(14,0);
        yield return new WaitForSeconds(1f);
        yield return 0;
        SetAnimState(12,0);
        yield return new WaitForSeconds(passiveAttackDelay);
        NextAttack();
    }

    // Bullet flak shot
    IEnumerator IAirAtk3()
    {
        ToggleFlipping(false);
        yield return 0;
        SetAnimStateAndReturn(15,0);
        yield return new WaitForSeconds(0.3f);
        DataShare.PlaySound("BM_Flak",transform.position,false);
        spriteTrailMain.Activate();
        yield return new WaitForSeconds(0.3f);
        spriteTrailMain.DeActivate();
        yield return 0;
        yield return new WaitUntil(()=> AnimFinished());
        NextAttack();
    }
    #endregion
}
