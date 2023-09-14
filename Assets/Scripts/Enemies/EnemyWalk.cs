using UnityEngine;

public class EnemyWalk : MonoBehaviour
{
    public float movementSpeed = 10;
    int dir = 1;
    Rigidbody2D rb;
    public float pauseWalkCooldown = 0;
    public delegate void PauseCooldownEndEvent();
    public PauseCooldownEndEvent pauseCooldownEndEvent;

    float roundedX = -99;
    int height = 0;

    void Movement()
    {
        rb.AddForce(Vector2.right*-movementSpeed*Time.fixedDeltaTime*dir,ForceMode2D.Impulse);
        float curX = dir == 1 ? Mathf.Floor(transform.position.x+0.5f) : Mathf.Ceil(transform.position.x-1.5f);
        if(curX!=roundedX)
        {
            roundedX = curX;
            GroundDetect();
        }
    }
    void GroundDetect()
    {
        // Check if gap or invalid tile
        Vector3Int tilePos = new Vector3Int((int)roundedX-dir,height,0);
        Vector3 pos = transform.position;
        // Gaps
        #if UNITY_EDITOR
        Debug.DrawLine(pos,tilePos,Color.cyan);
        #endif
        if(GameMaster.GetTileResult(pos,tilePos,"MainMap") != -1 && GameMaster.GetTileResult(pos,tilePos,"SSMap") != -1)
        {
            ChangeDir();
            return;
        }
        // Walls
        tilePos.y+=1;
        ///Debug.DrawLine(transform.position,tilePos,Color.cyan);
        if(GameMaster.GetTileResult(pos,tilePos,"MainMap") != 0)
        {
            ChangeDir();
        }
    }
    void ChangeDir()
    {
        dir *= -1;

        Flip(dir);
    }
    void Flip(int dir)
    {

        if(dir == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = dir;
        transform.localScale = scale;
    }
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dir = (int)Mathf.RoundToInt(transform.localScale.x);
        height = Mathf.FloorToInt(transform.position.y)-1;
    }
    void OnDisable()
    {
        if(rb==null) return;
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
    }
    public void Respawn()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = Vector2.zero;
        if(pauseWalkCooldown>0)
        {
            pauseWalkCooldown-=Time.fixedDeltaTime;
            if(pauseWalkCooldown<=0) pauseCooldownEndEvent?.Invoke();
        }
        else Movement();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name.ToLower().Contains("camtransition"))
        {
            ChangeDir();
        }
    }
}
