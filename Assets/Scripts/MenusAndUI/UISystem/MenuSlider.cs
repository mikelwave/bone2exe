using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuSlider : SelectableBase
{
    #region main
    [SerializeField] string ToggleOptionName = "";
    public bool isOn = true;
    Slider slider;
    Image sliderBG;
    Image sliderKnob;
    Image toggleBG;
    Image sliderCheckmark;
    Image label;
    MenuToggleSlider menuToggleSlider;


    public bool Interactable {get{return slider.interactable;}}
    bool MouseClick = false;

    // Animation
    #region animation
    bool boil = false;

    Sprite GetSliderBGOffSprite()
    {
        return OptionsGraphics.GetSliderBGOff();
    }
    Sprite GetSliderBGSprite()
    {
        return OptionsGraphics.GetSliderBG();
    }
    Sprite[] GetSliderKnobBoil()
    {
        return OptionsGraphics.GetSliderKnobBoil();
    }
    Sprite GetSliderKnobOff()
    {
        return OptionsGraphics.GetSliderKnobOff();
    }
    Sprite GetToggleBGOffSprite()
    {
        return OptionsGraphics.GetToggleBGOff();
    }
    Sprite[] GetToggleBGBoil()
    {
        return OptionsGraphics.GetToggleBGBoil();
    }
    Color GetTextColor(bool isOn)
    {
        if(isOn) return OptionsGraphics.GetColorOn();
        else return OptionsGraphics.GetColorOff(0);
    }
    #endregion

    IEnumerator IWaitUntilMouseRelease()
    {
        // To prevent spamming the settings changes, wait until mouse has been released if it was pressed.
        yield return 0;
        yield return new WaitUntil(()=>!MGInput.GetButton(MGInput.controls.UI.MouseClick));
        MouseClick = false;
        GameSettings.SetSettingFloat(OptionName,slider.value);
    }

    public void OnSliderEdit()
    {
        ApplySetting();
        if(MouseClick) return;
        MouseClick = true;
        StartCoroutine(IWaitUntilMouseRelease());
    }
    void AnimateSlider()
    {
        boil = !boil;
        int val = boil ? 0 : 1;
        sliderKnob.sprite = GetSliderKnobBoil()[val];
        toggleBG.sprite = GetToggleBGBoil()[val];
    }
    public void Toggle(bool isOn,bool animate = true)
    {
        this.isOn = isOn;
        sliderCheckmark.enabled = isOn;
        if(animate)
        {
            if(WaitForApply) QueueApply();
            else
            {
                GameSettings.SetSettingBool(ToggleOptionName,isOn);
                ApplySetting();
                OnApplyEvent?.Invoke();
            }
            AnimateClick();
        }
    }
    public override void ApplySetting()
    {
        canQueueApply = true;
    }
    void SliderSelect(bool isOn,Color AdditiveColor)
    {
        sliderCheckmark.sprite = OptionsGraphics.GetToggleCheck(isOn);
        sliderCheckmark.color = AdditiveColor;
        label.color = GetTextColor(isOn) * AdditiveColor;
        if(isOn) AnimateSlider();
        else
        {
            sliderKnob.sprite = GetSliderKnobOff();
            sliderKnob.color = AdditiveColor;
            toggleBG.sprite = GetToggleBGOffSprite();
            toggleBG.color = AdditiveColor;
        }
        sliderBG.sprite = isOn ? GetSliderBGSprite() : GetSliderBGOffSprite();
        sliderBG.color = AdditiveColor;
    }

    void Awake()
    {
        if(slider != null) return;

        slider = GetComponent<Slider>();
        sliderBG = transform.GetChild(0).GetComponent<Image>();
        sliderKnob = transform.GetChild(1).GetChild(0).GetComponent<Image>();

        toggleBG = transform.parent.GetChild(1).GetComponent<Image>();
        menuToggleSlider = toggleBG.GetComponent<MenuToggleSlider>();

        sliderCheckmark = toggleBG.transform.GetChild(0).GetComponent<Image>();
        label = toggleBG.transform.GetChild(1).GetComponent<Image>();


        startInteractable = slider.interactable;
        base.SetInteractable(startInteractable);
        DeselectVisual(!slider.interactable ? inactiveColor : Color.white);
        if(interactable && OptionName != "" && ToggleOptionName != "") LoadSettings();

        slider.onValueChanged.AddListener(delegate {OnSliderEdit(); });
    }
    public override void LoadSettings()
    {
        isOn = GameSettings.GetSettingBool(ToggleOptionName).Get();
        slider.value = GameSettings.GetSettingFloat(OptionName).Get();
        Toggle(isOn,false);
    }
    public override void ForceSelect(bool forceAssign, bool withSound)
    {
        playSelectSound = withSound;
        if(slider.interactable)
        {
            slider.Select();
        }
        if(forceAssign)
        {
            EventSystem.current.SetSelectedGameObject(slider.gameObject,null);
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
        
        if(!slider.interactable) return;
        DeselectVisual(Color.white);
        OnExitEvent?.Invoke(); 
    }
    public override void SetInteractable(bool interactable)
    {
        if(slider == null) Awake();
        slider.interactable = interactable && startInteractable;
        base.SetInteractable(interactable);
    }
    public void AnimateClick()
    {
        if(sliderAnim!=null) StopCoroutine(sliderAnim);
        sliderAnim = StartCoroutine(ISliderAnim());
    }
    public override void OnClick()
    {
        if(!slider.interactable || !slider.gameObject.activeInHierarchy) return;
        ///print("Click "+ transform.name);
        DataShare.PlaySound("Menu_Select",false,0.2f,1);
        OnClickEvent?.Invoke();

        if(MGInput.GetButton(MGInput.controls.Player.Jump))
        {
            Toggle(!isOn);
        }
    }
    public override void DeselectVisual(Color AdditiveColor)
    {
        SliderSelect(false,AdditiveColor);
        BoilAnimationUI.animateEvent -= AnimateSlider;
        hoverEvent?.Invoke(false);
    }

    public override void SelectHighlight()
    {
        if(Menu.self.firstAwake && playSelectSound) DataShare.PlaySound("Menu_Highlight",false,0.2f,1);

        Menu.SetSelectionNoForce(this,false);
        playSelectSound = true;
        Menu.self.firstAwake = true;
        SliderSelect(true,Color.white);
        BoilAnimationUI.animateEvent = AnimateSlider;

        OnEnterEvent?.Invoke();
        hoverEvent?.Invoke(true);
    }
    Coroutine sliderAnim;
    IEnumerator ISliderAnim()
    {
        float progress = 0;
        float speed = 20;
        Transform toMove = transform.parent;
        Vector3 startScale = toMove.localScale;
        Vector3 endScale = startScale * 1.1f;
        // Pop out
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime * speed;
            toMove.localScale = Vector3.Lerp(startScale,endScale,progress);
            yield return 0;
        }
        // Return
        progress = 0;
        startScale = toMove.localScale;
        endScale = Vector3.one;
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime * speed;
            toMove.localScale = Vector3.Lerp(startScale,endScale,progress);
            yield return 0;
        }
    }
    #endregion
}
