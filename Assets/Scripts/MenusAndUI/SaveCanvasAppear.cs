using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveCanvasAppear : MonoBehaviour
{
    TextMeshProUGUI textMeshProUGUI;
    Image image;
    [SerializeField] Sprite savedSprite;
    CanvasGroup canvasGroup;
    public static string Insert = string.Empty;
    public static bool SaveText = true;
    IEnumerator ISaveFade()
    {
        textMeshProUGUI.text = "Saving "+Insert+"...";
        yield return 0;
        yield return new WaitUntil(()=>SaveLoadData.saveComplete);
        if(!SaveLoadData.saveFailed)
        {
            image.sprite = savedSprite;
            textMeshProUGUI.text = "Saved "+Insert+".";
        }
        else textMeshProUGUI.text = "Saving "+Insert+" failed.";
        float progress = 0;
        Insert = "";

        while(progress<1f)
        {
            progress += Time.unscaledDeltaTime;
            yield return 0;
        }
        // Fade out
        progress = 0;
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime * 10;
            canvasGroup.alpha =  Mathf.Lerp(1,0,progress);
            yield return 0;
        }
        Destroy(gameObject);
    }
    IEnumerator ITextFade()
    {
        SaveText = true;
        textMeshProUGUI.text = Insert;
        Insert = "";
        image.gameObject.SetActive(false);
        textMeshProUGUI.GetComponent<RectTransform>().anchoredPosition = image.GetComponent<RectTransform>().anchoredPosition;
        float progress = 0;
        while(progress<1.5f)
        {
            progress += Time.unscaledDeltaTime;
            yield return 0;
        }
        // Fade out
        progress = 0;
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime * 10;
            canvasGroup.alpha =  Mathf.Lerp(1,0,progress);
            yield return 0;
        }
        Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        textMeshProUGUI = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        image = transform.GetChild(1).GetComponent<Image>();
        StartCoroutine(SaveText ? ISaveFade() : ITextFade());
    }
}
