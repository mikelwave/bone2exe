using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PauseMenu : Menu
{
    static Animator anim;
    public static bool paused = false;
    public static bool allowPause = true;
    static TextMeshProUGUI levelName;

    [SerializeField] UnityEvent ActivationEvent = new UnityEvent();
    [SerializeField] UnityEvent DeActivationEvent = new UnityEvent();
    // Start is called before the first frame update
    protected override void Awake()
    {
        transform.SetParent(null);
        if(DataShare.FindCopy("PauseMenu",this.gameObject))
        {
            // Check if paused
            if(paused)
            {
                Pause(false);
            }
            return;
        }
        self = this;

        print("Assigned new pause menu");

        menu = transform.GetChild(1).gameObject;
        OverwriteBoilAnimationUI();
        
        anim = GetComponent<Animator>();
        if(PauseWait!=null) StopCoroutine(PauseWait);
        PauseWait = self.StartCoroutine(IPauseWait("PauseM_Idle",false));
        paused = false;

        levelName = self.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        
        // Assign buttons
        Transform menuTransform = menu.transform;
        buttons = menuTransform.GetComponentsInChildren<SelectableBase>();
        ///print("Buttons count: "+buttons.Length);
        selection = buttons[0];
        self.enabled = false;
        postConfirmFunc += UnPauseAnim;
        
        //GetComponent<Canvas>().worldCamera = GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetComponent<Camera>();
    }
    static Coroutine PauseWait;
    static IEnumerator IPauseWait(string AnimName,bool toggle)
    {
        PauseMenu p = (PauseMenu)self;
        if(toggle && allowPause)
        {
            p.ActivationEvent?.Invoke();
            Time.timeScale = 0;
            self.OpenMenu();
            self.SetFirst();
        }

        int delay = 5;
        while(delay>0)
        {
            yield return 0;
            delay--;
        }
        yield return new WaitUntil(()=>anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f);

        if(!toggle && allowPause)
        {
            Time.timeScale = DataShare.GameSpeed;
        }

        yield return new WaitUntil(()=>anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        ///print("Pause animation finished");
        if(!toggle)
        {
            p.DeActivationEvent?.Invoke();
            self.CloseMenu();
            anim.enabled = false;
            self.enabled = false;
        }
        else
        {
            ToggleButtons(true);
        }
        PauseWait = null;
    }
    public void UnPauseAnim()
    {
        string animName = "PauseM_Out";
        anim.Play(animName);
        PauseWait = self.StartCoroutine(IPauseWait(animName,false));
        paused = false;
    }
    public static void SetLevelName()
    {
        if(PauseMenu.self == null) return;
        levelName.text = GameMaster.GetLevelTitle();
    }
    public static void Pause(bool toggle)
    {
        ///print("Pause call: "+paused);
        if(!allowPause) return;
        if(toggle)
        {
            if(Time.timeScale != DataShare.GameSpeed) return;
            self.enabled = true;
        }
        else
        {
            if(self.GetType().ToString() == "PauseMenu")
            Menu.ToggleButtons(false);
            else return;
        }
        if(PauseWait != null) return;
        anim.enabled = true;
        ///print("Pause: "+toggle);
        Menu.self.firstAwake = false;
        paused = toggle;
        string animName = toggle ? "PauseM_In" : "PauseM_Out";
        anim.Play(animName);

        PauseWait = self.StartCoroutine(IPauseWait(animName,toggle));
    }
}
