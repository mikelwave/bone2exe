using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;


public class Menu : MonoBehaviour
{
    public bool firstAwake = false;
    public static BoilAnimationUI boilAnimationUI;
    public static Menu self;
    protected GameObject menu;

    // Buttons
    protected SelectableBase[] buttons;
    protected SelectableBase selection;
    protected SelectableBase savedSelection;

    // Function to perform on successful confirmation

    public delegate void PostConfirmFunc();
    public PostConfirmFunc postConfirmFunc;

    public delegate void ConfirmationEvent(bool toggle,PostConfirmFunc func);
    public ConfirmationEvent confirmationEvent;
    
    public GameObject messageBox,messageBoxImage;
    public GameObject optionsMenu;

    public delegate void DirPressCallback(int dir);
    public DirPressCallback dirPressCallback;

    public delegate void SelectionCallback(Vector3 selectionPos);
    public SelectionCallback selectionCallback;

    public delegate void ApplyCallback();
    public ApplyCallback applyCallback;

    public void RestoreControl()
    {
        if(selection == null) return;
        selection.playSelectSound = false;
        EventSystem.current.SetSelectedGameObject(selection.gameObject,null);
    }

    public virtual void OpenMenu()
    {
        menu.SetActive(true);
        buttons[0].ForceSelect(true,true);

        InputSystemUIInputModule uIInputModule = EventSystem.current.transform.GetComponent<InputSystemUIInputModule>();
        uIInputModule.deselectOnBackgroundClick = false;
    }
    public virtual void CloseMenu()
    {
        menu.SetActive(false);
        ToggleButtons(false);
        ResetButtonVisuals();
    }
    public void SetFirst()
    {
        buttons[0].ForceSelect(true,true);
    }
    public static void SetSelection(SelectableBase selection, bool withSound)
    {
        self.selection = selection;
        selection.ForceSelect(true,withSound);
    }
    public static void SetSelectionNoForce(SelectableBase selection, bool withSound)
    {
        self.selectionCallback?.Invoke(selection.transform.position);
        self.selection = selection;
    }
    public static void ToggleButtons(bool toggle)
    {
        if(self == null) return;
        foreach(SelectableBase b in self.buttons)
        {
            ///print(b.name+" interactable: "+toggle);
            b.SetInteractable(toggle);
        }
    }
    public static void ToggleButtons(bool toggle, SelectableBase button)
    {
        if(self == null) return;
        // Foreign button check
        if(self.buttons[0].transform.parent != button.transform.parent) return;

        foreach(SelectableBase b in self.buttons)
        {
            ///print(b.name+" interactable: "+toggle);
            b.SetInteractable(toggle);
        }
    }
    public void LocalToggleButtons(bool toggle)
    {
        foreach(SelectableBase b in buttons)
        {
            ///print(b.name+" interactable: "+toggle);
            b.SetInteractable(toggle);
        }
    }
    public void ResetButtonVisuals()
    {
        foreach(SelectableBase b in buttons)
        {
            b.DeselectVisual(Color.white);
        }
    }
    
    protected virtual void Awake()
    {
        ///print(transform.name+" awake");

        //Canvas canvas = GetComponent<Canvas>();
        //if(canvas != null)
        //canvas.worldCamera = GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetComponent<Camera>();
        
        menu = transform.GetChild(0).gameObject;
        OverwriteBoilAnimationUI();
        // Assign buttons
        Transform menuTransform = menu.transform;
        buttons = transform.GetComponentsInChildren<SelectableBase>();
        selection = buttons[0];
    }
    public void OverwriteBoilAnimationUI()
    {
        boilAnimationUI = menu.GetComponent<BoilAnimationUI>();
        print(transform.name+" overwrites base menu BoilAnimationUI");
        if(boilAnimationUI == null)
        Debug.LogError("Error assigning boil animation UI");

    }
    public static void SetImage(Image image,SelectableBase button)
    {
        boilAnimationUI.imageRenderer = image;
        boilAnimationUI.main = true;
        boilAnimationUI.Animate();
        self.selection = button;
    }
    // Called by outside scripts to check if user is sure
    public void ConfirmCheck(string question, PostConfirmFunc func)
    {
        // Create options box
        GameObject obj = GameObject.FindWithTag("MessageBox");
        if(obj == null)
        obj = Instantiate(messageBox);
        obj.transform.SetParent(transform);
        Transform tr = obj.transform;
        tr.localPosition = Vector3.zero;

        DisplayMessage(question, tr, func);
    }
    // Display image variant
    public void ConfirmCheck(Sprite sprite, string question, PostConfirmFunc func)
    {
        // Create options box
        GameObject obj = GameObject.FindWithTag("MessageBox");
        if(obj == null)
        obj = Instantiate(messageBoxImage);
        obj.transform.SetParent(transform);
        Transform tr = obj.transform;
        tr.localPosition = Vector3.down*40;

        // Set image
        tr.GetChild(0).GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = sprite;

        DisplayMessage(question, tr, func);
    }
    void DisplayMessage(string question, Transform tr, PostConfirmFunc func)
    {
        PauseMenu.allowPause = false;

        //Prepare options box
        tr.rotation = Quaternion.identity;
        tr.localScale = Vector3.one;

        TMPro.TextMeshProUGUI textMesh = tr.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        textMesh.text = question;

        UIMessageBox linkedMenu = tr.GetComponent<UIMessageBox>();
        savedSelection = self.selection;
        linkedMenu.postConfirmFunc += func;
        linkedMenu.confirmationEvent = FinalizeConfirmCheck;

        // Disables master menu
        ToggleButtons(false);
        ///linkedMenu.OverwriteBoilAnimationUI();
        linkedMenu.ShowMenu();
        ///linkedMenu.LocalToggleButtons(true);
    }
    // Called in root script once button was pressed
    void FinalizeConfirmCheck(bool toggle, PostConfirmFunc func)
    {
        print("Finalizing confirm check");
        
        // If false, free up master menu buttons
        if(!toggle)
        {
            PauseMenu.allowPause = true;
            LocalToggleButtons(true);
        }
        // Perform queued action
        else
        {
            postConfirmFunc?.Invoke();
            func?.Invoke();
        }

        // Clear up
        postConfirmFunc -= func;
        OverwriteBoilAnimationUI();
        SetSelection(savedSelection,false);

        savedSelection = null;

    }
    protected virtual void UpdateControl()
    {
        if(MGInput.GetButtonDown(MGInput.controls.Player.Jump))
        {
            selection.OnClick();
        }
        if(!MGInput.GetButton(MGInput.controls.UI.MouseClick) && this == self && self.firstAwake && !EventSystem.current.alreadySelecting && selection != null && selection.interactable && EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(selection.gameObject);
        }
    }
    protected virtual void FixedUpdate()
    {
        UpdateControl();
    }
    protected virtual void Update()
    {
        if(Time.timeScale == 0) UpdateControl();
    }
}
