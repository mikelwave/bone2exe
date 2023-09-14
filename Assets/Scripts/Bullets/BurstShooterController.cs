using UnityEngine;

[RequireComponent (typeof (BulletBurstShooter))]
public class BurstShooterController : MonoBehaviour
{
    BulletBurstShooter bulletBurstShooter;
    public int shootAmount = 2;
    public int overrideDir = 0;
    public bool resetRotationOnShoot = true;
    public float resetRotationTo = 0;
    public bool straightAnglesOnly = true;
    public float angleDivider = 90;
    void Start()
    {
        bulletBurstShooter = GetComponent<BulletBurstShooter>();
    }
    public void Shoot()
    {
        if(resetRotationOnShoot)
        {
            if(straightAnglesOnly)
            {
                float rotation = bulletBurstShooter.shootPoint.eulerAngles.z;
                print("Rotation: "+rotation);
                rotation = Mathf.Round(rotation/angleDivider)*angleDivider;
                print("Rounded: "+rotation);
                bulletBurstShooter.shootPoint.localEulerAngles = Vector3.forward*rotation;
            }
            else
            {
                bulletBurstShooter.shootPoint.localEulerAngles = Vector3.forward*resetRotationTo;
            }
        }
        bulletBurstShooter.Shoot(shootAmount,overrideDir);
    }

}
