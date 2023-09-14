using UnityEngine;

public class RotateToTarget : MonoBehaviour
{
    [SerializeField] Transform Target;
    [SerializeField] float rotationSpeed = 100;
    [SerializeField] float startRotation = -999;
    bool active = false;
    public bool ToggleRotating{ set{active = value;}}

    void OnEnable()
    {
        if(Target == null)
        {
            Target = GameObject.FindWithTag("Player").transform;
        }
        active = true;
        if(startRotation < -360)
        Rotate(true);
        else Rotate(startRotation);
    }

    // Update is called once per frame
    void Update()
    {
        Rotate(false);

        ///print(transform.eulerAngles+" "+rot);
    }

    void Rotate(bool instant)
    {
        if(Target == null || !active) return;

        Vector3 distance = Target.position - transform.position;
        float angle = Mathf.Atan2(distance.y,distance.x);

        Quaternion rot = instant ? 
        Quaternion.Euler(0,0,Mathf.Rad2Deg*angle) : 
        Quaternion.Slerp(transform.rotation, Quaternion.Euler(0,0,Mathf.Rad2Deg*angle), rotationSpeed * Time.deltaTime);

        transform.rotation = rot;
    }
    void Rotate(float targetRotation)
    {
        Quaternion rot = Quaternion.Euler(0,0,Mathf.Rad2Deg*targetRotation); 
        transform.rotation = rot;
    }
}
