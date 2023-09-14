using System.Collections;
using UnityEngine;

public class SmalltownBehaviour : MonoBehaviour
{
    public float chargeSpeed = 10;
    public float lineOfSight = 5; // Also used as circle radius
    public float raycastIntervals = 0.25f;
    public LayerMask WhatIsVisible;

    Animator anim;
    Rigidbody2D rb;
    Transform target;

    Coroutine ChargeCor;
    bool charging = false;
    bool willTurnAround = false;
    public int startTurnsLeft = 1;
    int turnsLeft = 0;
    public float skidVelocityLossSpeed = 1f;
    Vector3 startPose;
    Vector3 startScale;
    EnemyGlobalStats enemyGlobalStats;
    GenericAdapter genericAdapter;

    [SerializeField] Material[] materials = new Material[2];
    SpriteRenderer spriteRenderer;
    byte blinkVal;

    // Trail effect
    SpriteTrailMain spriteTrailMain;
    int chargeLoopID = -1;

    [SerializeField] bool silent = false;
    IEnumerator IChargeReset()
    {
        spriteTrailMain.DeActivate();
        yield return new WaitForSeconds(0.5f);

        if(anim.GetInteger("Status")==2) yield break;
        
        anim.SetInteger("Status",0);
        yield return new WaitForSeconds(0.25f);
        Flip(-(int)transform.localScale.x);
        StopCharging();
    }

    public void AggroCharge()
    {
        charging = true;
        genericAdapter.hasHitAnimation = false;
        // Start charge
        anim.SetInteger("Status",1);
    }
    public void StopCharging()
    {
        genericAdapter.hasHitAnimation = true;
        StopSkidSound();
        CancelInvoke("CheckForTurn");
        spriteTrailMain.DeActivate();
        charging = false;
        turnsLeft = startTurnsLeft;
        rb.velocity = Vector2.zero;
    }

