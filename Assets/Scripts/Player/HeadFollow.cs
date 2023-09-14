using UnityEngine;

public class HeadFollow : MonoBehaviour
{
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    Vector3 worldPos;
    [SerializeField] Transform target;
    // Start is called before the first frame update
    void Start()
    {
        target = transform.parent;
        worldPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 desiredPos = target.position + offset;
        Vector3 smoothedPos = Vector3.Lerp(worldPos,desiredPos,smoothSpeed*Time.fixedDeltaTime);
        
        if(Vector3.Distance(worldPos,desiredPos) > 2)
        {
            worldPos = desiredPos;
        }
        else worldPos = smoothedPos;

        transform.position = worldPos;
    }
}
