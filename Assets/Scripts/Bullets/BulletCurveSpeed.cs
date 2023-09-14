using UnityEngine;

[RequireComponent (typeof(CurveSpeedAnimator))]
[RequireComponent (typeof(BulletMovement))]
public class BulletCurveSpeed : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        CurveSpeedAnimator curveSpeedAnimator = GetComponent<CurveSpeedAnimator>();
        BulletMovement bulletMovement = GetComponent<BulletMovement>();
        curveSpeedAnimator.progression = bulletMovement.BulletLifeProgression;
        curveSpeedAnimator.setSpeed = bulletMovement.BulletSpeedSet;
    }
}
