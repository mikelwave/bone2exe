using UnityEngine;

[RequireComponent (typeof(CurveTrailSize))]
[RequireComponent (typeof(BulletMovement))]
public class BulletCurveTrail : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<CurveTrailSize>().progression = GetComponent<BulletMovement>().BulletLifeProgression;
    }
}
