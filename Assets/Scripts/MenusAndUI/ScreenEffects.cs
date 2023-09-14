using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffects : MonoBehaviour
{
    static Image overlay;
    static ScreenEffects screenEffects;
    // Start is called before the first frame update
    void Init()
    {
        if(screenEffects!=null) return;

        overlay = transform.GetChild(transform.childCount-1).GetComponent<Image>();
        screenEffects = this;
    }
    public static Coroutine fadeCor;
    static IEnumerator IFadeCor(float fadeSpeed,bool fadeIn,Color fadeColor)
    {
        Color targetColor = fadeIn ? fadeColor : new Color(fadeColor.r,fadeColor.g,fadeColor.b,0);
        Color startColor = targetColor;
        startColor.a = fadeIn ? 0 : 1;
        float progress = 0;
        while(progress<1)
        {
            overlay.color = Color.Lerp(startColor,targetColor,progress);
            progress+=Time.deltaTime*fadeSpeed;
            yield return 0;
        }
        overlay.color = targetColor;
        fadeCor = null;
    }
    public static bool TransitionFinished {get {return fadeCor == null;}}
    public static void FadeScreen(float fadeSpeed,bool fadeIn, Color fadeColor)
    {
        if(screenEffects==null) GameObject.FindWithTag("HUD").GetComponent<ScreenEffects>().Init();
        if(fadeCor != null)screenEffects.StopCoroutine(fadeCor);
        fadeCor = screenEffects.StartCoroutine(IFadeCor(fadeSpeed,fadeIn,fadeColor));
    }
}
