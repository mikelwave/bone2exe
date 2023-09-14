using UnityEngine;

[RequireComponent (typeof (Rigidbody2D))]
public class VelocityCap : MonoBehaviour
{
    Rigidbody2D rb;
    public Vector2 VerticalCap = new Vector2(-10,10);
    public Vector2 HoritzontalCap = new Vector2(-10,10);
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 currentVelocity = rb.velocity;
        rb.velocity = new Vector2(
        Mathf.Clamp(currentVelocity.x,HoritzontalCap.x,HoritzontalCap.y),
        Mathf.Clamp(currentVelocity.y,VerticalCap.x,VerticalCap.y));
    }
}
