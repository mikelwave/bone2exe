using UnityEngine;
using System;
using UnityEngine.Events;

// Generic script for enemies getting hit and killed
[RequireComponent (typeof (EnemyGlobalStats))]
[RequireComponent (typeof (Animator))]
public class GenericAdapter : MonoBehaviour
{
    #region main
    [Tooltip("Whether to play hit animation or not.")]
    public bool hasHitAnimation = true;
    public bool executeHitEvent = true;
    [Tooltip("Play the death animation automatically on lethal hit.")]
    public bool automaticDeathPose = true;
    public bool isBoss = false;
    bool wasKilled = false;
    EnemyHit enemyHit;
    EnemyGlobalStats enemyGlobalStats;
    Animator anim;
    SpriteRenderer spriteRenderer;
    void OnEnable()
    {
        if(enemyGlobalStats==null)return;

        if(enemyGlobalStats.respawns)
        {
            // Set material
            if(wasKilled && spriteRenderer == null)
            {
                spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
                spriteRenderer.material = GameMaster.self.enemyRespawnMaterial;
            }
            anim.SetInteger("Status",0);
            enemyHit.canBeTouched = true;
            respawnEvent.Invoke();
        }
        else
        {
            if(!enemyHit.canBeTouched && !isBoss)
            {
                anim.SetInteger("Status",2);
            }
            else respawnEvent.Invoke();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        enemyHit = transform.GetChild(0).GetComponent<EnemyHit>();
        enemyHit.hitEvent += GenericHit;

        enemyGlobalStats = GetComponent<EnemyGlobalStats>();
        anim = GetComponent<Animator>();
    }

    // Generic hit behaviour
    bool GenericHit(byte damage,float XPos)
    {        
        // Kill
        if(enemyGlobalStats.Hurt(damage))
        {
            wasKilled = true;
            enemyHit.canBeTouched = false;
            if(automaticDeathPose)
            anim.SetInteger("Status",2);
            deathEvent.Invoke();
            return true;
        }
        else
        {
            if(hasHitAnimation)
            {
                anim.Rebind();
                anim.Update(0f);
                //Hurt
                anim.SetInteger("Status",3);
            }
            if(executeHitEvent)
            hitEvent.Invoke();
            return false;
        }
    }
    public void SetStatus(int status)
    {
        anim.SetInteger("Status",status);
    }
    public void AnimEvent()
    {
        animEvent?.Invoke();
    }
    #endregion
    [Serializable]
    public class MainEvent : UnityEvent {};
    [SerializeField]
    MainEvent deathEvent = new MainEvent();

    [SerializeField]
    MainEvent hitEvent = new MainEvent();
    [SerializeField]
    MainEvent respawnEvent = new MainEvent();
    [SerializeField]
    MainEvent animEvent = new MainEvent();
    public MainEvent OnHitEvent { get { return hitEvent; } set { hitEvent = value; }}
    public MainEvent OnDeathEvent { get { return deathEvent; } set { deathEvent = value; }}
    public MainEvent OnRespawnEvent { get { return respawnEvent; } set { respawnEvent = value; }}
    public MainEvent OnAnimEvent { get { return animEvent; } set { animEvent = value; }}
}
