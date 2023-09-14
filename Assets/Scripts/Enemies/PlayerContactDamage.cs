using UnityEngine;

public class PlayerContactDamage : MonoBehaviour
{
    public bool active = true;
    Transform child;
    BulletMovement bulletMovement;
    void Start()
    {
        child = transform.GetChild(0);
        bulletMovement = GetComponent<BulletMovement>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag=="Player" && active && ( child == null || Mathf.Abs(child.lossyScale.x) >= DataShare.bulletLifeMinContact))
        {
            DMG(other.transform);
        }
    }
    public void DMG(Transform other)
    {
        if(active)
        {
            if(bulletMovement != null)
            {
                if(bulletMovement.DisableBullet(true,other.GetComponent<Renderer>(),true,false,false))
                {
                    bulletMovement.impactEvent?.Invoke();
                }
            }
            other.GetComponent<PlayerControl>().HurtEvent((Vector2)(other.position-transform.position));
        }
    }
}
