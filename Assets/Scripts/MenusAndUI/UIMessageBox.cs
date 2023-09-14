using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class UIMessageBox : Menu
{
    InputSystemUIInputModule uIInputModule;
    void PlayAppearAnim(bool toggle, bool performAction)
    {
        print("Appear anim: "+toggle);
        if(appearAnim!=null) StopCoroutine(appearAnim);
        appearAnim = StartCoroutine(IAppearAnim(toggle, performAction));
    }
    Coroutine appearAnim;
    IEnumerator IAppearAnim(bool appear, bool performAction)
    {
        this.enabled = false;
        uIInputModule = EventSystem.current.transform.GetComponent<InputSystemUIInputModule>();
        uIInputModule.enabled = false;
        float progress = 0;
        float speed = 10;
        Vector3 startScale = !appear ? Vector3.one : Vector3.zero;
        Vector3 endScale = Vector3.one * 1.2f;

        transform.localScale = startScale;
        yield return 0;
        if(appear)
        {
            OpenMenu();
        }
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
        endScale = appear ? Vector3.one : Vector3.zero;
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime * speed;
            transform.localScale = Vector3.Lerp(startScale,endScale,progress);
            yield return 0;
        }
        if(!appear)
        {
            CloseMenu();
            // Confirmation event here
            confirmationEvent?.Invoke(performAction, postConfirmFunc);
        }
        else
        {
            LocalToggleButtons(true);
        }
        uIInputModule.enabled = true;
        
        if(!appear)
        {
            Destroy(gameObject);
        }
        else
        {
            this.enabled = true;
        }
    }
    public void ShowMenu()
    {
        // Appear anim here
        PlayAppearAnim(true,false);
    }

    // Called in sub script by button
    public void Confirmation(bool toggle)
    {
        // Lock options buttons
        LocalToggleButtons(false);

        // Outro anim
        print(transform.name+" Confirmation: "+toggle);

        PlayAppearAnim(false,toggle);
    }
    protected override void Awake()
    {
        base.Awake();
    }
}
