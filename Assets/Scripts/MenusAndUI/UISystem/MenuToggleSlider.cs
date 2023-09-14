using UnityEngine;
using UnityEngine.EventSystems;

public class MenuToggleSlider : SelectableBase
{
    MenuSlider Base;
    void Awake()
    {
        Base = transform.parent.GetChild(0).GetComponent<MenuSlider>();
    }
    public override void DeselectVisual(Color additiveColor)
    {
        Base.DeselectVisual(additiveColor);
    }

    public override void ForceSelect(bool forceAssign, bool withSound)
    {
        if(!Base.Interactable) return;
        Base.ForceSelect(forceAssign,withSound);
    }

    public override void OnClick()
    {
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        Base.OnDeselect(eventData);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if(!Base.Interactable) return;
        Base.Toggle(!Base.isOn);
        Base.playSelectSound = false;
        EventSystem.current.SetSelectedGameObject(Base.gameObject);
        Base.OnClick();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if(!Base.Interactable) return;
        Base.OnPointerEnter(eventData);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if(!Base.Interactable) return;
        Base.OnSelect(eventData);
    }

    public override void SelectHighlight()
    {
        if(!Base.Interactable) return;
        Base.SelectHighlight();
    }

    public override void SetInteractable(bool interactable)
    {
    }

    public override void LoadSettings()
    {
        Base.LoadSettings();
    }
}
