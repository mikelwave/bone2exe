using UnityEngine;

public class RotateTowardsPlayer : MonoBehaviour
{
    [SerializeField] bool globalScaleConstant = false;
    [SerializeField] float smoothSpeed = 0.125f;
    Vector3 worldAngle;
    public Transform target;
    [SerializeField] Transform source;
    
    // Start is called before the first frame update
    void Start()
    {
        if(source == null) source = transform;
        if(target == null) target = GameObject.FindWithTag("Player").transform;

        worldAngle = source.right;

    }

    // Used to calculate actual firing angle
    void FixedUpdate()
    {
        Vector3 pos = source.position;

        Vector3 distance = target.position+(Vector3.up*0.5f) - pos;

        Vector3 desiredAngle;
        // Set the X axis rotation based on the distance between the target and the source
        if(globalScaleConstant && transform.lossyScale.x < 0)
        {
            desiredAngle = distance*transform.lossyScale.x;
        }
        else desiredAngle = distance*transform.localScale.x;

        Vector3 smoothedAngle = Vector3.Lerp(worldAngle,desiredAngle,smoothSpeed*Time.fixedDeltaTime);

        worldAngle = smoothedAngle;
        source.right = worldAngle;
    }
}