using System.Collections;
using UnityEngine;

public class PlayerItemTouch : MonoBehaviour
{
    public delegate void ItemPickupEvent();
    public ItemPickupEvent itemPickupEvent;

    public delegate bool CheckIfCanPickUp();
    public CheckIfCanPickUp checkIfCanPickUp;
    public float collectSpeed = 4;
    public string pickupSound = "";
    IEnumerator IDisappear(Transform obj)
    {
        float progress = 0;
        Vector3 startScale = obj.localScale;
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*collectSpeed;
            obj.localScale = Vector3.Lerp(startScale,Vector3.zero,progress);
            yield return 0;
        }
        Destroy(gameObject);
    }
    void Collect()
    {
        GetComponent<ObjectEnabler>().canDespawn = false;
        if(pickupSound!="") DataShare.PlaySound(pickupSound,transform.position,false);
        Transform child = transform.GetChild(0);
        child.GetComponent<BoxCollider2D>().enabled = false;
        child.GetChild(0).GetComponent<ParticleSystem>().Stop(true,ParticleSystemStopBehavior.StopEmitting);
        child.GetChild(0).SetParent(null);
        StartCoroutine(IDisappear(child));
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only pick up if player is eligible
        if(other.tag=="Player" && checkIfCanPickUp.Invoke())
        {
            Collect();
            itemPickupEvent?.Invoke();
        }
    }
}
