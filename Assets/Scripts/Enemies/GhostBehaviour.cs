using System.Collections;
using UnityEngine;

public class GhostBehaviour : MonoBehaviour
{
    Animator anim;
    BulletShooter bulletShooter;
    bool setTarget = false;

    void Init()
    {
        anim = GetComponent<Animator>();
        bulletShooter = transform.GetChild(0).GetComponent<BulletShooter>();
    }
    void OnEnable()
    {
        if(anim!=null)
        {
            anim.Rebind();
            anim.Update(0f);
        }
        ResetSequence();
    }
    void OnDisable()
    {
        StopSequence();
    }
    public void ResetSequence()
    {
        if(anim==null) Init();
        if(cor!=null)StopCoroutine(cor);
        cor = StartCoroutine(IShootRepeat());

    }
    public void StopSequence()
    {
        if(cor!=null)StopCoroutine(cor);
    }
    Coroutine cor;
    IEnumerator IStatusChange(int status)
    {
        yield return 0;
        anim.SetInteger("Status",status);
        ResetSequence();
    }
    IEnumerator IShootRepeat()
    {
        anim.SetInteger("Status",0);
        while(true)
        {
            yield return new WaitForSeconds(1.8f);
            if(anim.GetInteger("Status") == 0)
            {
                anim.SetInteger("Status",1);
                yield return new WaitForSeconds(0.2f);
                if(anim.GetInteger("Status") == 1)
                {
                    anim.SetInteger("Status",0);
                    if(!setTarget)
                    {
                        setTarget = true;
                        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MoveToTarget>().target = transform;
                    }
                    DataShare.PlaySound("Ghost_spit",transform.position,false);
                    bulletShooter.Shoot();
                }
            }
        }
    }
}
