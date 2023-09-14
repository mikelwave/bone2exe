using UnityEngine;

public class MoveToPointCurve : MoveToPoint
{
    [SerializeField]
    AnimationCurve XMultiplierCurve;

    public AnimationCurve YMultiplierCurve;
    protected override void OnEnable()
    {
        if(updateEvent == null)
        {
            updateEvent += XMultiplierCurve.keys.Length != 0 ? UpdateX : null;
            updateEvent += YMultiplierCurve.keys.Length != 0 ? UpdateY : null;
        }

        base.OnEnable();
    }
    void OnDestroy()
    {
        updateEvent = null;
    }
    void UpdateX()
    {
        XMultiplier = XMultiplierCurve.Evaluate(progress);
    }
    void UpdateY()
    {
        YMultiplier = YMultiplierCurve.Evaluate(progress);
    }
    #if UNITY_EDITOR
    float GetX(float progress)
    {
        return XMultiplierCurve.keys.Length == 0 ? 1 : XMultiplierCurve.Evaluate(progress);
    }
    float GetY(float progress)
    {
        return YMultiplierCurve.keys.Length == 0 ? 1 : YMultiplierCurve.Evaluate(progress);
    }
    protected override void OnDrawGizmosSelected()
    {
        if(Application.isPlaying) return;
        // Draws a blue line from this transform to the target
        Gizmos.color = Color.blue;
        Vector2 startPoint = transform.position;
        Vector2 prevPoint = startPoint;
        const int curveDetail = 10;
        float step = 1/(float)curveDetail;
        float progress = 0;
        int opCount = 0;
        while(progress <= 1)
        {  
            Vector2 newPoint;
            newPoint.x = Mathf.Lerp(startPoint.x,targetPoint.x*GetX(progress),progress);
            newPoint.y = Mathf.Lerp(startPoint.y,targetPoint.y*GetY(progress),progress);
            Gizmos.DrawLine(prevPoint, newPoint);
            progress += step;
            prevPoint = newPoint;
            opCount++;
        }
        Gizmos.DrawLine(prevPoint, targetPoint);
    }
    #endif
}
