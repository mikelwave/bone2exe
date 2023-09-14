using UnityEngine;

public class SpriteTrailFade : MonoBehaviour
{
    public Vector2 RandomXSpeed = Vector2.zero;
    public Vector2 RandomYSpeed = Vector2.zero;
    public bool RandomizeXDir = true;
    public bool RandomizeYDir = true;
    Vector2 RandomVelocity = Vector2.zero;
    public Color startColor;
    public float fadeSpeed = 2f;
    SpriteRenderer spriteRenderer;
    Transform Parent;
    Transform GMParent;
    delegate void UpdateEvent();
    UpdateEvent updateEvent;
    delegate void EnableEvent();
    EnableEvent enableEvent;

    // Called if movement value was assigned
    void SetRandomVelocity()
    {
        RandomVelocity = new Vector2(Random.Range(RandomXSpeed.x,RandomXSpeed.y),Random.Range(RandomYSpeed.x,RandomYSpeed.y));
        RandomVelocity.x *= RandomizeXDir ? ((int)Random.Range(0,2) == 0 ? 1 : -1) : 1;
        RandomVelocity.y *= RandomizeYDir ? ((int)Random.Range(0,2) == 0 ? 1 : -1) : 1;
    }
    void MoveTrailElement()
    {
        transform.position += (Vector3)RandomVelocity*Time.deltaTime;
    }

    public void Init(Transform Parent)
    {
        this.Parent = Parent;
        GMParent = GameMaster.self.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(RandomXSpeed != Vector2.zero || RandomYSpeed != Vector2.zero)
        {
            enableEvent += SetRandomVelocity;
            updateEvent += MoveTrailElement;
        }
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if(spriteRenderer == null)
        {
            gameObject.SetActive(false);
            return;
        }
        enableEvent?.Invoke();
        spriteRenderer.color = startColor;
        transform.SetParent(GMParent);
    }

    // Update is called once per frame
    void Update()
    {
        updateEvent?.Invoke();
        Color c = spriteRenderer.color;
        c.a-=Time.deltaTime*fadeSpeed;
        spriteRenderer.color = c;
        if(c.a<=0)
        {
            if(Parent!=null)
            {
                transform.SetParent(Parent);
                gameObject.SetActive(false);
            }
            else Destroy(gameObject);
        }
    }
}
