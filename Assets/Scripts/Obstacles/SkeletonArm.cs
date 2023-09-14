using UnityEngine;

public class SkeletonArm : MonoBehaviour
{
    Animator anim;
    bool activated = false;
    Vector3 pos;
    Transform target;
    // Start is called before the first frame update
    void Start()
    {
        anim = transform.parent.GetComponent<Animator>();
        pos = transform.parent.position;
    }
    void OnDisable()
    {
        if(anim == null) return;
        
        activated = false;
        anim.speed = 1;
    }
    void OnEnable()
    {
        if(anim == null) return;

        anim.Rebind();
        anim.Update(0f);
    }
    void DetectIfLeft()
    {
        if(Vector3.Distance(pos,target.position)>=1f)
        {
            anim.SetInteger("Status",2);
            DataShare.PlaySound("SkeletonArm_raise",transform.position,false);
            anim.speed = Random.Range(0.75f,1f);
            CancelInvoke("DetectIfLeft");
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(!activated && other.tag == "Player")
        {
            target = other.transform;
            activated = true;
            anim.SetInteger("Status",1);
            InvokeRepeating("DetectIfLeft",0,Time.fixedDeltaTime);
        }
    }
}
