using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class LevelSelectButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
{
    #region main
    Button button;
    Vector3 startScale;
    public float highlightScale = 0.5f;
    public float pressedScale = 0.25f;

    public delegate void HighlightEvent(float from, float to, float progress);
    public HighlightEvent highlightEvent;
    public delegate void MouseHoverEvent(bool mouseOver);
    public MouseHoverEvent mouseHoverEvent;

    [SerializeField]
    string clickSound = "";

    [SerializeField] float offTransparency = 0.5f;
    protected virtual void Start()
    {
        if(clickSound != "") OnClickEvent.AddListener(PlaySound);
        Init();
    }
    protected void Init()
    {
        button = GetComponent<Button>();
        startScale = transform.localScale;
        if(!button.interactable)
        button.transform.GetComponent<Image>().color = Color.gray;
        //Set transparent
        highlightEvent?.Invoke(offTransparency,1,0);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(button.interactable)
        button.Select();

        if(buttonAnim!=null) StopCoroutine(buttonAnim);
        buttonAnim = StartCoroutine(IButtonAnim(startScale + Vector3.one*highlightScale,true));
        mouseHoverEvent?.Invoke(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if(!button.interactable) return;
        if(buttonAnim!=null) StopCoroutine(buttonAnim);
        buttonAnim = StartCoroutine(IButtonAnim(startScale,false));
        mouseHoverEvent?.Invoke(false);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        OnClick();
    }
    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }
    public void OnClick()
    {
        if(!button.interactable || !button.gameObject.activeInHierarchy) return;
        ///print("Click "+ transform.name);
        if(buttonAnim!=null) StopCoroutine(buttonAnim);
        buttonAnim = StartCoroutine(IButtonAnimPress(startScale - Vector3.one*pressedScale,false));
        OnClickEvent?.Invoke();
    }
    public void FakeOnClick()
    {
        if(!button.interactable || !button.gameObject.activeInHierarchy) return;
        ///print("Fake Click "+ transform.name);
        if(buttonAnim!=null) StopCoroutine(buttonAnim);
        buttonAnim = StartCoroutine(IButtonAnimPress(startScale - Vector3.one*pressedScale,true));
        mouseHoverEvent?.Invoke(true);
        if(clickSound != "") PlaySound();
    }
    void PlaySound()
    {
        DataShare.PlaySound(clickSound,false,0.1f,1);
    }
    Coroutine buttonAnim;
    IEnumerator IButtonAnim(Vector3 endScale,bool expand)
    {
        float progress = 0;
        float speed = 20;
        Vector3 startScale = transform.localScale;

        if(transform.localScale==endScale) yield break;
        // Pop out
        while(progress<1)
        {
            progress += Time.deltaTime * speed;
            transform.localScale = Vector3.Lerp(startScale,endScale,progress);
            if(expand) highlightEvent?.Invoke(offTransparency,1,progress);
            else highlightEvent?.Invoke(1,offTransparency,progress);
            yield return 0;
        }
    }
    IEnumerator IButtonAnimPress(Vector3 endScale,bool exitEvent)
    {
        float progress = 0;
        float speed = 20;
        // Adjust press animation based on called by mouse click or keyboard
        Vector3 startScale = !exitEvent ? transform.localScale : startScale = this.startScale + Vector3.one*highlightScale;
        // Pop out
        while(progress<1)
        {
            progress += Time.deltaTime * speed/2;
            transform.localScale = Vector3.Lerp(startScale,endScale,progress);
            if(exitEvent) highlightEvent?.Invoke(offTransparency,1,progress);
            yield return 0;
        }
        // Return
        progress = 0;
        startScale = transform.localScale;
        endScale = !exitEvent ? this.startScale + Vector3.one*highlightScale : this.startScale;
        while(progress<1)
        {
            progress += Time.deltaTime * speed/2;
            transform.localScale = Vector3.Lerp(startScale,endScale,progress);
            if(exitEvent)
            {
                highlightEvent?.Invoke(1,offTransparency,progress);
            }
            yield return 0;
        }
        if(exitEvent) mouseHoverEvent?.Invoke(false);
    }
    #endregion
    [SerializeField]
    UnityEvent clickEvent = new UnityEvent();
    public UnityEvent OnClickEvent { get { return clickEvent; } set { clickEvent = value; }}
}
