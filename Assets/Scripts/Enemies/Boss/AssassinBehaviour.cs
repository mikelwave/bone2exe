using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class AssassinBehaviour : BossGlobalStats
{
    #region main
    Rigidbody2D rb;
    float movementSpeed = 2000;

    delegate void FixedUpdateEvent();
    FixedUpdateEvent fixedUpdateEvent;
    Vector2Int roundedPos = Vector2Int.one*-99;

    [Range(0,3)]
    byte rotationAngle = 0; //0 - 0, 1 - 90, 2 - 180, 3 - 270
    Transform render;

    BulletShooter[] bulletShooters;
    SpriteTrailMain spriteTrailMain;
    [SerializeField] GameObject smokeObj;
    ParticleSystem smokeParticleSystem;

    List <GameObject> preknives;

    // Start is called before the first frame update
    protected override void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        render = transform.GetChild(0);
        dir = (int)Mathf.RoundToInt(transform.localScale.x);
        spriteTrailMain = render.GetChild(2).GetComponent<SpriteTrailMain>();
        spriteTrailMain.SetSource(render.GetComponent<SpriteRenderer>());

        // Get a list of bullet shooters ordered by their ID value.
        bulletShooters = transform.GetChild(0).GetChild(1).GetComponents<BulletShooter>().OrderBy(x => x.GetComponent<BulletShooter>().ID).ToArray();

        startBattleCallback += PreAtk;
        endBattleCallback += BattleEndReset;
        postDeathCallback += CustomDeathAnim;

        preknives = new List<GameObject>();
        Transform objects = transform.GetChild(1);
        for(int i = 0; i<objects.childCount;i++)
        {
            preknives.Add(objects.GetChild(i).gameObject);
        }

        base.Start();
    }
    void OnDestroy()
    {
        StopAllCoroutines();
    }
    void BattleEndReset(Vector2 bulletPos)
    {
        GetComponent<SortingGroup>().sortingOrder = 5;
        fixedUpdateEvent = null;
        rb.velocity = new Vector2((transform.position.x-bulletPos.x)*8f,20);
        transform.localEulerAngles = Vector3.zero;
        anim.speed = 0;
        ToggleFlipping(false);
        spriteTrailMain.DeActivate();
        rb.gravityScale = 10;
        StartCoroutine(IKnifeMelt());
    }
    void CustomDeathAnim()
    {
        anim.speed = 1;
        Transform tr = Instantiate(smokeObj,render.position,Quaternion.identity).transform;
        tr.SetParent(render);
        tr.localPosition = Vector3.up*-1;
        smokeParticleSystem = tr.GetComponent<ParticleSystem>();
        var emission = smokeParticleSystem.emission;
        emission.rateOverTime = 0;
        emission.rateOverDistance = 0.5f;
        DataShare.PlaySound("Assassin_DeathFly",transform,false);
    }
    public void SwitchLayer()
    {
        SortingGroup sortingGroup = GetComponent<SortingGroup>();
        sortingGroup.sortingLayerName = "Background";
        sortingGroup.sortingOrder = 0;
        var main = smokeParticleSystem.main;
        main.startSizeMultiplier = 0.25f;
        main.startColor = new Color(0.3f,0.3f,0.3f,1);
        StartCor(Destroy());
    }
    public void IntroKnifeSound()
    {
        DataShare.PlaySound("Assassin_KnifeThrow",transform.position,false);
        foreach(GameObject t in preknives)
        {
            t.SetActive(true);
        }
    }
    public void CrawlStartSound()
    {
        loopSoundID = DataShare.PlaySound("Assassin_crawl_start",transform.position,false); 
    }
    public void PreAtk()
    {
        StartCor(IPreAtk());
    }
    public void Atk1()
    {
        StartCor(IAtk1(3));
    }
    public void Atk2()
    {
        StartCor(IAtk2());
    }
    public void Atk3()
    {
        StartCor(IAtk3());
    }
    void FixedUpdate()
    {
        fixedUpdateEvent?.Invoke();
    }
    void Movement()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(render.right*-movementSpeed*Time.fixedDeltaTime*dir,ForceMode2D.Impulse);
        Vector3 pos = transform.position;
        Vector2Int curPos = new Vector2Int(
        (int)(dir == 1 ? Mathf.Floor(pos.x+0.5f) : Mathf.Ceil(pos.x-1.5f)),
        Mathf.FloorToInt(pos.y-0.5f));  
        
        if(curPos!=roundedPos && rotationAngle != 2)
        {
            roundedPos = curPos;
            WallDetect();
        }
    }
    void WallDetect()
    {
        // Check if gap or invalid tile
        Vector3Int tilePos;

        // right and down
        if(dir == 1)
        {
            if(rotationAngle%3 == 0)
            tilePos = new Vector3Int(roundedPos.x-(rotationAngle == 0 ? 1 : 0),roundedPos.y+(rotationAngle == 0 ? 0 : 1),0);
            else tilePos = new Vector3Int(roundedPos.x-(rotationAngle == 1 ? 1:0),roundedPos.y,0);
        }
        else
        {
            if(rotationAngle%3 == 0)
            tilePos = new Vector3Int(roundedPos.x+(rotationAngle == 0 ? 1 : 2),roundedPos.y,0);
            else tilePos = new Vector3Int(roundedPos.x,roundedPos.y+(rotationAngle == 1 ? 1:0),0);
        }

        // Walls
        #if UNITY_EDITOR
        Debug.DrawLine(render.position,tilePos,Color.cyan);
        #endif
        if(GameMaster.GetTileResult(render.position,tilePos,"MainMap") != 0)
        {
            fixedUpdateEvent = null;
            StartCoroutine(IRotateWall());
            rb.velocity = Vector2.zero;
        }
    }
    // Normal attack
    public void Shoot()
    {
        bulletShooters[0].Shoot();
    }
    public void BurstShoot()
    {
        DataShare.PlaySound("Assassin_ball",transform.position,false);
        bulletShooters[1].Shoot(3,0);
    }
    #endregion

    #region IEnumerators
    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
    IEnumerator IRotateWall()
    {
        ///print(transform.name+" Rotate started");
        Vector3 rotation = transform.eulerAngles;
        Vector3 targetRotation = rotation + (Vector3.forward*90*-dir);
        Vector3 startPos = new Vector3(Mathf.FloorToInt(transform.position.x)+0.5f,roundedPos.y+0.5f,transform.position.z);
        Vector3 endPos = startPos+transform.up;
        ///print(transform.up);
        endPos.x = Mathf.Round(endPos.x * 10f) * 0.1f;
        endPos.y = Mathf.Round(endPos.y * 10f) * 0.1f;

        const float rotationSpeed = 10;
        float progress = 0;
        while(progress<1)
        {
            progress+=Time.fixedDeltaTime * rotationSpeed;
            transform.eulerAngles = Vector3.Lerp(rotation,targetRotation,progress);
            transform.position = Vector3.Lerp(startPos,endPos,progress);
            yield return 0;
        }
        targetRotation.z = Mathf.Repeat(targetRotation.z,360);
        rotationAngle = (byte)(Mathf.RoundToInt(targetRotation.z)/90);
        // restore movement
        fixedUpdateEvent = Movement;
    }
    IEnumerator IPreAtk()
    {
        yield return new WaitForSeconds(1f);
        DataShare.PlaySound("Assassin_KnifePre",transform.position,false);
        SetAnimStateAndReturn(4,0);
        spriteTrailMain.Activate();
        yield return 0;
        yield return new WaitUntil(()=> AnimFinished());
        spriteTrailMain.DeActivate();

        yield return new WaitForSeconds(0.25f+passiveAttackDelay);
        NextAttack(false);
    }
    // knife throw
    IEnumerator IAtk1(byte amount)
    {
        ToggleFlipping(true);
        while(amount>0)
        {
            spriteTrailMain.Activate();
            SetAnimStateAndReturn(5,0);
            yield return 0;
            yield return new WaitUntil(()=> AnimFinished());
            spriteTrailMain.DeActivate();
            yield return new WaitForSeconds(0.15f+passiveAttackDelay/3);
            amount--;
            yield return 0;
        }
        yield return new WaitForSeconds(0.4f+passiveAttackDelay);
        NextAttack();
    }
    // boomerangs
    IEnumerator IAtk2()
    {

        DataShare.PlaySound("Assassin_ball_charge",transform.position,false);
        ToggleFlipping(true);
        SetAnimStateAndReturn(6,0);
        yield return 0;
        yield return new WaitUntil(()=> AnimFinished());

        yield return 0;

        yield return new WaitForSeconds(1.1f+passiveAttackDelay);
        NextAttack();
    }
    // crawl
    IEnumerator IAtk3()
    {
        spriteTrailMain.Activate();
        ToggleFlipping(false);
        rotationAngle = 0;
        SetAnimState(7,0);
        yield return 0;
        yield return new WaitUntil(()=> AnimFinishedTime(0));
        DataShare.StopSound(loopSoundID);
        loopSoundID = DataShare.PlaySound("Assassin_crawl_loop",transform,true);

        // main loop
        SetAnimState(8,0);
        fixedUpdateEvent = Movement;
        yield return 0;

        // Wait until upside down
        yield return new WaitUntil(()=> rotationAngle == 2);

        Transform playerTarget = GameMaster.GetClosestTransform("Player",transform.position);
        Transform t = transform;

        // Drop down if X matches or goes over player X pos
        yield return new WaitUntil(()=> (t.position.x-playerTarget.position.x)*-dir < 0);
        DataShare.StopSound(loopSoundID);
        loopSoundID = DataShare.PlaySound("Assassin_Fall",transform,false);
        fixedUpdateEvent = null;
        rb.velocity = Vector2.zero;
        rotationAngle = 0;

        // stop and fall
        SetAnimState(9,0);
        rb.gravityScale = 10;
        Vector3 pos = transform.position;

        // Find a raycast with objects on the ground layer only
        RaycastHit2D ray = Physics2D.Raycast(pos,-(Vector3.up*20f),20f,128);
        Vector2 endPos;
        if(ray.collider!=null)
        {
            endPos = ray.point;
            endPos.y+=2;
            Debug.DrawLine(pos,endPos,Color.green,2f);
            ///print("Point: "+ray.point);
        }
        else
        {
            endPos = pos-(Vector3.up*20f);
            Debug.DrawLine(pos,endPos,Color.red,2f);
        }
        yield return new WaitUntil(()=> transform.position.y<=endPos.y);
        if(loopSoundID != -1)
        {
            DataShare.StopSound(loopSoundID);
        }
        DataShare.PlaySound("Assassin_land",transform.position,false);
        SetAnimState(10,0);
        ToggleFlipping(true);
        spriteTrailMain.DeActivate();
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        pos = transform.position;
        pos.y = endPos.y-1.5f;
        transform.eulerAngles = Vector3.zero;
        transform.position = pos;
        yield return 0;

        yield return new WaitUntil(()=> AnimFinishedTime(0));
        SetAnimState(0,0);
        yield return 0;
        yield return new WaitUntil(()=> AnimFinished());

        yield return new WaitForSeconds(1f+passiveAttackDelay);
        NextAttack();
    }
    
    // Post death
    IEnumerator IKnifeMelt()
    {
        preknives = new List<GameObject>();
        Transform objects = GameObject.Find("Objects").transform;
        for(int i = 0; i<objects.childCount;i++)
        {
            GameObject t = objects.GetChild(i).gameObject;
            ///Debug.Log(t.name);
            if(t.name.Contains("Pre"))
                preknives.Add(t);
        }
        ///Debug.Log("List length: "+preknives.Count);

        yield return 0;
        yield return new WaitUntil(()=>Time.timeScale != 0);
        float progress = 0;
        float startPoint = preknives[0].transform.position.y;
        float endPoint = startPoint - (Vector3.up*2).y;
        
        float maxProgress = (float)preknives.Count/2;
        float step = maxProgress/(float)preknives.Count/2;
        while(progress < 2)
        {
            for(int i = 0; i<preknives.Count;i++)
            {
                Vector2 pos = preknives[i].transform.position;
                pos.y = Mathf.Lerp(startPoint,endPoint,progress-(step*(float)i));
                preknives[i].transform.position = pos;
            }
            progress += Time.deltaTime;
            yield return 0;
        }

        foreach(GameObject t in preknives)
        {
            Destroy(t);
        }
        preknives.Clear();
    }
    #endregion
}
