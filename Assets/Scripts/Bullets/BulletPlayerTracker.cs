using UnityEngine;


[RequireComponent (typeof(BulletShooter))]
public class BulletPlayerTracker : MonoBehaviour
{
    [SerializeField] float MinDistance = 5;
    [SerializeField] bool globalScaleConstant = false;
    [SerializeField] BulletShooter[] targetShooter;
    public Transform target;
    [SerializeField] Transform source;
    
    // Start is called before the first frame update
    void Start()
    {
        if(source == null) source = transform;
        if(target == null) target = GameObject.FindWithTag("Player").transform;
        if(targetShooter.Length == 0)
        GetComponent<BulletShooter>().shootEvent = TrackPlayerNoDist;
        else
        {
            foreach(BulletShooter shooter in targetShooter)
            {
                shooter.shootEvent = TrackPlayerNoDist;
            }
        }
    }

    // Used to check if player is near enough
    public bool TrackPlayer()
    {
        Vector3 pos = source.position;
        if(Vector3.Distance(target.position,pos)>MinDistance)
        {
            return false;
        }

        Vector3 distance = target.position - pos;
        // Set the X axis rotation based on the distance between the target and the source
        source.right = distance*transform.localScale.x;

        return true;
    }

    // Used to calculate actual firing angle
    public bool TrackPlayerNoDist()
    {
        Vector3 pos = source.position;

        Vector3 distance = target.position - pos;
        // Set the X axis rotation based on the distance between the target and the source
        if(globalScaleConstant && transform.lossyScale.x < 0)
        {
            source.right = distance*transform.lossyScale.x;
        }
        else source.right = distance*transform.localScale.x;

        return true;
    }
}
