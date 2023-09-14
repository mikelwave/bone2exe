using UnityEngine;

[ExecuteInEditMode]
public class MaceBehaviour : MonoBehaviour
{
    [Range (0,1)]
    public float angle = 0;
    [Range (-2,2)]
    public float speed = 4;
    [Range (1,7)]
    public float radius = 5;
    float chainLinkDistance = 1;
    Vector2 position;
    Transform[] objects;
    SpriteRenderer[] spriteRenderers;
    CircleCollider2D circleCollider;
    
    // Boil graphics
    public float boilSpeed = 0.1f;
    int boilInt = 0;
    public Sprite[] headSprites;
    public Sprite[] chainSprites;
    float startAngle = -1;

    #if UNITY_EDITOR
    float lastRadius;
    #endif
    void UpdateSpriteRenderers()
    {
        boilInt = (int)Mathf.Repeat(boilInt+1,2);
        spriteRenderers[0].sprite = headSprites[boilInt];
        for(int i = 1; i<spriteRenderers.Length;i++)
        {
            spriteRenderers[i].sprite = chainSprites[(int)Mathf.Repeat(boilInt+i%2,2)];
        }
    }
    void OnEnable()
    {
        #if UNITY_EDITOR
        if(Application.isPlaying)
        #endif
        {
            if(startAngle!=-1) angle = startAngle;
            if(spriteRenderers == null || spriteRenderers.Length == 0) AssignSpriteRenderers();
            InvokeRepeating("UpdateSpriteRenderers",0f,boilSpeed);
        }
    }
    void OnDisable()
    {
        #if UNITY_EDITOR
        if(Application.isPlaying)
        #endif
        CancelInvoke("UpdateSpriteRenderers");
    }
    void AssignSpriteRenderers()
    {
        spriteRenderers = new SpriteRenderer[transform.childCount];
        for(int i = 0; i<spriteRenderers.Length;i++)
        {
            spriteRenderers[i] = transform.GetChild(i).GetComponent<SpriteRenderer>();
        }
    }
    #if UNITY_EDITOR
    void OnValidate() { UnityEditor.EditorApplication.delayCall = _OnValidate; }
    void _OnValidate()
    {
        if (this == null || Application.isPlaying) return;
        if(circleCollider==null) Start();
        circleCollider.radius = radius-0.5f;
        GenerateChain();
        chainLinkDistance = radius/(transform.childCount-1);
    }
    #endif
    void GenerateChain()
    {
        // Check how many object exist and remove/add as much as needed
        int ChainCount = transform.childCount-1;
        int NeededChainAmount = Mathf.RoundToInt(radius);
        position = (Vector2)transform.position;

        // Cleanup
        // Add
        if(ChainCount < NeededChainAmount)
        {
            int Difference = NeededChainAmount - ChainCount;
            // Take the sample chain object from child 1
            GameObject sampleChain = transform.GetChild(1).gameObject;
            for(int i = 0; i<Difference;i++)
            {
                GameObject obj = Instantiate(sampleChain,position,Quaternion.identity);
                obj.transform.SetParent(transform);
                obj.name = "Chain";
            }
        }
        // Remove
        else if(ChainCount > NeededChainAmount)
        {
            int Difference = transform.childCount - (ChainCount - NeededChainAmount);
            for(int i = transform.childCount-1; i >= Difference; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        else if(objects != null && objects.Length!=0) return;
        objects = new Transform[transform.childCount];
        for(int i = 0; i<objects.Length; i++)
        {
            objects[i] = transform.GetChild(i);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        #if UNITY_EDITOR
        lastRadius = radius;
        #endif
        startAngle = angle;
        circleCollider = GetComponent<CircleCollider2D>();

        GenerateChain();
    }

    // Update is called once per frame
    void Update()
    {
        #if UNITY_EDITOR
        if(!Application.isPlaying)
        {
            position = (Vector2)transform.position;

            if(objects == null || objects.Length == 0) Start();
        }
        if(Application.isPlaying)
        #endif
        angle = Mathf.Repeat(angle+Time.deltaTime*speed,1);
        
        float PIAngle = angle * Mathf.PI * 2;
        for(int i = 0; i< objects.Length; i++)
        {
            float localRadius = radius;
            if(i == 1) continue;
            if(i > 1) localRadius = chainLinkDistance*(i-1);

            Vector2 pos;
            pos.x = position.x + Mathf.Cos(PIAngle) * localRadius;
            pos.y = position.y + Mathf.Sin(PIAngle) * localRadius;

            objects[i].position = pos;
        }
    }
}
