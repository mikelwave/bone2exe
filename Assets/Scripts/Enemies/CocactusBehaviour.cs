using System.Collections;
using UnityEngine;

public class CocactusBehaviour : MonoBehaviour
{
    // Normal variant only does a raycast pointing straight
    // Dark world variant when active will angle raycasts at player when he is close
    public float lineOfSight = 5;
    public float raycastIntervals = 0.25f;
    public LayerMask WhatIsVisible;
    Animator anim;
    Transform target;
    BulletBurstShooter burstShooter;
    EnemyGlobalStats enemyGlobalStats;
    bool oddSound = false;
    void Start()
    {
        anim = transform.parent.GetComponent<Animator>();
        burstShooter = transform.parent.GetComponent<BulletBurstShooter>();
    }

    void OnEnable()
    {
        InvokeRepeating("LookForPlayer",Random.Range(0f,raycastIntervals),raycastIntervals);
        
        if(enemyGlobalStats==null) enemyGlobalStats = transform.parent.GetComponent<EnemyGlobalStats>();
        // For dark mode always aims upwards and shoots 2 bullets
        if(enemyGlobalStats.type==EnemyGlobalStats.Type.Light) transform.GetChild(0).localEulerAngles = Vector3.zero;
        else transform.GetChild(0).localEulerAngles=Vector3.forward*90;

    }
    void OnDisable()
    {
        if(ResetCor!=null)StopCoroutine(ResetCor);
        target = null;
        CancelInvoke();
    }

    void LookForPlayer()
    {
        if(target!=null) return;
        Vector3 startPoint = transform.position-(Vector3.up*0.15f);
        RaycastHit2D ray = Physics2D.Raycast(startPoint,Vector3.right*-transform.parent.localScale.x,lineOfSight,WhatIsVisible);
        if(ray.collider!=null)
        {
            if(ray.transform.tag=="Player")
            {
                if(anim!=null)
                {
                    oddSound = false;
                    target = ray.transform;
                    anim.SetInteger("Status",1);
                    if(ResetCor!=null)StopCoroutine(ResetCor);
                    ResetCor = StartCoroutine(Reset());
                }


                #if UNITY_EDITOR
                Debug.DrawLine(startPoint,ray.point,Color.green,raycastIntervals);
                #endif
            }
            #if UNITY_EDITOR
            else
            {
                Debug.DrawLine(startPoint,ray.point,Color.red,raycastIntervals);
            }
            #endif
        }
        #if UNITY_EDITOR
        else
        {
            Debug.DrawLine(startPoint,startPoint+(Vector3.right*-transform.parent.localScale.x*lineOfSight),Color.red,raycastIntervals);
        }
        #endif
    }
    // Called by event
    public void DeathSpit()
    {
        EnemyGlobalStats.Type type = enemyGlobalStats.type;
        // For light mode point the shooting direction upwards
        if(type==EnemyGlobalStats.Type.Light) transform.GetChild(0).localEulerAngles=Vector3.forward*90;

        burstShooter.Shoot(type==EnemyGlobalStats.Type.Light ? 2 : 5,1);
    }
    // Called by burst shooter event
    public void ModeFire()
    {
        DataShare.PlaySound("Plant_spit"+(oddSound?"2":""),transform.position,false);
        oddSound = !oddSound;
        burstShooter.Shoot(enemyGlobalStats.type==EnemyGlobalStats.Type.Light ? 1 : 2,0);
    }
    Coroutine ResetCor;
    // Reset when shoot animation ends
    IEnumerator Reset()
    {
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(()=>anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        target = null;
        anim.SetInteger("Status",0);
    }
}
