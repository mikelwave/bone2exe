using UnityEngine;
using System;
using UnityEngine.Events;

public class BulletBurstShooter : BulletShooter
{
    #region main
    [Range (0,360)]
    public int coneSize = 180;
    delegate void SpawnBulletCallback (Vector3 position, Quaternion rotation,int dir);
    SpawnBulletCallback spawnBullet;
    [SerializeField] bool RoundSpawnPos = false;
    [SerializeField] bool resetAngleOnShoot = false;
    [SerializeField] float coneOffset = 0;
    [SerializeField] int defaultShootAmount = 1;
    protected override void Awake()
    {
        base.Awake();
        // If local bullet sample does not exist, use GameMaster global bullets
        spawnBullet = bulletSample == null ? GameMaster.SpawnBullet : SpawnBullet;

        if(shootPoint == null) shootPoint = transform.GetChild(0);
    }

    // Called from animation or externally
    public override void Shoot(int amount,int overrideDir)
    {
        if(resetAngleOnShoot)
        {
            transform.localEulerAngles = Vector3.zero;
            shootPoint.eulerAngles = Vector3.zero;
        }
        overrideDir = overrideDir == 0 ? (int)transform.lossyScale.x * (flipDir ? -1 : 1) : overrideDir;

        ShootMain(overrideDir,amount);
    }

    public override void Shoot()
    {
        if(resetAngleOnShoot) transform.localEulerAngles = Vector3.zero;
        int RoundedLossyX = (int)Mathf.RoundToInt(transform.lossyScale.x);
        int overrideDir = defaultOverrideDir == 0 ? (RoundedLossyX * (flipDir ? -1 : 1)) : defaultOverrideDir;
  
        ShootMain(overrideDir,defaultShootAmount);
    }
    void ShootMain(int overrideDir,int amount)
    {
        if(screenShakeAmount != 0) CamControl.ShakeCamera(screenShakeAmount,screenShakeTime);
        // Single shot or unsupported amount
        if(amount<=1)
        {
            amount = 1;
            spawnBullet?.Invoke(shootPoint.position,Quaternion.Euler(0,0,shootPoint.localEulerAngles.z),overrideDir);
            return;
        }
        // Calculate rotation offsets for each spawned bullet based on amount to spawn and cone size
        float halfSize = coneSize/2;
        Vector2 range = new Vector2(-coneSize+halfSize,coneSize-halfSize);
        float baseAngle = (shootPoint.localEulerAngles.z * overrideDir) + (coneOffset * overrideDir);

        // Do not close cone edges when full circle
        float divider = coneSize!=360 ? (float)(amount-1) : (float)amount;

        Vector3 spawnPoint = shootPoint.position;
        if(RoundSpawnPos)
        {
            if(spawnPoint.y<0)
            {
                spawnPoint.y = Mathf.Ceil(spawnPoint.y+0.27f) - 0.5f;
            }
            else
            {
                spawnPoint.y = Mathf.Floor(spawnPoint.y) + 0.5f;
            }
        }
        int spawned = 0;
        
        while(spawned<amount)
        {
            spawned++;

            float angle = baseAngle + Mathf.Lerp(range.x,range.y,(float)(amount-spawned)/divider);

            spawnBullet?.Invoke(spawnPoint,Quaternion.Euler(0,0,angle),overrideDir);
            
            ///print("Spawned "+spawned+" angle: "+angle);
        }
    }
    public void InvokeMainEvent()
    {
        OnMainEvent?.Invoke();
    }
    #endregion
    [Serializable]
    public class MainEvent : UnityEvent {};
    [SerializeField]
    MainEvent mainEvent = new MainEvent();
    public MainEvent OnMainEvent { get { return mainEvent; } set { mainEvent = value; }}
}
