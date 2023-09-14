using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;

public class BulletMovement : MonoBehaviour
{
    #region main
    public bool disableOnLoad = true;
    public bool destroyOnDisable = false;
    public bool disableOffscreen = true;
    public float bulletSpeed = 10;
    public float bulletLife = 1;
    public string spawnSound = "";
    public string impactSound = "Bullet_impact";
    public string expireSound = "";
    float bulletLifeCurrent = 0;
    public string[] solidBlocking;
    Vector2 direction;
    Rigidbody2D rb;
    Transform parent;
    float dir = 1;
    public float radius = 0;
    public delegate void Impact(Transform t, Renderer r);
    public Impact impact;
    public bool halfSolidStops = true;
    public bool canMove = true;
    [SerializeField] bool AutoSpawn = false;
    [SerializeField] bool constantSpeedSet = true;
    bool firstSpawn = false;
    Vector3 spawnPosition;
    public bool Available()
    {
        return !gameObject.activeInHierarchy;
    }
    public float BulletLifeProgression()
    {
        return bulletLifeCurrent/bulletLife;
    }
    public void BulletSpeedSet(float speed)
    {
        bulletSpeed = speed;
    }
    public void Spawn(Vector3 pos, Quaternion rot, float dir)
    {
        canMove = true;
        ///print(dir);
        this.dir = Mathf.Clamp(Mathf.Round(dir*10),-1,1);
        transform.SetParent(null);
        spawnPosition = pos;
        transform.position = pos;
        transform.rotation = rot;
        transform.localScale = new Vector3(dir,1,1);
        gameObject.SetActive(true);

        if(spawnSound!="") DataShare.PlaySound(spawnSound,transform.position,false);
        if(!constantSpeedSet) Movement();
    }
    public void SetParent(Transform parent)
    {
        this.parent = parent;
    }
    void Movement()
    {
        direction = transform.right * bulletSpeed * dir;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction,ForceMode2D.Impulse);
    }
    void OnDisable()
    {
        canMove = false;
        bulletLifeCurrent = 0;
    }
    IEnumerator DespawnDelay()
    {
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
        transform.SetParent(parent);
    }
    public bool DisableBullet(bool withImpact,Renderer renderer,bool withDelay,bool onLoad,bool wallImpact,bool withDisableEvent = true)
    {
        if(!gameObject.activeInHierarchy) return false;

        if(wallImpact && bulletLifeCurrent<=0.02f && !onLoad)
        {
            Vector2 moveDirection = (transform.right * bulletSpeed * dir).normalized;
            if(Mathf.Abs(moveDirection.x) != 1 && moveDirection.y > 0)
            {
                RaycastHit2D[] hits = Physics2D.CircleCastAll(spawnPosition,radius,Vector2.zero,1,128);
                
                foreach(RaycastHit2D hit in hits)
                {
                    if(hit.collider.tag == "SSMap") continue;
                    else
                    {
                        Vector2 direction = (Vector2)transform.position - hit.point;
                        ///print(hit.point + " " + transform.position + " " + direction);
                        ///Debug.DrawLine(hit.point,transform.position, Color.magenta,2f);
                        transform.position = spawnPosition + (Vector3)direction;
                        return false;
                    }
                }
            }
        }

        if(withImpact) DataShare.PlaySound(impactSound,transform.position,false);

        canMove = false;
        rb.velocity = Vector2.zero;

        if(firstSpawn)
        {
            if(withDisableEvent)
            disableEvent?.Invoke();
        }
        else firstSpawn = true;
        
        direction = Vector2.zero;
        if(withImpact)
        {
            impact?.Invoke(transform,renderer);
        }
        if(destroyOnDisable)
        {
            Destroy(gameObject);
            return true;
        }
        if(withDelay)
        {
            float newBulletLife = bulletLife*0.9f;
            if(bulletLifeCurrent<newBulletLife)
            bulletLifeCurrent = newBulletLife;
            StartCoroutine(DespawnDelay());
        }
        else
        {
            gameObject.SetActive(false);
            transform.SetParent(parent);
        }
        return true;
    }
    // Start is called before the first frame update
    public void Start()
    {
        if(rb!=null) return;
        rb = GetComponent<Rigidbody2D>();

        if(disableOnLoad) DisableBullet(false,null,false,true,false);
        if(AutoSpawn)
        {
            Spawn(transform.position,Quaternion.identity,1);
        }
    }

    // Update is called per fixed timestep
    void FixedUpdate()
    {
        bulletLifeCurrent+=Time.fixedDeltaTime;
        if(!canMove) return;

        if(bulletSpeed!=0 && constantSpeedSet)
        Movement();

        if(bulletLifeCurrent>bulletLife)
        {
            if(expireSound != "") DataShare.PlaySound(expireSound,transform.position,false);
            DisableBullet(false,null,true,false,false);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(!canMove) return;
        string tag = other.tag.ToLower();
        // Full solid
        foreach(string s in solidBlocking)
        {
            // Disable object
            if(s==tag)
            {
                if(DisableBullet(true,other.GetComponent<Renderer>(),true,false,true))
                impactEvent?.Invoke();
                break;
            }
        }
        // Half solid
        if(halfSolidStops && (direction.y < 0 || rb.velocity.y < 0) && tag == "ssmap")
        {
            if(DisableBullet(true,other.GetComponent<Renderer>(),true,false,false))
            impactEvent?.Invoke();
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag=="ObjActivator" && disableOffscreen)
        {
            DisableBullet(false,null,false,false,false);
        }
    }
    #endregion

    [Serializable]
    public class MainEvent : UnityEvent {}

    [SerializeField]
    MainEvent disableEvent = new MainEvent();
    public MainEvent impactEvent = new MainEvent();
    public MainEvent onDisableEvent {get { return disableEvent; } set { disableEvent = value; } }
    public MainEvent onImpactEvent {get { return impactEvent; } set { impactEvent = value; } }
}
