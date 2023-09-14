using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Events;

public class TrashBugAdapter : MonoBehaviour
{
    #region main
    EnemyWalk enemyWalk;
    EnemyHit enemyHit;
    EnemyGlobalStats enemyGlobalStats;
    Animator anim;
    public GameObject bulletObject;
    public bool onlyShootFromBehind = false;
    BulletShooter bulletShooter;
    SpriteRenderer spriteRenderer;
    public void ResetSequence()
    {
        if(bulletShooter==null) return;
        if(cor!=null)StopCoroutine(cor);
        cor = StartCoroutine(IShootRepeat());
    }
    void StopSequence()
    {
        if(cor!=null)StopCoroutine(cor);
        cor = null;

        if(anim!=null && anim.GetInteger("Status")==1)
        {
            anim.SetInteger("Status",0);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        enemyWalk = GetComponent<EnemyWalk>();
        enemyWalk.pauseCooldownEndEvent = RestoreWalk;

        enemyGlobalStats = GetComponent<EnemyGlobalStats>();

        enemyHit = transform.GetChild(0).GetComponent<EnemyHit>();
        enemyHit.bounceEvent = TrashbugBounce;
        enemyHit.hitEvent += TrashBugHit;
        
        anim = GetComponent<Animator>();

        // Icebugs
        bulletShooter = transform.GetChild(0).GetComponent<BulletShooter>();
        ResetSequence();
    }
    void OnEnable()
    {
        if(enemyGlobalStats==null) return;

        if(enemyGlobalStats.respawns)
        {
            // Set material
            if(anim.GetInteger("Status") == 2 && spriteRenderer == null)
            {
                spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
                spriteRenderer.material = GameMaster.self.enemyRespawnMaterial;
            }

            anim.SetInteger("Status",0);
            enemyHit.canBeTouched = true;
            enemyWalk.pauseWalkCooldown = 0f;
            ResetSequence();
            enemyWalk.Respawn();
        }
        else
        {
            if(!enemyHit.canBeTouched)
            {
                enemyWalk.enabled = false;
                anim.SetInteger("Status",2);
            }
            else
            {
                ResetSequence();
                enemyWalk.Respawn();
            }
        }
    }

    void TrashbugBounce()
    {
        DataShare.PlaySound("Trashbug_bounce",transform.position,false);
        enemyHit.canDealDamage = false;
        enemyWalk.pauseWalkCooldown = 0.25f;
        anim.SetInteger("Status",1);
    }
    void SpawnBullet()
    {
        if(bulletObject==null) return;
        MoveToTarget moveToTarget = Instantiate(bulletObject,transform.position,Quaternion.identity).GetComponent<MoveToTarget>();
        if(moveToTarget!=null)
        {
            moveToTarget.Init(GameObject.FindWithTag("Player").transform);
            moveToTarget.gameObject.SetActive(true);
        }
    }

    bool TrashBugHit(byte damage,float XPos)
    {
        enemyWalk.pauseWalkCooldown = 0.2f;
        
        anim.Rebind();
        anim.Update(0f);

        // Icebugs
        if(onlyShootFromBehind && (XPos * transform.localScale.x > 0))
        {
            //Shot anim
            anim.SetInteger("Status",4);
            return false;
        }

        // Kill
        if(enemyGlobalStats.Hurt(damage))
        {
            enemyHit.canBeTouched = false;
            anim.SetInteger("Status",2);
            enemyWalk.enabled = false;
            deathEvent.Invoke();
            StopSequence();

            // Dark mode burst
            if(enemyGlobalStats.type == EnemyGlobalStats.Type.Dark)
            {
                SpawnBullet();
            }

            return true;
        }
        else
        {
            // Hurt
            anim.SetInteger("Status",3);
            return false;
        }
    }
    void OnDisable()
    {
        StopSequence();
    }

    void RestoreWalk()
    {
        enemyHit.canDealDamage = true;
        anim.SetInteger("Status",0);
    }
    Coroutine cor;
    IEnumerator IShootRepeat()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.5f);
            enemyWalk.pauseWalkCooldown = 0.25f;
            anim.SetInteger("Status",1);
            yield return new WaitForSeconds(0.1f);
            anim.SetInteger("Status",0);
            yield return 0;
            yield return new WaitUntil(()=>anim.GetInteger("Status")!=3);
            bulletShooter.Shoot();
            yield return new WaitForSeconds(0.4f);

        }
    }
    #endregion

    [Serializable]
    public class MainEvent : UnityEvent {}

    [SerializeField]
    MainEvent deathEvent = new MainEvent();
    public MainEvent onDeathEvent {get { return deathEvent; } set { deathEvent = value; } }
}
