using UnityEngine;

public class PlayerAimControl : MonoBehaviour
{
    delegate void Aim(float angle,float speed);
    Aim aim;
    public float aimSpeed = 3;
    int dpadDir = 0;
    float angle = 0;
    static bool AimExtend = true;
    // Start is called before the first frame update
    void Start()
    {
        aim+=GetComponent<BulletShooter>().Aim;
        transform.parent.parent.GetComponent<PlayerControl>().playerFixedUpdateControl += AimControl;
    }

    // Called by main player script loop
    void AimControl(bool OnLadder, bool grounded)
    {
        int newdpadDir = 0;
        // Aim Extending on by default
        if(PlayerControl.freezePlayerInput == 0)
        {
            // Hold
            if(DataShare.aimControlHold)
            {
                bool prevAimExtend = AimExtend;
                AimExtend = !MGInput.GetButton(MGInput.controls.Player.ExtendAim);
                if(prevAimExtend != AimExtend) dpadDir = 0;
            }

            // Tap
            else
            {
                if(MGInput.GetButtonDown(MGInput.controls.Player.ExtendAim))
                {
                    AimExtend = !AimExtend;
                    dpadDir = 0;
                }
            }
            newdpadDir = MGInput.GetDpadYRaw(MGInput.controls.Player.Movement);
        }
        
        // Update dir
        if(newdpadDir == 0 || newdpadDir != dpadDir || OnLadder || !grounded)
        {
            dpadDir = newdpadDir;
            angle = 0;

            // On ladder: locked 0
            if(!OnLadder)
            {
                angle = 90*dpadDir/((!AimExtend)?2:1);
            }
        }
        aim?.Invoke(angle,aimSpeed);
    }
    void OnGUI()
    {
        if(!GameMaster.DebugInfo) return;
        GUI.Label(new Rect(10, Screen.height-30, 300, 20), "Aim extend: "+AimExtend);
    }
}
