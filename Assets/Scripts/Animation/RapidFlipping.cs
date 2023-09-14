using UnityEngine;

// The billy things use it
public class RapidFlipping : MonoBehaviour
{
    Vector3 flipScale;
    [SerializeField] float flipRate = 0.1f;

    void OnEnable()
    {
        flipScale = transform.localScale;
        InvokeRepeating("Flip",0,flipRate);
    }
    void OnDisable()
    {
        CancelInvoke("Flip");
    }
    void Flip()
    {
        flipScale.x *= -1;
        transform.localScale = flipScale;
    }
}
