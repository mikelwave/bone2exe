using UnityEngine;

public class CurveSpeedAnimator : CurveAnimator
{
    public delegate void SetSpeed(float speed);
    public SetSpeed setSpeed;
    public float speedMultiplier = 1;

    // Update is called once per frame
    void FixedUpdate()
    {
        setSpeed.Invoke(animationCurve.Evaluate(progression.Invoke())*speedMultiplier);
    }
}
