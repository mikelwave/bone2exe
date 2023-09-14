using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class CurveScaleAnimator : CurveAnimator
{
    public Vector3 scaleMultiplier = Vector3.one;
    Transform child;
    void Start()
    {
        if(transform.childCount != 0)
        child = transform.GetChild(0);
        else child = transform;
    }
    // Update is called once per frame
    void LateUpdate()
    {
        #if UNITY_EDITOR
        if(!Application.isPlaying)
        {
            float p = animationCurve.Evaluate(debugProgression);
            child.localScale = scaleMultiplier * p;
        }
        else
        #endif
        child.localScale = scaleMultiplier * animationCurve.Evaluate(progression.Invoke());
    }
}
