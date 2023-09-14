using System.Collections;
using UnityEngine;

public class MrMixBubbleProjectile : MonoBehaviour
{
    Vector3 startPos;
    Transform target;
    Animator anim;

    [SerializeField] float speed = 2;
    [SerializeField] float trackingSpeed = 2;
    [SerializeField] float smoothTime = 1;

    delegate void UpdateEvent();
    UpdateEvent updateEvent;

    Vector3 velocity;
    // Start is called before the first frame update
    void Start()
    {
        if(anim!=null) return;
        anim = GetComponent<Animator>();
        startPos = transform.position;
    }
    void OnEnable()
    {
        target = GameObject.FindWithTag("Player").transform;

        if(anim == null) Start();

        anim.Rebind();
        anim.Update(0f);
        anim.SetInteger("Status",0);

        StartCoroutine(IAttack());
    }
    public void Disable()
    {
        gameObject.SetActive(false);
    }
    void TrackTarget()
    {
        Vector3 currentPos = transform.position;
        transform.position = Vector3.SmoothDamp(currentPos,new Vector3(target.position.x,currentPos.y,currentPos.z),ref velocity,smoothTime,trackingSpeed,Time.deltaTime);
    }
    void Update()
    {
        updateEvent?.Invoke();
    }
    IEnumerator IAttack()
    {
        transform.position = startPos;
        Vector3 currentPos;
        yield return 0;

        gameObject.SetActive(true);
        yield return new WaitForSeconds(0.75f);

        // Track player for 3 seconds
        updateEvent = TrackTarget;
        yield return new WaitForSeconds(2f);
        updateEvent = null;

        // Fall down (ray to ground)
        DataShare.PlaySound("MrMix_BubbleReady",transform.position,false);
        currentPos = transform.position;
        anim.SetInteger("Status",1);

        // Raycast point
        RaycastHit2D rayPoint = Physics2D.Raycast(currentPos,Vector2.down,Mathf.Infinity,128);
        float point = 0;
        bool impact = false;
        if(rayPoint.collider != null)
        {
            point = rayPoint.point.y+1.5f;
            impact = true;
            Debug.DrawLine(currentPos,rayPoint.point,Color.red,2f);
        }

        else
        {
            point = ((Vector2)currentPos + Vector2.down*20).y;
        }
        yield return new WaitForSeconds(0.45f);
        DataShare.PlaySound("MrMix_BubbleFall",transform.position,false);

        // Lerp to ground
        float progress = 0;

        while(progress<1)
        {
            progress+=Time.deltaTime*speed;

            transform.position = new Vector3(currentPos.x,Mathf.Lerp(currentPos.y,point,progress),currentPos.z);
            yield return 0;
        }
        if(!impact)
        {
            Disable();
            yield break;
        }

        // Impact section
        anim.SetInteger("Status",2);
        DataShare.PlaySound("MrMix_BubbleLand",transform.position,false);
    }
}
