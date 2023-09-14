using UnityEngine;
using UnityEngine.Events;

public class BoilAnimation : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    int SpriteIndex = 0;
    public Sprite[] sprites;
    public float animationSpeed = 0.25f;
    [SerializeField] bool looping = true;
    public void ResetAnim(Sprite[] newSprites)
    {
        sprites = newSprites;
        SpriteIndex = 0;
        CancelInvoke("Animate");
        
        if(spriteRenderer!=null)
        InvokeRepeating("Animate",0f,animationSpeed);
    }
    public void ResetAnim()
    {
        SpriteIndex = 0;
        CancelInvoke("Animate");

        if(spriteRenderer!=null)
        InvokeRepeating("Animate",0f,animationSpeed);
    }
    // Start is called before the first frame update
    void Start()
    {
        if(spriteRenderer == null)
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void OnEnable()
    {
        if(spriteRenderer==null) Start();

        if(spriteRenderer!=null)
        InvokeRepeating("Animate",0f,animationSpeed);
    }
    void OnDisable()
    {
        CancelInvoke("Animate");
    }
    void OnDestroy()
    {
        CancelInvoke("Animate");
    }

    void Animate()
    {
        if(spriteRenderer == null)
        {
            CancelInvoke("Animate");
            return;
        }
        spriteRenderer.sprite = sprites[SpriteIndex];
        SpriteIndex = (int)Mathf.Repeat(SpriteIndex+=1,sprites.Length);
        if(SpriteIndex == 0 && !looping)
        {
            OnEndEvent?.Invoke();
        }
    }

    [SerializeField]
    UnityEvent EndEvent = new UnityEvent();
    public UnityEvent OnEndEvent { get {return EndEvent;} set {EndEvent = value;}}

}
