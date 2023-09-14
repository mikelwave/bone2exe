using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthbar : MonoBehaviour
{
    Image Icon, Fill;
    Sprite[] icons;
    Vector2 fixedPosition;
    RectTransform main;

    [SerializeField]
    AnimationCurve XMultiplierCurve;

    [SerializeField]
    AnimationCurve YMultiplierCurve;

    void BarAnim(float from, float to, float speed, float delay)
    {
        if(LerpBar != null) StopCoroutine(LerpBar);
        LerpBar = StartCoroutine(ILerpBar(from, to, speed, delay));
    }
    //From current fill position
    public void BarAnim(float to, float speed)
    {
        if(LerpBar != null) StopCoroutine(LerpBar);
        LerpBar = StartCoroutine(ILerpBar(Fill.fillAmount, to, speed, 0f, new Color(0.5f,0.5f,0.5f,1)));
    }
    public void Outro(float speed)
    {
        BarAnim(Fill.fillAmount,0,speed,0);
        StartCoroutine(IOutro());
    }

    Coroutine LerpBar;
    // Outro effect
    IEnumerator IOutro()
    {
        Icon.sprite = icons[1];
        yield return 0;
        float progress = 0;
        while(progress<1)
        {
            progress+=Time.deltaTime*1.5f;
            main.anchoredPosition = new Vector2(
                fixedPosition.x+XMultiplierCurve.Evaluate(progress),
                fixedPosition.y+YMultiplierCurve.Evaluate(progress));
            Vector3 rot = main.localEulerAngles;
            rot.z = Mathf.Repeat(rot.z+Time.deltaTime*1000,360);
            main.localEulerAngles = rot;
            yield return 0;
        }
    }
    // Intro effect
    IEnumerator IIntro()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        Vector2 startPos = fixedPosition+Vector2.down*10;
        Vector2 endPos = fixedPosition;
        canvasGroup.alpha = 0;
        Fill.fillAmount = 0;
        float progress = 0;
        
        // Appear
        while(progress<1)
        {
            progress+=Time.deltaTime*2;
            float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
            main.anchoredPosition = Vector2.Lerp(startPos,endPos,mathStep);
            canvasGroup.alpha = Mathf.Lerp(0,1,mathStep);
            yield return 0;
        }
        //Fill
        progress = 0;
        startPos = main.anchoredPosition;
        endPos = fixedPosition-Vector2.right*20;
        while(progress<1)
        {
            progress+=Time.deltaTime*4;
            float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
            Fill.fillAmount = Mathf.Lerp(0,1,mathStep);
            main.anchoredPosition = Vector2.Lerp(startPos,endPos,mathStep);
            yield return 0;
        }
        // Recoil
        progress = 0;
        startPos = main.anchoredPosition;
        endPos = fixedPosition+Vector2.right*5;
        while(progress<1)
        {
            progress+=Time.deltaTime*15;
            float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
            main.anchoredPosition = Vector2.Lerp(startPos,endPos,mathStep);
            yield return 0;
        }
        progress = 0;
        startPos = main.anchoredPosition;
        endPos = fixedPosition;
        while(progress<1)
        {
            progress+=Time.deltaTime*10;
            float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
            main.anchoredPosition = Vector2.Lerp(startPos,endPos,mathStep);
            yield return 0;
        }
    }
    
    // Loading variant
    IEnumerator ILerpBar(float from, float to, float speed, float delay)
    {
        Fill.fillAmount = from;
        if(delay > 0) yield return new WaitForSeconds(delay);
        float progress = 0;
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*speed;
            float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
            Fill.fillAmount = Mathf.Lerp(from,to,mathStep);
            yield return 0;
        }
        Fill.fillAmount = to;
    }

    // Color variant
    IEnumerator ILerpBar(float from, float to, float speed, float delay, Color startColor)
    {
        Fill.color = startColor;
        Fill.fillAmount = from;
        Vector2 startPos = fixedPosition+Vector2.right*5;
        if(delay > 0) yield return new WaitForSeconds(delay);
        float progress = 0;
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*speed;
            float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
            Fill.fillAmount = Mathf.Lerp(from,to,mathStep);
            Fill.color = Color.Lerp(startColor,Color.white,mathStep);
            main.anchoredPosition = Vector2.Lerp(startPos,fixedPosition,mathStep);
            yield return 0;
        }
        Fill.fillAmount = to;
    }
    public void Init(Sprite[] icons)
    {
        this.icons = icons;
        GetComponent<Canvas>().worldCamera = HUD.self.cam;
        main = transform.GetChild(0).GetComponent<RectTransform>();
        fixedPosition = main.anchoredPosition;

        Icon = main.GetChild(2).GetComponent<Image>();
        Fill = main.GetChild(1).GetComponent<Image>();

        Icon.sprite = this.icons[0];
        Icon.SetNativeSize();
        StartCoroutine(IIntro());
    }
}
