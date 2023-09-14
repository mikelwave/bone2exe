using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerContactDamage))]
public class Laser : MonoBehaviour
{
    bool active = false;

    [SerializeField] bool startOff = false;
    [SerializeField] float laserLength = 40;
    [SerializeField] float visualOffset = 0.2f;
    [Tooltip ("The amount to time to keep the laser on. 0 = Infinity.")]
    [SerializeField] float laserOnTime = 0;
    [SerializeField] LayerMask hitMask;
    [SerializeField] Sprite[] impactSprites;

    SpriteRenderer spriteRenderer;
    SpriteRenderer impactSpriteRenderer;
    Transform impactTransform;
    PlayerContactDamage playerContactDamage;
    Vector2 size;
    public float distance;
    public delegate void LaserUpdateCallback();
    public LaserUpdateCallback laserUpdateCallback;
    Coroutine LaserSpawnCor;
    bool despawning = false;
    public void Despawn()
    {
        LaserSpawn(false);
        LaserSpawnCor = StartCoroutine(ILaserDespawn());
    }
    void LaserSpawn(bool newCycle)
    {
        if(LaserSpawnCor != null) StopCoroutine(LaserSpawnCor);
        if(newCycle) LaserSpawnCor = StartCoroutine(ILaserSpawn());
    }
    IEnumerator ILaserDespawn()
    {
        despawning = true;
        playerContactDamage.active = false;
        float speed = 10;
        float progress = 0;
        yield return 0;
        while(progress<1)
        {
            spriteRenderer.size = new Vector2(spriteRenderer.size.x,Mathf.Lerp(size.y,0,progress));
            progress+=Time.deltaTime*speed;
            yield return 0;
        }
        byte impactSprite = 0;
        float timeThreshold = 0.025f;
        float time = 0;
        while(impactSprite<impactSprites.Length)
        {
            time += Time.deltaTime;
            if(time >= timeThreshold)
            {
                time -= timeThreshold;
                impactSprite++;
                if(impactSprite<impactSprites.Length)
                impactSpriteRenderer.sprite = impactSprites[impactSprite];
            }
            spriteRenderer.size = new Vector2(distance,spriteRenderer.size.y);
            yield return 0;
        }
        active = false;
        gameObject.SetActive(false);
        spriteRenderer.size = new Vector2(size.x,size.y);
        despawning = false;
    }
    IEnumerator ILaserSpawn()
    {
        despawning = false;
        float startSize = 0;
        float speed = 10;
        float progress = 0;
        spriteRenderer.size = new Vector2(0,size.y);
        yield return 0;
        while(progress<1)
        {
            spriteRenderer.size = new Vector2(Mathf.Lerp(startSize,distance,progress),spriteRenderer.size.y);
            progress+=Time.deltaTime*speed;
            yield return 0;
        }
        playerContactDamage.active = true;
        active = true;
        byte impactSprite = 0;
        float timeThreshold = 0.05f;
        float time = 0;
        float sineTime = 0;
        Vector2 sizeSine = new Vector2(spriteRenderer.size.y*0.5f,spriteRenderer.size.y);
        
        if(laserOnTime != 0)
            Invoke("Despawn",laserOnTime);

        while(true)
        {
            time += Time.deltaTime;
            sineTime = Mathf.Repeat(sineTime+(Time.deltaTime*40),Mathf.PI*2);
            if(time >= timeThreshold)
            {
                time -= timeThreshold;
                impactSprite = (byte)Mathf.Repeat(impactSprite+=1,2);
                impactSpriteRenderer.sprite = impactSprites[impactSprite];
                spriteRenderer.size = new Vector2(spriteRenderer.size.x,Mathf.Lerp(sizeSine.x,sizeSine.y,Mathf.Abs(Mathf.Sin(sineTime))));
            }
            spriteRenderer.size = new Vector2(distance,spriteRenderer.size.y);
            yield return 0;
        }
    }
    void OnDisable()
    {
        active = false;
        LaserSpawn(false);
    }
    public void Toggle(bool enable)
    {
        if(spriteRenderer == null) Start();
        spriteRenderer.enabled = enable;
        if(enable) LaserSpawn(true);
        else impactSpriteRenderer.sprite = null;
    }
    void Start()
    {
        if(spriteRenderer != null) return;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerContactDamage = GetComponent<PlayerContactDamage>();
        spriteRenderer = spriteRenderer != null ? spriteRenderer : transform.GetChild(0).GetComponent<SpriteRenderer>();
        impactSpriteRenderer = transform.GetChild(transform.childCount-1).GetComponent<SpriteRenderer>();
        impactTransform = impactSpriteRenderer.transform;
        size = spriteRenderer.size;
        Toggle(!startOff);
    }
    void FixedUpdate()
    {
        if(despawning) return;
        RaycastHit2D[] rays = Physics2D.RaycastAll(transform.position,transform.right,laserLength,hitMask);

        #if UNITY_EDITOR
        Debug.DrawLine(transform.position,transform.right*laserLength,rays.Length != 0 ? Color.green : Color.red);
        #endif

        if(rays.Length!=0)
        {
            foreach(RaycastHit2D ray in rays)
            {
                Transform tr = ray.collider.transform;
                // Hurt player
                if(active && tr.tag == "Player")
                {
                    if(PlayerControl.currentHealth < 0) continue;
                    
                    playerContactDamage.DMG(tr);
                }
                else if(tr.tag == "SSMap") continue;
                
                distance = visualOffset + Vector2.Distance((Vector2)transform.position,ray.point);
                impactTransform.localPosition = Vector3.right*distance;
                laserUpdateCallback?.Invoke();
                break;
            }
        }
        else
        {
            distance = visualOffset + laserLength;
            impactTransform.localPosition = Vector3.right*distance;
            laserUpdateCallback?.Invoke();
        }
    }
}