    // Called by animation
    public void StartCharge()
    {
        genericAdapter.hasHitAnimation = false;
        spriteTrailMain.Activate();
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.right*-transform.localScale.x*chargeSpeed,ForceMode2D.Impulse);
        if(!silent) DataShare.PlaySound("Figure_dash",transform,false);
        if(willTurnAround && turnsLeft>0)
        {
            InvokeRepeating("CheckForTurn",0.25f,0.1f);
        }
    }
    Coroutine skidCor;
    IEnumerator ISkid()
    {
        anim.SetInteger("Status",6);
        float progress = 0;
        Vector2 velocity = rb.velocity;
        if(!silent) chargeLoopID = DataShare.PlaySound("Figure_skid",transform,true);
        while(progress<1)
        {
            if(!charging)
            {
                StopCoroutine(skidCor);
                yield break;
            }
            progress+=Time.deltaTime*skidVelocityLossSpeed;
            float step = Mathf.SmoothStep(0,1,progress);
            rb.velocity = Vector2.Lerp(velocity,Vector2.zero,step);
            yield return 0;
        }
        rb.velocity = Vector2.zero;
        Flip(-(int)transform.localScale.x);
        AggroCharge();
        DataShare.StopSound(chargeLoopID);
        chargeLoopID = -1;
    }
    void CheckForTurn()
    {
        if(target==null)
        {
            CancelInvoke("CheckForTurn");
            return;
        }
        // If target is behind the transform and facing away
        if(target.position.x-transform.position.x*transform.localScale.x>0)
        {
            turnsLeft--;
            if(turnsLeft<=0) CancelInvoke("CheckForTurn");
            if(skidCor!=null)StopCoroutine(skidCor);
            skidCor = StartCoroutine(ISkid());
        }
    }
    // Called by GenericAdapter event
    public void Hit()
    {
        if(genericAdapter.hasHitAnimation)
        {
            StopCharging();
        }
        else
        {
            blinkVal = 0;
            CancelInvoke("HitBlink");
            InvokeRepeating("HitBlink",0,0.1f);
        }
    }
    void Reset()
    {
        StopCharging();
        if(ChargeCor!=null)StopCoroutine(ChargeCor);
        if(skidCor!=null)StopCoroutine(skidCor);
        turnsLeft = startTurnsLeft;
        target = null;
    }

    void LookForPlayer()
    {
        if(charging) return;
        Vector3 startPoint = transform.position+(Vector3.up*0.25f);
        RaycastHit2D ray = Physics2D.Raycast(startPoint,Vector3.right*-transform.localScale.x,lineOfSight,WhatIsVisible);
        if(ray.collider!=null)
        {
            if(ray.transform.tag=="Player")
            {
                if(anim!=null)
                {
                    target = ray.transform;
                    AggroCharge();
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
            Debug.DrawLine(startPoint,startPoint+(Vector3.right*-transform.localScale.x*lineOfSight),Color.red,raycastIntervals);
        }
        #endif

    }
    void Flip(int dir)
    {
        if(dir == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = dir;
        transform.localScale = scale;
    }
    void OnEnable()
    {
        InvokeRepeating("LookForPlayer",Random.Range(0f,raycastIntervals),raycastIntervals);
        if(rb!=null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            anim.SetInteger("Status",0);
            anim.Rebind();
            anim.Update(0f);
        }
        if(ChargeCor!=null)StopCoroutine(ChargeCor);
        if(startScale!=Vector3.zero)
        {

            transform.position = startPose;
            transform.localScale = startScale;
        }
    }
    void OnDisable()
    {
        CancelInvoke("LookForPlayer");
        if(rb==null) return;
        Reset();

        if(DeathFallWait != null)
        StopCoroutine(DeathFallWait);
        
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
    }
    Coroutine DeathFallWait;
    IEnumerator IDeathFallWait()
    {
        yield return 0;
        yield return new WaitUntil(() => Mathf.Round(rb.velocity.y) == 0);
        anim.SetInteger("Status",5);
        DeathFallWait = null;
    }
    public void DeathEvent()
    {
        StopSkidSound();
        spriteTrailMain.DeActivate();
        rb.gravityScale = 8;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        enemyGlobalStats.lockSpawn = true;
        Reset();
        enemyGlobalStats.lockSpawn = false;
        CancelInvoke("LookForPlayer");
        if(DeathFallWait != null)
        StopCoroutine(DeathFallWait);
        DeathFallWait = StartCoroutine(IDeathFallWait());
    }
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        startPose = transform.position;
        startScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();
        spriteTrailMain = transform.GetChild(1).GetComponent<SpriteTrailMain>();
        spriteTrailMain.SetSource(transform.GetChild(0).GetComponent<SpriteRenderer>());
        enemyGlobalStats = GetComponent<EnemyGlobalStats>();
        genericAdapter = GetComponent<GenericAdapter>();
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        materials[0] = spriteRenderer.material;
        if(enemyGlobalStats.type == EnemyGlobalStats.Type.Dark)
        {
            willTurnAround = true;
        }
        turnsLeft = startTurnsLeft;
    }
    void HitBlink()
    {
        blinkVal++;
        spriteRenderer.material = materials[blinkVal%2==0?0:1];
        if(blinkVal >= BossGlobalStats.maxBlinkVal) CancelInvoke("HitBlink");
    }
    void StopSkidSound()
    {
        if(chargeLoopID!=-1)
        {
            DataShare.StopSound(chargeLoopID);
            chargeLoopID = -1;
        }
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.tag == "MainMap")
        {
            int status = anim.GetInteger("Status");
            if(charging && (status==1 || status==6))
            {
                CancelInvoke("CheckForTurn");
                if(!silent) DataShare.PlaySound("Figure_wallImpact",transform.position,false);
                StopSkidSound();
                if(skidCor!=null)StopCoroutine(skidCor);
                rb.velocity = Vector2.zero;
                anim.SetInteger("Status",4);

                ChargeCor = StartCoroutine(IChargeReset());
            }
        }
    }
}
