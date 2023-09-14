using UnityEngine;

public class CurveAnimator : MonoBehaviour
{
    public delegate float Progression();
    public Progression progression;
    public AnimationCurve animationCurve;

    #if UNITY_EDITOR
    [Range(0,1)]
    public float debugProgression = 0;
    #endif
}
