// Script for generating and creating trail sprites (used and controlled by external scripts)
using UnityEngine;

public class SpriteTrailMain : MonoBehaviour
{
    public int spritesAmount = 5;
    public float spawnRate = 0.2f;
    SpriteRenderer[] trailSprites;
    SpriteRenderer source;
    Transform sourceTransform;
    void GenerateTrailSprites()
    {
        trailSprites = new SpriteRenderer[spritesAmount];
        GameObject sample = transform.GetChild(0).gameObject;
        trailSprites[0] = sample.GetComponent<SpriteRenderer>();
        sample.GetComponent<SpriteTrailFade>().Init(transform);
        for(int i = 1;i<trailSprites.Length;i++)
        {
            GameObject obj = Instantiate(sample,sample.transform.position,Quaternion.identity);
            obj.transform.SetParent(transform);
            obj.GetComponent<SpriteTrailFade>().Init(transform);
            trailSprites[i] = obj.GetComponent<SpriteRenderer>();
        }
    }
    void SpawnTrailSprite()
    {
        if(trailSprites == null || trailSprites.Length == 0)
        {
            DeActivate();
            return;
        }
        foreach(SpriteRenderer s in trailSprites)
        {
            if(!s.gameObject.activeInHierarchy)
            {
                Transform t = s.transform;
                s.sprite = source.sprite;
                t.position = sourceTransform.position;
                t.rotation = sourceTransform.rotation;
                t.localScale = sourceTransform.localScale;
                t.gameObject.SetActive(true);
                return;
            }
        }
    }
    void Start()
    {
        GenerateTrailSprites();
    }
    public void SetSource(SpriteRenderer spriteRenderer)
    {
        source = spriteRenderer;
        sourceTransform = source.transform;
    }
    public void Activate()
    {
        if(source==null) return;
        InvokeRepeating("SpawnTrailSprite",0f,spawnRate);
    }
    public void DeActivate()
    {
        CancelInvoke("SpawnTrailSprite");
    }
}
