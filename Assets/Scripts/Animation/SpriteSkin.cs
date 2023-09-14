using UnityEngine;

[ExecuteInEditMode]
public class SpriteSkin : MonoBehaviour
{
    public Sprite[] mainSprites;
    public Sprite[] overrideSprites;
    public SpriteRenderer spriteRenderer;

    // Save time finding unchanged sprites
    public int boilIndex = 1;
    OverrideSprite lastSprite;
    OverrideSprite boilSprite; // Reserved special index to optimize idle animations

    // Find index of currently displayed sprite
    int FindSpriteIndex()
    {
        Sprite curSprite = spriteRenderer.sprite;

        if(lastSprite.Sprite==curSprite) return lastSprite.Index;
        else if(curSprite==boilSprite.Sprite) return boilSprite.Index;

        for(int i = 0; i < mainSprites.Length; i++)
        {
            if(curSprite==mainSprites[i])
            {
                lastSprite.Index = i;
                lastSprite.Sprite = curSprite;

                return i;
            }
        }
        return 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(spriteRenderer==null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        if(mainSprites==null) mainSprites = new Sprite[0];
        if(overrideSprites==null) overrideSprites = new Sprite[0];

        if(Application.isPlaying)
        {
            lastSprite = new OverrideSprite();
            boilSprite = new OverrideSprite(boilIndex,mainSprites[boilIndex]);
            UpdateSprite();
        }
    }
    void UpdateSprite()
    {
        if(spriteRenderer.sprite!=null)
        {
            spriteRenderer.sprite = overrideSprites[FindSpriteIndex()];
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        #if UNITY_EDITOR
        if(!Application.isPlaying && mainSprites.Length==overrideSprites.Length&&mainSprites.Length!=0)
        {
            if(lastSprite==null) lastSprite = new OverrideSprite(0,mainSprites[0]);
            if(boilSprite==null) boilSprite = new OverrideSprite(boilIndex,mainSprites[boilIndex]);
        }
        if(mainSprites.Length==overrideSprites.Length&&mainSprites.Length!=0)
        #endif
        UpdateSprite();
    }
}

class OverrideSprite
{
    int index = 0;
    Sprite sprite;

    public int Index { get { return index;} set { index = value;}}
    public Sprite Sprite { get { return sprite;} set { sprite = value;}}

    public OverrideSprite(int index, Sprite sprite)
    {
        this.index = index;
        this.sprite = sprite;
    }
    public OverrideSprite(){}
}
