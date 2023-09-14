using UnityEngine;

[ExecuteInEditMode]
public class BackgroundScaler : MonoBehaviour
{
    // For scale = 1
    public Vector2 sourceScale = new Vector3(3.967f,1.11f,0);
    public Vector3 Offset = Vector2.zero;
    public bool moveX = true;
    public bool moveY = true;

    #if UNITY_EDITOR
    public bool workInEditMode = false;
    #endif
    void Scale()
    {
        // Get Z scale, set position
        float depth = Offset.z;
        float scale = transform.localScale.x;
        Vector3 pos = Vector3.zero;
        pos.z = depth;
        if(moveX) pos.x = (sourceScale.x)*(depth) + Offset.x;
        if(moveY) pos.y = (sourceScale.y)*(depth) + Offset.y;
        transform.localPosition = pos;
        transform.localScale = Vector3.one * (depth/10+1);
    }
    #if UNITY_EDITOR
    // Start is called before the first frame update
    void Awake()
    {
        Scale();
    }
    #endif
    void Start()
    {
        #if UNITY_EDITOR
        if(Application.isPlaying)
        #endif
        Scale();
    }
    #if UNITY_EDITOR
    void Update()
    {
        if(workInEditMode && !Application.isPlaying)
        Scale();
        else if(!workInEditMode && !Application.isPlaying)
        {
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
        }
    }
    #endif

}
