using UnityEngine;

public class CameraHeadBehaviour : MonoBehaviour
{
    Animator anim;
    BulletShooter bulletShooter;
    [SerializeField] float fireRate = 2f;
    [SerializeField] float initialFireDelay = 2f;
    float dir = 0;
    public bool flipDir = true;
    [SerializeField] string shootSound = "";

    Renderer render;

    void Init()
    {
        anim = GetComponent<Animator>();
        render = transform.GetChild(0).GetComponent<Renderer>();
        bulletShooter = transform.GetChild(0).GetComponent<BulletShooter>();
        dir = transform.localScale.x * (flipDir ? -1 : 1);
    }
    void ShootingLoop()
    {
        DataShare.PlaySound(shootSound,transform.position,false);
        if(render.isVisible)
        anim.SetTrigger("Shoot");
    }
    // Called by animation
    public void Shoot()
    {
        bulletShooter.Shoot(dir);
    }
    void OnEnable()
    {
        if(anim == null) Init();

        anim.Rebind();
        anim.Update(0f);

        InvokeRepeating("ShootingLoop",initialFireDelay,fireRate);
    }
    void OnDisable()
    {
        CancelInvoke("ShootingLoop");
    }
}
