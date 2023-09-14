using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HappyMouseBehaviour : BossGlobalStats
{
    #region main
    [SerializeField] float movementSpeed = 500;
    [SerializeField] float fastMovementSpeed = 5000;
    [SerializeField] Vector2[] eyeSpawnPoints;
    [SerializeField] GameObject specialItem;
    float currentSpeed;
    float speedAdditive = 0.5f;
    int walkPress = 0; // Simulate pressing a direction
    Transform render;
    Rigidbody2D rb;
    VelocityCap speedCap;

    BulletShooter[] bulletShooters;
    SpriteTrailMain spriteTrailMain;

    Transform bulletSource;

    GameObject piano;
    float PianoSpawnHeight;

    // Start is called before the first frame update
    protected override void Start()
    {   
        rb = GetComponent<Rigidbody2D>();
        render = transform.GetChild(0);
        dir = (int)Mathf.RoundToInt(transform.localScale.x);
        speedCap = GetComponent<VelocityCap>();

        spriteTrailMain = render.GetChild(1).GetComponent<SpriteTrailMain>();
        bulletSource = render.GetChild(0);
        spriteTrailMain.SetSource(render.GetComponent<SpriteRenderer>());

        piano = transform.GetChild(1).gameObject;
        PianoSpawnHeight = piano.transform.position.y;

        // Get a list of bullet shooters ordered by their ID value.
        bulletShooters = transform.GetComponentsInChildren<BulletShooter>().OrderBy(x => x.GetComponent<BulletShooter>().ID).ToArray();

        startBattleCallback += PreAtk;
        endBattleCallback += BattleEndReset;
        Vector2 pos = transform.position;

        for (int i = 0; i < eyeSpawnPoints.Length; i++)
        {
            eyeSpawnPoints[i]+=pos;
        }

        base.Start();
    }
    void BattleEndReset(Vector2 bulletPos)
    {
        rb.velocity = Vector2.zero;
        anim.speed = 0;
        Destroy(piano);
        ToggleFlipping(false);
    }
    public void PreAtk()
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
    public void Atk4()
    {
        // Piano drop
        if(!piano.gameObject.activeInHierarchy)
        {
            Transform pianoTr = piano.transform;
            pianoTr.SetParent(null);
            pianoTr.position = new Vector3(target.position.x,PianoSpawnHeight,0);
            pianoTr.rotation = Quaternion.identity;
            pianoTr.localScale = Vector3.one;
            piano.SetActive(true);
        }
        NextAttack();
    }
    void Update()
    {
        XMovement(walkPress);
    }
    public void Shoot()
    {
        DataShare.PlaySound("HappyMouse_Shoot",transform.position,false);
        bulletShooters[0].Shoot();
    }
    void SetSpeedCap(float maxSpeed)
    {
        speedCap.HoritzontalCap = new Vector2(-1,1) * maxSpeed;
    }
    void XMovement(int xPos)
    {
        rb.velocity = new Vector2(0,rb.velocity.y);

        if(xPos!=0)
        {

            Vector2 HorizontalSpeed = speedCap.HoritzontalCap;
            if(Mathf.Abs(currentSpeed)>HorizontalSpeed.y)
            currentSpeed = Mathf.MoveTowards(currentSpeed,0,xPos*speedAdditive);

            else currentSpeed = Mathf.Clamp(currentSpeed+xPos*speedAdditive,HorizontalSpeed.x,
            HorizontalSpeed.y);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed,0,speedAdditive);
        }
        Vector2 force = Vector2.right*currentSpeed;
        force.y = 0;
        rb.AddForce(force,ForceMode2D.Impulse);
    }
    public void SpawnSpecial()
    {
        Instantiate(specialItem,transform.position+(Vector3.up/2),Quaternion.identity);
    }
    #endregion
    #region IEnumerators
    protected override IEnumerator IEndBattle(Vector2 bulletPos)
    {
        PlayerControl.godmode = true;
        yield return 0;
        yield return new WaitUntil(()=> Time.timeScale != 0);

        endBattleCallback?.Invoke(bulletPos);

        anim.speed = 1;
        walkPress = 0;
        speedAdditive = 4f;

        spriteTrailMain.DeActivate();

        anim.SetInteger("Status",2);
        FacePlayer();
        target.GetComponent<PlayerControl>()?.Flip(transform.position.x>=target.position.x ? 1 : -1);

        dialogueSystem.gameObject.SetActive(true);
        DialogueSystem.StartConvo(21);

        // Exit scene
        yield return new WaitUntil(()=>DialogueSystem.Event>=1);
        SetAnimState(5,0);
        DataShare.PlaySound("HappyMouse_Exit",transform.position,false);
        GetComponent<SortingGroup>().sortingOrder = 5;

        yield return new WaitUntil(()=>DialogueSystem.Event>=2);
        PlayerControl.DecPlayerFreeze();

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
    IEnumerator IPreAtk()
    {
        lockSpawn = true;
        dir *= -1;
        Flip(dir);
        yield return new WaitForSeconds(0.5f);
        SetAnimState(6,0);
        SetSpeedCap(movementSpeed);
        walkPress = -dir;
        yield return new WaitForSeconds(3f+passiveAttackDelay);
        NextAttack(false);
    }

    // Walk fast
    IEnumerator IAtk1()
    {
        DataShare.PlaySound("HappyMouse_SpeedUp",transform.position,false);
        SetSpeedCap(fastMovementSpeed);
        spriteTrailMain.Activate();
        SetAnimState(7,0);
        speedAdditive = 1;
        yield return new WaitForSeconds(2.5f);
        walkPress = 0;
        speedAdditive = 1.5f;
        yield return new WaitUntil(()=>Mathf.Abs(currentSpeed)<=movementSpeed);
        SetSpeedCap(movementSpeed);
        walkPress = -dir;
        SetAnimState(6,0);
        spriteTrailMain.DeActivate();
        speedAdditive = 0.5f;
        yield return new WaitForSeconds(0.4f+passiveAttackDelay);
        NextAttack();
    }

    // Shoot
    IEnumerator IAtk2()
    {
        walkPress = 0;
        yield return new WaitUntil(()=> currentSpeed == 0);
        SetAnimStateAndReturn(8,0);

        float camX = CamControl.self.transform.position.x;
        if(transform.position.x <= camX)
        dir = -1;
        else dir = 1;
        Flip(dir);

        if(type == Type.Dark) anim.speed = 2f;
        
        yield return 0;
        yield return new WaitUntil(()=> AnimFinishedTime(0));

        if(type == Type.Dark) anim.speed = 1;

        // Return to walking
        SetSpeedCap(movementSpeed);
        walkPress = -dir;

        yield return new WaitForSeconds(0.7f+passiveAttackDelay);
        NextAttack();
    }

    // Building eyes spawn
    IEnumerator IAtk3()
    {
        int amount =  (type == Type.Light ? 3 : 4) + (int)Mathf.Lerp(0,2,(float)(HP - CurrentHP)/(float)HP);
        List <int> DuplicateInts = new List<int>();
        while(amount>0)
        {
            amount--;
            int ID = Random.Range(0,eyeSpawnPoints.Length);
            // Dont allow to spawn in the same location twice
            if(DuplicateInts.Count>0)
            {
                while(DuplicateInts.Contains(ID))
                {
                    ID = Random.Range(0,eyeSpawnPoints.Length);
                }
            }
            DuplicateInts.Add(ID);
            bulletSource.position = eyeSpawnPoints[ID];
            bulletSource.eulerAngles = Vector3.zero;
            bulletShooters[1].Shoot();
            yield return new WaitForSeconds(0.2f);
        }
        DuplicateInts.Clear();
        yield return new WaitForSeconds(0.4f+passiveAttackDelay);
        NextAttack();
    }
    #endregion

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (UnityEditor.Selection.activeGameObject != gameObject) return;
        if(eyeSpawnPoints.Length == 0) return;
        Gizmos.color = Color.green;
        Vector3 pos = transform.position;
        foreach(Vector3 spawnPoint in eyeSpawnPoints)
        {
            Gizmos.DrawSphere(spawnPoint+pos,0.5f);
        }
    }
    #endif
}
