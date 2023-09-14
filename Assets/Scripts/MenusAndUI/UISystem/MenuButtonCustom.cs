using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuButtonCustom : SelectableBase
{
    #region main
    [Range(0,1)]
    [SerializeField] int buttonType = 0;
    [SerializeField] bool bigButton = false;
    [SerializeField] bool textless = false;
    Button button;
    TextMeshProUGUI textMesh;
    Image image;

    delegate Sprite GetButtonOffSprite();
    GetButtonOffSprite getButtonOffSprite;
    delegate Sprite[] GetButtonBoil();
    GetButtonBoil getButtonBoil;


    Sprite GetBigButtonOffSprite()
    {
        return OptionsGraphics.GetBigButtonOff(buttonType);
    }
    Sprite GetSmallButtonOffSprite()
    {
        return OptionsGraphics.GetSmallButtonOff(buttonType);
    }
    Color GetTextColor(bool isOn)
    {
        if(isOn) return OptionsGraphics.GetColorOn();
        else return OptionsGraphics.GetColorOff(buttonType);
    }

    // Animation
    bool boil = false;
    void AnimateButton()
    {
        boil = !boil;
        image.sprite = getButtonBoil?.Invoke()[boil ? 0 : 1];
    }

    void Awake()
    {
        if(button != null) return;
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        if(!textless) textMesh = transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        getButtonOffSprite = bigButton ? GetBigButtonOffSprite : GetSmallButtonOffSprite;
        getButtonBoil = bigButton ? OptionsGraphics.GetBigButtonBoil : OptionsGraphics.GetSmallButtonBoil;

        startInteractable = button.interactable;
        base.SetInteractable(startInteractable);
        DeselectVisual(!button.interactable ? inactiveColor : Color.white);
    }
    public override void LoadSettings()
    {
    }
    public override void ForceSelect(bool forceAssign, bool withSound)
    {
        playSelectSound = withSound;
        ///print("Force select "+withSound);
        if(button.interactable)
        {
            if(!textless) textMesh.color = GetTextColor(true);
            BoilAnimationUI.animateEvent = AnimateButton;
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
        if(EventSystem.current.currentSelectedGameObject != gameObject) return;
        ///print("Click "+ transform.name);
        DataShare.PlaySound("Menu_Select",false,0.2f,1);
        OnClickEvent?.Invoke();
        if(buttonAnim!=null) StopCoroutine(buttonAnim);
        buttonAnim = StartCoroutine(IButtonAnim());
    }
    public override void DeselectVisual(Color AdditiveColor)
    {
        if(!textless) textMesh.color = GetTextColor(false) * AdditiveColor;
        image.sprite = getButtonOffSprite?.Invoke();
        image.color = AdditiveColor;
        BoilAnimationUI.animateEvent -= AnimateButton;
        hoverEvent?.Invoke(false);
    }

    public override void SelectHighlight()
    {
        if(Menu.self.firstAwake && playSelectSound) DataShare.PlaySound("Menu_Highlight",false,0.2f,1);

        Menu.SetSelectionNoForce(this,false);
        playSelectSound = true;
        Menu.self.firstAwake = true;
        if(!textless) textMesh.color = GetTextColor(true);
        BoilAnimationUI.animateEvent = AnimateButton;

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
