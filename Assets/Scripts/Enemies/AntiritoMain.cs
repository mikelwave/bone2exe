using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiritoMain : MonoBehaviour
{
    // On activation, move main object in the facing direction
    // Spawned antiritos move in a sine wave, with X offsets and sine progress offsets
    bool activated = false;
    [Header ("Settings")]
    public bool respawns = true;
    public float movementSpeed = 1;
    float startMovementSpeed = 0;
    [Range (1,10)]
    public int enemyAmount = 5;
    public float enemyDistance = 1.25f;
    bool charging = false;
    bool moving = true;
    public bool GivesPoint = true;
    Transform holder;
    Vector3 screenCenter;
    [Space]
    [Header ("Sine movement")]

    // Sine progress
    public float sineSpeed = 1;
    public float sineAmplitude = 1;

    // Sine spacing is calculated by the amount of enemies
    float sineSpacing;
    float sineProgress = 0;

    public bool activatedByCamera = false;

    // Objects
    Transform[] objects;
    byte killedAmount = 0;
    IEnumerator ITurnMaterials()
    {
        yield return 0;
        yield return new WaitUntil(() => !this.enabled);
        foreach(Transform t in objects)
        {
            t.GetChild(0).GetComponent<SpriteRenderer>().material = GameMaster.self.enemyRespawnMaterial;
        }
    }
    
    void CheckAlive()
    {
        for(int i = 0; i < holder.childCount; i++)
        {
            if(holder.GetChild(i).GetChild(0).gameObject.activeInHierarchy)
            return;
        }
        // Disable holder
        holder.gameObject.SetActive(false);
        this.enabled = false;
    }
    void CheckAllKilled(bool lethal,Vector2 bulletPos)
    {
        if(!lethal || !GivesPoint) return;
        killedAmount++;
        ///print("Killed: "+killedAmount);
        if(killedAmount<enemyAmount) return;
        if(GivesPoint)
        {
            ///print("All anti-ritos killed");
            StartCoroutine(ITurnMaterials());
            DataShare.PlaySound("Antirito_AllDead",transform.position,false);
            GameMaster.EnemiesKilled++;
        }
        GivesPoint = false;
    }
    void StopAndCharge()
    {
        if(charging) return;
        charging = true;
        foreach(Transform t in objects)
        {
            Animator anim = t.GetComponent<Animator>();
            if(anim.GetInteger("Status")==0) anim.SetInteger("Status",1);
        }
        StartCoroutine(IStopAndCharge());
    }
    IEnumerator IStopAndCharge()
    {
        moving = false;
        yield return new WaitForSeconds(0.5f);
        movementSpeed*=4;
        moving = true;
    }

    public float GetSine(int index)
    {
        return Mathf.Sin(sineProgress+(sineSpacing*(float)index));
    }
    void Movement()
    {
        holder.position -= holder.right * transform.lossyScale.x * movementSpeed * Time.fixedDeltaTime;
    }
    void UpdateSine()
    {
        sineProgress = Mathf.Repeat(sineProgress+(Time.fixedDeltaTime*sineSpeed),Mathf.PI*2);
    }
    void GenerateObjects()
    {
        GameObject sample = holder.GetChild(0).gameObject;
        if(sample == null) Destroy(gameObject);
        objects = new Transform[enemyAmount];
        objects[0] = sample.transform;

        EnemyGlobalStats enemyGlobalStats = objects[0].GetComponent<EnemyGlobalStats>();
        enemyGlobalStats.despawnEvent = CheckAlive;
        enemyGlobalStats.Active = false;
        bool dark = enemyGlobalStats.type == EnemyGlobalStats.Type.Dark;

        GenericAdapter genericAdapter = objects[0].GetComponent<GenericAdapter>();
        if(dark) genericAdapter.OnDeathEvent.AddListener(StopAndCharge);
        genericAdapter.OnDeathEvent.AddListener(CheckAlive);

        for(int i = 0;i<enemyAmount;i++)
        {
            Transform obj;
            if(i != 0)
            {
                obj = Instantiate(sample,sample.transform.position,Quaternion.identity).transform;
                obj.SetParent(holder);
                obj.localPosition += (Vector3.right*enemyDistance*i);
                obj.localScale = sample.transform.localScale;
            }
            else obj = objects[0];

            genericAdapter = obj.GetComponent<GenericAdapter>();
            genericAdapter.OnDeathEvent.AddListener(CheckAlive);
            if(dark) genericAdapter.OnDeathEvent.AddListener(StopAndCharge);

            obj.GetComponent<EnemyGlobalStats>().despawnEvent = CheckAlive;
            obj.GetChild(0).GetComponent<EnemyHit>().posthitEvent += CheckAllKilled;
            objects[i] = obj;
        }
    }
    void Start()
    {
        startMovementSpeed = movementSpeed;
        holder = transform.GetChild(0);
        sineSpacing = Mathf.PI*2/(float)enemyAmount;
        GenerateObjects();
        holder.gameObject.SetActive(false);
        this.enabled = false;
    }
    void FixedUpdate()
    {
        if(!activated) return;

        if(moving) Movement();
        if(!charging)
        {
            UpdateSine();
            Vector3 pos = holder.position;
            for(int i = 0;i<objects.Length;i++)
            {
                pos.x = objects[i].position.x;
                pos.y = holder.position.y+GetSine(i); 
                objects[i].position = pos;
            }
        }
    }
    void OnDisable()
    {
        sineProgress = 0;
        charging = false;
        movementSpeed = startMovementSpeed;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(!activated && (other.tag == "Player" || (activatedByCamera && other.tag == "ObjActivator")))
        {
            this.enabled = true;
            activated = true;
            screenCenter = CamControl.self.transform.position;
            // Move holder to offscreen
            holder.position = new Vector3(screenCenter.x+20 * transform.lossyScale.x,holder.position.y,holder.position.z);
            holder.gameObject.SetActive(true);
            foreach(Transform t in objects)
            {
                t.GetComponent<EnemyGlobalStats>().Spawn(true);
            }
        }
    }

    // Reset
    void OnTriggerExit2D(Collider2D other)
    {
        if(respawns && other.tag == "ObjActivator")
        {
            activated = false;
            this.enabled = false;
            holder.gameObject.SetActive(false);
        }
    }
}
