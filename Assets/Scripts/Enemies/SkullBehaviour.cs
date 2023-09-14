using System.Collections;
using UnityEngine;

public class SkullBehaviour : MonoBehaviour
{
    Rigidbody2D rb;
    Animator anim;
    BulletShooter bulletShooter;
    Transform bulletSource;
    Transform target;
    GameObject particles;
    ParticleSystem partSystem;
    Collider2D col;
    EnemyGlobalStats enemyGlobalStats;
    SineMoveBetweenRelativePoints sineMove;
    BulletPlayerTracker bulletPlayerTracker;
    public float MinDistance = 10;
    // Start is called before the first frame update
    void Init()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        sineMove = GetComponent<SineMoveBetweenRelativePoints>();
        enemyGlobalStats = GetComponent<EnemyGlobalStats>();
        enemyGlobalStats.despawnEvent = Reset;
        bulletShooter = transform.GetChild(0).GetComponent<BulletShooter>();
        bulletPlayerTracker = bulletShooter.transform.GetComponent<BulletPlayerTracker>();
        bulletSource = transform.GetChild(0).GetChild(0);
        particles = transform.GetChild(0).GetChild(1).gameObject;
        partSystem = particles.GetComponent<ParticleSystem>();
        target = GameObject.FindWithTag("Player").transform;
    }
    void OnEnable()
    {
        if(rb==null)Init();
        ResetSequence();
    }
    IEnumerator EndReset()
    {
        yield return 0;
        yield return 0;
        enemyGlobalStats.lockSpawn = false;
    }
    void Reset()
    {
        col.isTrigger = true;
        if(enemyGlobalStats.lockSpawn == true || rb.gravityScale == 0) return;
        enemyGlobalStats.lockSpawn = true;
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        rb.velocity = Vector2.zero;
        sineMove.Reset();
        if(partSystem.isPlaying)
        {
            partSystem.Stop(true,ParticleSystemStopBehavior.StopEmitting);
        }
        col.enabled = false;
        col.enabled = true;
        StartCoroutine(EndReset());
    }
    Coroutine RestoreParticles;
    IEnumerator IRestoreParticles()
    {
        particles.transform.SetParent(null);
        yield return new WaitUntil(()=>!partSystem.isPlaying);
        particles.transform.SetParent(transform.GetChild(0));
    }
    Coroutine DeathFallWait;
    IEnumerator IDeathFallWait()
    {
        col.isTrigger = false;
        yield return 0;
        yield return 0;
        yield return new WaitUntil(() => Mathf.Round(rb.velocity.y) == 0);
        anim.SetInteger("Status",4);
        DeathFallWait = null;
    }
    public void DeathEvent()
    {
        particles.transform.position = transform.position;
        particles.SetActive(true);
        if(RestoreParticles!=null)StopCoroutine(RestoreParticles);
        RestoreParticles = StartCoroutine(IRestoreParticles());
        rb.gravityScale = 8;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        enemyGlobalStats.lockSpawn = true;
        col.enabled = false;
        col.enabled = true;
        enemyGlobalStats.lockSpawn = false;
        StopSequence();
        this.enabled = false;
        if(DeathFallWait != null)
        StopCoroutine(DeathFallWait);
        DeathFallWait = StartCoroutine(IDeathFallWait());
    }
    public void ResetSequence()
    {
        if(cor!=null)StopCoroutine(cor);
        cor = StartCoroutine(IShootRepeat());
    }
    void Flip(int dir)
    {
        if(dir == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = dir;
        transform.localScale = scale;
    }
    void StopSequence()
    {
        if(cor!=null)StopCoroutine(cor);
        cor = null;

        CancelInvoke("FacePlayer");

        if(anim.GetInteger("Status")==1)
        {
            anim.SetInteger("Status",0);
        }
    }
    void OnDisable()
    {
        StopSequence();
    }
    void FacePlayer()
    {
        Flip(transform.position.x>=target.position.x ? 1 : -1);
    }
    /*void TrackPlayer()
    {
        if(Vector3.Distance(target.position,bulletSource.position)>MinDistance)
        {
            StopSequence();
            return;
        }
        else if(cor==null) ResetSequence();

        Vector3 distance = target.position - bulletSource.position;
        //Set the X axis rotation based on the distance between the target and the source
        bulletSource.right = distance;
    }*/
    Coroutine cor;
    IEnumerator IShootRepeat()
    {
        yield return new WaitForSeconds(Random.Range(0.1f,1f));
        while(true)
        {
            if(bulletPlayerTracker.TrackPlayer())
            {
                InvokeRepeating("FacePlayer",0,0.3f);
                yield return new WaitForSeconds(0.5f);
                anim.SetInteger("Status",1);
                yield return new WaitForSeconds(0.2f);
                anim.SetInteger("Status",0);
                yield return 0;
                yield return new WaitUntil(()=>anim.GetInteger("Status")!=3);
                CancelInvoke("FacePlayer");
                yield return 0;
                DataShare.PlaySound("Skull_spit",transform.position,false);
                bulletShooter.Shoot(1);
            }
            else CancelInvoke("FacePlayer");

            yield return new WaitForSeconds(1.3f);

        }
    }
}
