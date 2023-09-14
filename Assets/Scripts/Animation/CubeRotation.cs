using UnityEngine;

public class CubeRotation : MonoBehaviour
{
    [SerializeField] Vector3 rotation;
    float loopClamp = Mathf.PI*2;
    float timeDelta = 0;

    void FixedUpdate()
    {
        timeDelta = Mathf.Repeat(timeDelta+Time.fixedDeltaTime,loopClamp);
        transform.Rotate(
            new Vector3
            (
                Mathf.Sin(timeDelta+1f)* rotation.x,
                Mathf.Cos(timeDelta+0.5f)* rotation.y,
                Mathf.Sin(timeDelta)* rotation.z
            ),Space.World);
    }
}
