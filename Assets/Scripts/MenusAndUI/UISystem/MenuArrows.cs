using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ArrowButton : MonoBehaviour, IPointerDownHandler
{
    MenuArrows Base;
    public bool Right = true;

    public void Set(MenuArrows Base, bool Right)
    {
        this.Base = Base;
        this.Right = Right;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if(!Base.Interactable) return;
        Base.playSelectSound = false;
        EventSystem.current.SetSelectedGameObject(Base.gameObject);
        Base.ChangeOption(Right,transform);
    }
}

public class MenuArrows : SelectableBase
{
    Button button;
    bool boil = false;

    Image[] Arrows = new Image[2];
    TextMeshProUGUI buttonText;
    Image Label;
    Image ButtonBG;

    public string[] OptionsText;
    public int SelectionIndex = 0;

    public bool Interactable { get { return button.interactable;}}

    Color GetTextColor(bool isOn)
    {
        if(isOn) return OptionsGraphics.GetColorOn();
        else return OptionsGraphics.GetColorOff(0);
    }

    public override void ApplySetting()
    {
        GameSettings.SetSettingInt(OptionName,SelectionIndex);
        base.ApplySetting();
    }

    void AnimateArrows()
    {
        boil = !boil;
        int val = boil ? 0 : 1;

        // Main button
        ButtonBG.sprite = OptionsGraphics.GetSmallButtonBoil()[val];

        // Arrows
        Sprite arrSprite = OptionsGraphics.GetArrowBoil()[val];
        foreach (var Arrow in Arrows)
        {
            Arrow.sprite = arrSprite;
        }
    }

    public void ChangeOption(bool DirectionRight,Transform buttonTransform)
    {
        if(buttonAnim!=null) StopCoroutine(buttonAnim);
        StartCoroutine(IButtonAnim(DirectionRight , buttonTransform));
        DataShare.PlaySound("Menu_Select",false,0.2f,1);
        SetOptionText(DirectionRight ? 1 : -1);

        if(WaitForApply) QueueApply();
        else ApplySetting();
    }
    public void SetOptionText(int toAdd)
    {
        SelectionIndex = (int)Mathf.Repeat(SelectionIndex + toAdd,OptionsText.Length);
        buttonText.text = OptionsText[SelectionIndex];
    }
    public void ChangeOptionDir(int dir)
    {
        if(dir == 0) return;
        ChangeOption(dir == 1,Arrows[dir == -1 ? 0 : 1].transform);
    }

    Coroutine buttonAnim;
    IEnumerator IButtonAnim(bool DirectionRight, Transform buttonTransform)
    {
        float progress = 0;
        float speed = 20;
        Vector3 startScale = DirectionRight ? new Vector3(-1,1,1) : Vector3.one; 
        Vector3 endScale = startScale * 1.2f;
        //Pop out
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime * speed;
            buttonTransform.localScale = Vector3.Lerp(startScale,endScale,progress);
            yield return 0;
        }
        //Return
        progress = 0;
        endScale = startScale;
        startScale = buttonTransform.localScale;
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime * speed;
            buttonTransform.localScale = Vector3.Lerp(startScale,endScale,progress);
            yield return 0;
        }
    }

    void Awake()
    {
        button = GetComponent<Button>();
        ButtonBG = GetComponent<Image>();
        // Left arrow
        Arrows[0] = transform.GetChild(0).GetComponent<Image>();
        ArrowButton arrowButton = Arrows[0].gameObject.AddComponent<ArrowButton>();
        arrowButton.Set(this,false);
        // Right arrow
        Arrows[1] = transform.GetChild(1).GetComponent<Image>();
        arrowButton = Arrows[1].gameObject.AddComponent<ArrowButton>();
        arrowButton.Set(this,true);
        // Button text
        buttonText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        // Label
        Label = transform.GetChild(3).GetComponent<Image>();

        DeselectVisual(!button.interactable ? inactiveColor : Color.white);
        
        startInteractable = button.interactable;
        base.SetInteractable(startInteractable);
        if(interactable && OptionName != "") LoadSettings();

        SelectionIndex = Mathf.Clamp(SelectionIndex,0,OptionsText.Length-1);
        SetOptionText(0);
    }
    public override void LoadSettings()
    {
        SelectionIndex = GameSettings.GetSettingInt(OptionName).Get();
    }
    void ToggleVisual(bool isOn, Color AddtiveColor)
    {
        // Button text & label
        buttonText.color = GetTextColor(isOn) * AddtiveColor;
        Label.color = buttonText.color;

        if(isOn) return;

        // Main button
        ButtonBG.sprite = OptionsGraphics.GetSmallButtonOff(0);
        ButtonBG.color = AddtiveColor;

        // Arrows
        foreach(var Arrow in Arrows)
        {
            Arrow.sprite = OptionsGraphics.GetArrowOff();
            Arrow.color = AddtiveColor;
        }
    }
    public override void DeselectVisual(Color AdditiveColor)
    {
        ToggleVisual(false,AdditiveColor);
        Menu.self.dirPressCallback -= ChangeOptionDir;
        BoilAnimationUI.animateEvent -= AnimateArrows;
        hoverEvent?.Invoke(false);
    }

    public override void ForceSelect(bool forceAssign, bool withSound)
    {
        playSelectSound = withSound;
        if(button.interactable)
        {
            SelectHighlight();
        }
        if(forceAssign)
        {
            EventSystem.current.SetSelectedGameObject(gameObject,null);
        }
    }
    public override void OnClick()
    {
        if(!button.interactable) return;
        OnClickEvent?.Invoke();
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        ///print(transform.name+" deselected");
        
        if(!button.interactable) return;
        DeselectVisual(Color.white);
        OnExitEvent?.Invoke(); 
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if(!button.interactable) return;
        OnClick();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if(!button.interactable) return;
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if(!button.interactable)
        {
            return;
        }
        SelectHighlight();
    }

    public override void SelectHighlight()
    {
        if(Menu.self.firstAwake && playSelectSound) DataShare.PlaySound("Menu_Highlight",false,0.2f,1);

        Menu.SetSelectionNoForce(this,false);
        Menu.self.dirPressCallback = ChangeOptionDir;
        playSelectSound = true;
        Menu.self.firstAwake = true;

        ToggleVisual(true,Color.white);
        AnimateArrows();

        BoilAnimationUI.animateEvent = AnimateArrows;

        OnEnterEvent?.Invoke();
        hoverEvent?.Invoke(true);
    }

    public override void SetInteractable(bool interactable)
    {
        if(ButtonBG == null) Awake();
        button.interactable = interactable && startInteractable;
        base.SetInteractable(interactable);
    }
}
