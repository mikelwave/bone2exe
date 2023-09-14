using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class SelectableBase : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler
{
    protected bool startInteractable;
    public bool interactable = false;
    public bool playSelectSound = true;
    protected bool canQueueApply = true;

    protected static Color inactiveColor = new Color(0.3f,0.3f,0.3f,1f);

    public virtual void SetInteractable(bool interactable)
    {
        this.interactable = interactable;
    }
    public abstract void ForceSelect(bool forceAssign, bool withSound);
    public abstract void DeselectVisual(Color AdditiveColor);
    public abstract void OnClick();

    public abstract void OnPointerEnter(PointerEventData eventData);
    public abstract void OnPointerDown(PointerEventData eventData);
    public abstract void OnSelect(BaseEventData eventData);
    public abstract void OnDeselect(BaseEventData eventData);
    public virtual void LoadSettings(){}
    public virtual void ApplySetting()
    {
        canQueueApply = true;
        OnApplyEvent?.Invoke();
        ///print(gameObject.name+" apply setting");
    }
    public delegate void HoverEvent(bool In);
    public HoverEvent hoverEvent;

    // Events
    [SerializeField]
    UnityEvent clickEvent = new UnityEvent();
    [SerializeField]
    UnityEvent enterEvent = new UnityEvent();
    [SerializeField]
    UnityEvent exitEvent = new UnityEvent();
    [SerializeField] UnityEvent applyEvent = new UnityEvent();
    public UnityEvent OnApplyEvent { get { return applyEvent; } set { applyEvent = value; }}
    public UnityEvent OnClickEvent { get { return clickEvent; } set { clickEvent = value; }}
    public UnityEvent OnEnterEvent { get {return enterEvent; } set { enterEvent = value; }}
    public UnityEvent OnExitEvent { get {return exitEvent; } set { exitEvent = value; }}

    [SerializeField] protected bool WaitForApply = false;
    [SerializeField] protected string OptionName = "";

    public virtual void SelectHighlight()
    {
        if(Menu.self.firstAwake && playSelectSound) DataShare.PlaySound("Menu_Highlight",false,0.2f,1);
        playSelectSound = true;
        Menu.self.firstAwake = true;
        OnEnterEvent?.Invoke();
        hoverEvent?.Invoke(true);
    }
    public virtual void QueueApply()
    {
        if(canQueueApply)
        {
            print(gameObject.name+" queued apply");
            canQueueApply = false;
            Menu.self.applyCallback += ApplySetting;
        }
    }
}
