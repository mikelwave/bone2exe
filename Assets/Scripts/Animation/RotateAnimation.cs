using UnityEngine;

public class RotateAnimation : MonoBehaviour
{
    public float speed = 1;

    // Update is called once per frame
    void Update()
    {
        Vector3 rot = transform.localEulerAngles;
        rot.z = Mathf.Repeat(rot.z+Time.deltaTime*speed,360);
        transform.localEulerAngles = rot;
    }
}
