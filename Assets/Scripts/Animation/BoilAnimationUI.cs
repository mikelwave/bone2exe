using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BoilAnimationUI : MonoBehaviour
{
    // Sprite of a non-selected button
    [SerializeField] bool EventOnly = false;
    [SerializeField] bool eraseEventsOnDisable = true;
    [SerializeField] private Sprite buttonOffSprite;
    public Sprite ButtonOffSprite { get {return buttonOffSprite;}}
    public Image imageRenderer;
    int SpriteIndex = 0;
    public Sprite[] sprites;
    public float animationSpeed = 0.25f;
    public delegate void AnimateEvent();
    public static AnimateEvent animateEvent;
    public bool main = false;
    Coroutine UnscaledAnimate;
    IEnumerator IUnscaledAnimate()
    {
        float tick = animationSpeed;
        while(true)
        {
            tick -=Time.unscaledDeltaTime;
            if(tick<=0)
            {
                Animate();
                tick = Mathf.Repeat(tick,animationSpeed);
            }
            yield return 0;
        }
    }

    void OnEnable()
    {
        if(!EventOnly)
        {
            if(imageRenderer == null)
            imageRenderer = GetComponent<Image>();
        }

        if(Time.timeScale != 0)
            InvokeRepeating("Animate",0f,animationSpeed);

        else
        {
            if(UnscaledAnimate!=null)StopCoroutine(UnscaledAnimate);
            UnscaledAnimate = StartCoroutine(IUnscaledAnimate());
        }
    }
    void Cancel()
    {
        CancelInvoke("Animate");
        if(UnscaledAnimate!=null)StopCoroutine(UnscaledAnimate);
    }
    void OnDisable()
    {
        if(eraseEventsOnDisable)
        animateEvent = null;
        Cancel();
    }
    void OnDestroy()
    {
        Cancel();
    }

    public void Animate()
    {
        if(!EventOnly)
        {
            if(imageRenderer == null) return;
            imageRenderer.sprite = sprites[SpriteIndex];
            SpriteIndex = (int)Mathf.Repeat(SpriteIndex+=1,sprites.Length);
        }
        if(main)
        {
            animateEvent?.Invoke();
        }
    }
}
