using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class SpawnVelocity : MonoBehaviour
{
    public Vector2 StartVelocity;
    [SerializeField] Vector2 RandomizedSpawnVelocity = new Vector2();
    // Start is called before the first frame update
    void OnEnable()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        rb.velocity = Vector2.zero;
        Vector2 usedVelocity = RandomizedSpawnVelocity == Vector2.zero ? StartVelocity :
        new Vector2(Random.Range(StartVelocity.x,RandomizedSpawnVelocity.x),
                    Random.Range(StartVelocity.y,RandomizedSpawnVelocity.y));
        rb.AddForce(Vector2.one*usedVelocity,ForceMode2D.Impulse);
    }
}
