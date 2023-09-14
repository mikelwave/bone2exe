using System.Collections;
using UnityEngine;

public class SatchelTick : MonoBehaviour
{
    Animator anim;
    Laser[] lasers;
    [SerializeField] GameObject SatchelExplodeEffect;
    static int BeepSoundID = -1;

    IEnumerator ISatchelCount()
    {
        float progress = 0;
        float tickInterval = 0;
        float maxTickTime = 0.5f;
        anim.SetInteger("Status", 1);

        while(progress <= 2)
        {
            progress += Time.deltaTime;
            tickInterval += Time.deltaTime;

            if(tickInterval > maxTickTime)
            {
                tickInterval = 0;
                maxTickTime -= maxTickTime/5;
                if(BeepSoundID == -1) BeepSoundID = DataShare.GetSoundID("BM_SatchelBeep");
                if(BeepSoundID != -1) DataShare.PlaySound(BeepSoundID,transform.position,false,0.5f,1f);

                anim.SetTrigger("Tick");
            }
            yield return 0;
        }
        if(BeepSoundID == -1) BeepSoundID = DataShare.GetSoundID("BM_SatchelBeep");
        if(BeepSoundID != -1) DataShare.PlaySound(BeepSoundID,transform.position,false,0.5f,1f);
        anim.SetInteger("Status", 2);
        anim.SetTrigger("Tick");
        yield return new WaitForSeconds(0.15f);
        foreach(Laser laser in lasers)
        {
            laser.Toggle(true);
        }
        Instantiate(SatchelExplodeEffect,transform.position,Quaternion.identity);
        CamControl.ShakeCamera(0.3f,0.25f);
        DataShare.PlaySound("Explode_small",transform.position,false);
        DataShare.PlaySound("BM_SatchelExplode",transform.position,false);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);

    }
    // Start is called before the first frame update
    void Start()
    {
       anim = GetComponent<Animator>(); 
       lasers = transform.GetComponentsInChildren<Laser>();
       StartCoroutine(ISatchelCount());
    }
}
