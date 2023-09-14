using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class CurveSpriteAnimator : CurveAnimator
{
    public Sprite[] sprite;
    int spriteCount = 0;
    SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        if(transform.childCount!=0)
        {
            spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }
        if(spriteRenderer==null)spriteRenderer = GetComponent<SpriteRenderer>();
        spriteCount = sprite.Length-1;

        if(sprite.Length<2) Destroy(this);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        #if UNITY_EDITOR
        if(!Application.isPlaying)
        {
            spriteCount = sprite.Length-1;
            int p = Mathf.FloorToInt(Mathf.Clamp(animationCurve.Evaluate(debugProgression),0,spriteCount));
            ///print(p);
            spriteRenderer.sprite = sprite[p];
        }
        else
        #endif
        spriteRenderer.sprite = sprite[Mathf.FloorToInt(Mathf.Clamp(animationCurve.Evaluate(progression.Invoke()),0,spriteCount))];
    }
}
