using UnityEngine;

[ExecuteInEditMode]
public class CurveColorAnimator : CurveAnimator
{
    [SerializeField] Color ColorA;
    [SerializeField] Color ColorB;
    SpriteRenderer render;
    void Start()
    {
        if(transform.childCount != 0)
        render = transform.GetChild(0).GetComponent<SpriteRenderer>();
        else render = transform.GetComponent<SpriteRenderer>();
    }
    // Update is called once per frame
    void LateUpdate()
    {
        #if UNITY_EDITOR
        if(!Application.isPlaying)
        {
            float p = animationCurve.Evaluate(debugProgression);
            render.color = Color.Lerp(ColorA,ColorB,p);
        }
        else
        #endif
        render.color = Color.Lerp(ColorA,ColorB,animationCurve.Evaluate(progression.Invoke()));
    }
}
