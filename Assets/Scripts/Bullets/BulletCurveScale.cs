using UnityEngine;

[RequireComponent (typeof(CurveScaleAnimator))]
[RequireComponent (typeof(BulletMovement))]
public class BulletCurveScale : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<CurveScaleAnimator>().progression = GetComponent<BulletMovement>().BulletLifeProgression;
    }
}
