using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuToggle : SelectableBase
{
    #region main
    Toggle toggle;
    Image toggleBG;
    Image toggleCheckmark;
    Image label;

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

    public override void ApplySetting()
    {
        bool toSet = toggle.isOn;
        GameSettings.SetSettingBool(OptionName,toSet);
        ///print(gameObject.name+" toggle: "+toSet);
        base.ApplySetting();
    }

    // Animation
    bool boil = false;
    bool loaded = false;
    void AnimateToggle()
    {
        boil = !boil;
        toggleBG.sprite = GetToggleBGBoil()[boil ? 0 : 1];
    }
    void ToggleSelect(bool isOn,Color AdditiveColor,bool animate = true)
    {

        toggleCheckmark.sprite = OptionsGraphics.GetToggleCheck(isOn);
        toggleCheckmark.color = AdditiveColor;
        label.color = GetTextColor(isOn) * AdditiveColor;
        if(isOn && animate) AnimateToggle();
        else
        {
            toggleBG.sprite = GetToggleBGOffSprite();
            toggleBG.color = AdditiveColor;
        }
    }

    void Awake()
    {
        if(toggle != null) return;
        toggle = GetComponent<Toggle>();
        toggleBG = transform.GetChild(0).GetComponent<Image>();
        toggleCheckmark = toggleBG.transform.GetChild(0).GetComponent<Image>();
        label = transform.GetChild(1).GetComponent<Image>();

        startInteractable = toggle.interactable;
        base.SetInteractable(startInteractable);
        if(interactable && OptionName != "") LoadSettings();
        DeselectVisual(!toggle.interactable ? inactiveColor : Color.white);
    }
    public override void LoadSettings()
    {
        toggle.isOn = GameSettings.GetSettingBool(OptionName).Get();
        loaded = true;
    }
    public override void ForceSelect(bool forceAssign, bool withSound)
    {
        playSelectSound = withSound;
        if(toggle.interactable)
        {
            toggle.Select();
        }
        if(forceAssign)
        {
            EventSystem.current.SetSelectedGameObject(toggle.gameObject,null);
        }
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        ForceSelect(false,true);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
    }
    public override void OnSelect(BaseEventData eventData)
    {
        SelectHighlight();
    }
    public override void OnDeselect(BaseEventData eventData)
    {
        ///print(transform.name+" deselected");
        
        if(!toggle.interactable) return;
        DeselectVisual(Color.white);
        OnExitEvent?.Invoke(); 
    }
    public override void SetInteractable(bool interactable)
    {
        if(toggle == null) Awake();
        toggle.interactable = interactable && startInteractable;
        base.SetInteractable(interactable);
    }
    public void ClickToggleMouse(bool togg)
    {
        if(!loaded) return;
        toggle.isOn = togg;
        if(ClickVisual())
        {
            if(WaitForApply) QueueApply();
            else ApplySetting();
        }
    }
    bool ClickVisual()
    {
        if(!toggle.interactable || !toggle.gameObject.activeInHierarchy) return false;
        DataShare.PlaySound("Menu_Select",false,0.2f,1);
        OnClickEvent?.Invoke();

        if(toggleAnim!=null) StopCoroutine(toggleAnim);
        toggleAnim = StartCoroutine(IToggleAnim());
        return true;
    }
    public override void OnClick()
    {
        toggle.isOn = !toggle.isOn;
    }
    public override void DeselectVisual(Color AdditiveColor)
    {
        ToggleSelect(false,AdditiveColor);
        BoilAnimationUI.animateEvent -= AnimateToggle;
        hoverEvent?.Invoke(false);
    }

    public override void SelectHighlight()
    {
        if(Menu.self.firstAwake && playSelectSound) DataShare.PlaySound("Menu_Highlight",false,0.2f,1);

        Menu.SetSelectionNoForce(this,false);
        playSelectSound = true;
        Menu.self.firstAwake = true;
        ToggleSelect(true,Color.white);
        BoilAnimationUI.animateEvent = AnimateToggle;

        OnEnterEvent?.Invoke();
        hoverEvent?.Invoke(true);
    }
    Coroutine toggleAnim;
    IEnumerator IToggleAnim()
    {
        float progress = 0;
        float speed = 20;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * 1.1f;
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
