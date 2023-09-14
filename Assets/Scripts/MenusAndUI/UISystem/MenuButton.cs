using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuButton : SelectableBase
{
    #region main
    Button button;
    TextMeshProUGUI textMesh;
    Image image;

    void Awake()
    {
        if(button != null) return;
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        textMesh = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if(!button.interactable)
        button.transform.GetComponent<Image>().color = inactiveColor;
        startInteractable = button.interactable;
        base.SetInteractable(startInteractable);
    }
    public override void ForceSelect(bool forceAssign, bool withSound)
    {
        playSelectSound = withSound;
        if(button.interactable)
        {
            button.Select();
        }
        if(forceAssign)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject,null);
        }
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        ForceSelect(false,true);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        OnClick();
    }
    public override void OnSelect(BaseEventData eventData)
    {
        SelectHighlight();
    }
    public override void OnDeselect(BaseEventData eventData)
    {
        ///print(transform.name+" deselected");
        
        if(!button.interactable) return;
        DeselectVisual(Color.white);
        OnExitEvent?.Invoke(); 
    }
    public override void SetInteractable(bool interactable)
    {
        if(button == null) Awake();
        button.interactable = interactable && startInteractable;
        base.SetInteractable(interactable);
    }
    public override void OnClick()
    {
        if(!button.interactable || !button.gameObject.activeInHierarchy) return;
        ///print("Click "+ transform.name);
        DataShare.PlaySound("Menu_Select",false,0.2f,1);
        OnClickEvent?.Invoke();
        if(buttonAnim!=null) StopCoroutine(buttonAnim);
        buttonAnim = StartCoroutine(IButtonAnim());
        Menu.ToggleButtons(false,this);
    }
    public override void DeselectVisual(Color additiveColor)
    {
        textMesh.color = Color.white;
        image.sprite = Menu.boilAnimationUI.ButtonOffSprite;
        hoverEvent?.Invoke(false);
    }

    public override void SelectHighlight()
    {
        if(Menu.self.firstAwake && playSelectSound) DataShare.PlaySound("Menu_Highlight",false,0.2f,1);
        playSelectSound = true;
        Menu.self.firstAwake = true;
        textMesh.color = Color.black;
        Menu.SetImage(image,this);
        OnEnterEvent?.Invoke();
        hoverEvent?.Invoke(true);
    }
    Coroutine buttonAnim;
    IEnumerator IButtonAnim()
    {
        float progress = 0;
        float speed = 20;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * 1.2f;
        // Pop out
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime * speed;
            transform.localScale = Vector3.Lerp(startScale,endScale,progress);
            yield return 0;
        }
        // Return
        progress = 0;
        startScale = transform.localScale;
        endScale = Vector3.one;
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime * speed;
            transform.localScale = Vector3.Lerp(startScale,endScale,progress);
            yield return 0;
        }
    }
    #endregion
}
