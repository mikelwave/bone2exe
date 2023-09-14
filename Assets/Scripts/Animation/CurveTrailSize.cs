using UnityEngine;

public class CurveTrailSize : MonoBehaviour
{
    public delegate float Progression();
    public AnimationCurve animationCurve;
    public Progression progression;
    TrailRenderer trailRenderer;
    // Start is called before the first frame update
    void Awake()
    {
        if(transform.childCount!=0)
        {
            trailRenderer = transform.GetChild(0).GetComponent<TrailRenderer>();
            if(trailRenderer == null) this.enabled = false;
        }
        else this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        trailRenderer.widthMultiplier = animationCurve.Evaluate(progression.Invoke());
    }
}
