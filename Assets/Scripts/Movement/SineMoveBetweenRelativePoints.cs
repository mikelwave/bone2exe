using UnityEngine;
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif

public class SineMoveBetweenRelativePoints : MonoBehaviour
{
    public Vector2 RelativePoint1,RelativePoint2;
    public float sineSpeed = 1;
    float sineProgress = Mathf.PI;
    Vector3 startOffset = Vector3.zero;
    #if UNITY_EDITOR
    [SerializeField] bool drawLine = true;
    #endif
    void Start()
    {
        startOffset = transform.localPosition;
        
    }
    public void Reset()
    {
        sineProgress = 0;
        transform.localPosition = startOffset + (Vector3)Vector2.Lerp(RelativePoint1,RelativePoint2,(Mathf.Sin(sineProgress)+1)/2);
    }

    // Update is called once per time step
    void FixedUpdate()
    {
        #if UNITY_EDITOR
        if(Application.isPlaying)
        #endif
        {
            Vector3 pos;
            sineProgress = Mathf.Repeat(sineProgress+(Time.fixedDeltaTime*sineSpeed),Mathf.PI*2);
            pos = Vector2.Lerp(RelativePoint1,RelativePoint2,(Mathf.Sin(sineProgress)+1)/2);
            pos.z = 0;
            transform.localPosition = pos+startOffset;
        }
    }
    #if UNITY_EDITOR
    void Update()
    {
        if(!Application.isPlaying && drawLine)
        {
            Vector2 pos = (Vector2)transform.position;
            Debug.DrawLine(RelativePoint1+pos,RelativePoint2+pos,Color.red);
        }
    }
    #endif
}
