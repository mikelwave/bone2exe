using System.Collections;
using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    public bool bounce = false;
    public delegate void BounceEvent();
    public BounceEvent bounceEvent;

    // Function that gets called for taking damage and checking if hit was lethal
    public delegate bool HitEvent(byte damage,float XPos);
    public HitEvent hitEvent;

    public delegate void FailHitEvent();
    public FailHitEvent failHitEvent;

    // Callback function that is called with true or false based on whether hit was lethal, after the hit operation, used by bosses
    public delegate void PostHitEvent(bool lethal,Vector2 bulletPos);
    public PostHitEvent posthitEvent;
    
    SpriteRenderer spriteRenderer;
    public bool canBeTouched = true;
    public bool canDealDamage = true;
    public bool canTakeDamage = true; // Whether the entity can take damage at all
    public bool spawnGore = true;
    bool canGetHit = true; // Used for post hit invincibility frames
    bool startCanGetHit = true;
    public bool GivesPoint = true;
    public float hitCooldownTime = 0.2f;
    public string DeathSound = "";
    public void SetDealDamage(bool toggle)
    {
        canDealDamage = toggle;
    }
    public void SetBeTouched(bool toggle)
    {
        canBeTouched = toggle;
    }
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startCanGetHit = canGetHit;
    }
    void GoreHit()
    {
        GameMaster.SpawnGore(transform.position);
    }
    void OnDisable()
    {
        if(cor!=null)StopCoroutine(cor);
        canGetHit = startCanGetHit;
    }
    Coroutine cor;
    IEnumerator IHitCooldown()
    {
        canGetHit = false;
        yield return new WaitForSeconds(hitCooldownTime);
        canGetHit = true;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(!canBeTouched) return;
        string s = other.tag.ToLower();
        Vector3 pos = transform.position;
        if(s=="player")
        {
            PlayerControl playerControl = other.GetComponent<PlayerControl>();
            if(playerControl == null) return;
            if(bounce && PlayerControl.freezePlayerInput == 0 && playerControl.YVelocity<0 && other.transform.position.y+0.1f>pos.y)
            {
                // Bounce off of enemy
                playerControl.Bounce();
                bounceEvent?.Invoke();
            }
            else
            {
                if(!canDealDamage) return;
                // Damage player
                playerControl.HurtEvent((Vector2)(other.transform.position-pos));
            }
        }

        if(s.Contains("playerbullet"))
        {
            BulletMovement bulletMovement = other.transform.parent.GetComponent<BulletMovement>();
            if(!bulletMovement.canMove) return;

            bulletMovement.DisableBullet(true,spriteRenderer,true,false,false);
            if(!canTakeDamage)
            {
                failHitEvent?.Invoke();
                return;
            }
            if(!canGetHit) return;

            // If hit was lethal
            HitFunc(other.transform, pos, (byte)(CheatInput.SuperWeps ? 10 : ((s.Contains("dark")?1:2)) * (GameMaster.superModeTime > 0 ? 2 : 1)));
        }
    }
    public void HitFunc(Transform other, Vector3 pos, byte damage)
    {
        if(hitEvent.Invoke(damage,pos.x-other.position.x))
        {
            if(spawnGore) GoreHit();
            if(GivesPoint) GameMaster.EnemiesKilled++;
            GivesPoint = false;

            posthitEvent?.Invoke(true,other.position);
                DataShare.PlaySound("Enemy_hit",pos,false);
            if(DeathSound == "") DataShare.PlaySound("enemy_die"+(Random.Range(1,4).ToString()),pos,false);
            else DataShare.PlaySound(DeathSound,pos,false);
        }
        else
        {
            if(cor!=null)StopCoroutine(cor);
            if(gameObject.activeInHierarchy)
            cor = StartCoroutine(IHitCooldown());

            posthitEvent?.Invoke(false,other.position);

            DataShare.PlaySound("Enemy_hit",pos,false);
        }
    }
}
