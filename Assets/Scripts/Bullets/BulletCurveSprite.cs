using UnityEngine;

[RequireComponent (typeof(CurveSpriteAnimator))]
[RequireComponent (typeof(BulletMovement))]
public class BulletCurveSprite : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<CurveSpriteAnimator>().progression = GetComponent<BulletMovement>().BulletLifeProgression;
    }
}
