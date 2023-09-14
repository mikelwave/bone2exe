using UnityEngine;
using TMPro;
using System.Globalization;

public class BulletShooter : MonoBehaviour
{
    [Tooltip("Used when multiple shooters are attached to the same object")]
    public byte ID = 0;
    public GameObject bulletSample;
    public GameObject impactSample;
    public int pooledBullets = 5;
    public int pooledImpacts = 4;
    public float fireRate = 0.5f;
    float curFireDelay = 0;
    [SerializeField] bool snapAimOnShot = false;
    public BulletMovement[] bullets;
    ParticleSystem[] impacts;

    Transform bulletHolder;

    public Transform shootPoint;

    float targetRotation = 0;
    float curRotation = 0;
    [SerializeField] protected bool flipDir = false;
    [SerializeField] protected int defaultOverrideDir = 0;
    [SerializeField] protected float screenShakeAmount;
    [SerializeField] protected float screenShakeTime;
    public delegate bool ShootEvent();
    public ShootEvent shootEvent;
    public void SetFireRate(TextMeshProUGUI t)
    {
        string s = t.text.Substring(0,t.text.Length-1);
        ///print("In text: "+"'"+s+"'");
        ///print("Length: "+s.Length);
        float number = float.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
        if(float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
        {
            ///print("fire rate: "+number);
            fireRate = number;
            curFireDelay = fireRate;
        }
    }
    public void SetFireRate(int value)
    {
        fireRate = value;
        curFireDelay = fireRate;
    }

    public void GetFireRate(TMP_InputField t)
    {
        t.text = fireRate.ToString();
    }
    protected virtual void Awake()
    {
        curFireDelay = fireRate;
        if(shootPoint == null)
            shootPoint = transform.GetChild(0);

        if(bulletSample == null) return;
        //Create bullet holder
        bulletHolder = new GameObject("BulletHolder").transform;
        bulletHolder.SetParent(transform);
        bulletHolder.localPosition = Vector3.zero;

        //Pool bullets
        bullets = new BulletMovement[pooledBullets];
        float radius = bulletSample.transform.GetChild(0).GetComponent<CircleCollider2D>().radius+0.1f;
        for(int i = 0;i<pooledBullets;i++)
        {
            GameObject bullet = Instantiate(bulletSample,Vector3.zero,Quaternion.identity);
            bullets[i] = bullet.GetComponent<BulletMovement>();
            bullets[i].radius = radius;
            bullets[i].impact = Impact;
            bullet.transform.SetParent(bulletHolder);
            bullets[i].SetParent(bulletHolder);
        }

        //Pool impacts
        if(impactSample!=null)
        {
            impacts = new ParticleSystem[pooledImpacts];
            for(int i = 0;i<pooledImpacts;i++)
            {
                GameObject impact = Instantiate(impactSample);
                impact.transform.SetParent(bulletHolder);
                impacts[i] = impact.GetComponent<ParticleSystem>();
                impacts[i].gameObject.SetActive(false);
            }
        }
    }
    public void AimUpdate()
    {
        if(curFireDelay<fireRate)
        {
            curFireDelay+=Time.deltaTime;
        }
    }
    //Force bullet spawn
    public void SpawnBullet(Vector3 position, Quaternion rotation,int dir)
    {
        for(int i = 0; i<bullets.Length; i++)
        {
            if(bullets[i].Available())
            {
                curFireDelay = 0;
                shootEvent?.Invoke();
                bullets[i].Spawn(position,rotation,dir);
                return;
            }
        }
        Debug.Log("Shooting failed.");
    }

    //Used by burst variant
    public virtual void Shoot(int amount,int overrideDir)
    {
    }

    //Normal shoot function
    public virtual void Shoot()
    {
        if(curFireDelay<fireRate) return;

        float dir;
        if(defaultOverrideDir != 0) dir = defaultOverrideDir;
        else dir = transform.lossyScale.x * (flipDir ? -1 : 1);
        for(int i = 0; i<bullets.Length; i++)
        {
            if(bullets[i].Available())
            {
                if(screenShakeAmount != 0) CamControl.ShakeCamera(screenShakeAmount,screenShakeTime);
                curFireDelay = 0;
                shootEvent?.Invoke();

                if(!snapAimOnShot) bullets[i].Spawn(shootPoint.position,shootPoint.rotation,dir);
                else
                {
                    bullets[i].Spawn(shootPoint.position,Quaternion.Euler(0,0,targetRotation*dir),dir);
                }
                return;
            }
        }
        Debug.Log("Shooting failed.");
    }

    //Overwrite direction variant
    public void Shoot(float dir)
    {
        if(curFireDelay<fireRate) return;
        
        if(defaultOverrideDir != 0) dir = defaultOverrideDir;

        for(int i = 0; i<bullets.Length; i++)
        {
            if(bullets[i].Available())
            {
                curFireDelay = 0;
                shootEvent?.Invoke();
                if(!snapAimOnShot) bullets[i].Spawn(shootPoint.position,shootPoint.rotation,dir);
                else
                {
                    bullets[i].Spawn(shootPoint.position,Quaternion.Euler(0,0,targetRotation*dir),dir);
                }
                return;
            }
        }
        Debug.Log("Shooting failed.");
    }
    //Spawn bullet impact
    public void Impact(Transform t,Renderer renderer)
    {
        if(impactSample==null) return;

        for(int i = 0; i<impacts.Length; i++)
        {
            if(!impacts[i].isPlaying)
            {
                if(renderer!=null)
                {
                    ParticleSystemRenderer r = impacts[i].GetComponent<ParticleSystemRenderer>();
                    r.sortingLayerID = renderer.sortingLayerID;
                    r.sortingOrder = renderer.sortingOrder-1;
                }
                Transform tr = impacts[i].transform;
                tr.rotation = t.rotation;
                tr.SetParent(null);
                tr.position = t.position;// - (t.right*t.localScale.x)*0.25f;
                tr.localScale = t.localScale;
                impacts[i].gameObject.SetActive(true);
                break;
            }
        }
    }
    //Called by player aim control or enemy aim control
    public void Aim(float angle,float speed)
    {     
        targetRotation = angle;

        curRotation = Mathf.MoveTowards(curRotation,targetRotation,speed
        *Time.fixedDeltaTime);

        transform.localEulerAngles = new Vector3(0,0,curRotation);
    }
}
