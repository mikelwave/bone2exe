using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreditsSequence : MonoBehaviour
{
    float musicLength = 0;
    const float fallBackLength = 73.178f;
    const float logoAppearTime = 4.478f; // Time from the end to show the copyright logo
    bool logoAppeared = false;
    bool skip = false;
    Image logo;
    [SerializeField] float camScrollDestination = -1400f;
    [SerializeField] float camScrollTwoDestination = -800f;
    [SerializeField] Transform cameraTr;
    [SerializeField] Transform stageCameraTr;
    [SerializeField] Image skipRenderer;

    public float elapsedTime = 0;


    IEnumerator ISkipCooldown()
    {
        skip = true;
        Color c = skipRenderer.color;
        c.a = 0;
        float progress = 0;
        while(progress<1)
        {
            progress += Time.deltaTime*5;
            yield return 0;
            c.a = Mathf.Lerp(0,1,progress);
            skipRenderer.color = c;
        }
        c.a = 1;
        skipRenderer.color = c;
        ///print("Skip?");
        yield return new WaitForSeconds(2f);
        ///print("Skip expired");
        progress = 0;
        while(progress<1)
        {
            progress += Time.deltaTime*5;
            yield return 0;
            c.a = Mathf.Lerp(1,0,progress);
            skipRenderer.color = c;
        }
        c.a = 0;
        skipRenderer.color = c;
        skip = false;
    }
    IEnumerator ILogoAppear()
    {
        float progress = 0;
        logo = transform.GetChild(1).GetComponent<Image>();
        Transform logoTr = logo.transform;
        logo.color = Color.black;
        logoTr.localScale = Vector3.zero;
        float endScale = 1.1f;

        yield return new WaitForSeconds(1f);

        while(progress<1)
        {
            progress+=Time.deltaTime*5;
            logo.color = Color.Lerp(Color.black, Color.white,progress);
            logoTr.localScale = (progress * endScale) * Vector3.one; 
            yield return 0;
        }
        progress = 0;
        while(progress<1)
        {
            progress+=Time.deltaTime*8;
            logoTr.localScale = Mathf.Lerp(endScale,1,progress) * Vector3.one;
            yield return 0;
        }
    }

    IEnumerator ICreditsSequence()
    {
        // Declares
        StartCoroutine(ILogoAppear());
        yield return new WaitForSeconds(1f);
        // Music
        DataShare.LoadMusic("Credits",false);
        yield return 0;
        yield return new WaitUntil(()=>DataShare.songLoaded);
        if(!DataShare.trueMidiMusic && DataShare.MusicIsPlaying)
        {
            musicLength = DataShare.MusicLength;
        }
        else musicLength = fallBackLength;

        float lastLogoAppearTime = musicLength - logoAppearTime;
        ///print("Music length: "+musicLength);
        ///print("Logo appear time: "+lastLogoAppearTime);
        Time.timeScale = 1;
        elapsedTime = 0;

        // Animation
        Vector3 pos = cameraTr.position;
        Vector3 pos2 = stageCameraTr.position;
        yield return new WaitForSeconds(1f);
        musicLength-=1;

        while(elapsedTime<musicLength)
        {
            elapsedTime+=Time.deltaTime;
            float val = elapsedTime/musicLength;
            pos.y = Mathf.Lerp(0,camScrollDestination,val);
            pos2.y = Mathf.Lerp(0,camScrollTwoDestination,val);
            cameraTr.position = pos;
            stageCameraTr.position = pos2;
            if(!logoAppeared && elapsedTime>=lastLogoAppearTime)
            {
                logoAppeared = true;
                ShowCopyLogo();
            }
            yield return 0;
        }
        pos.y = camScrollDestination;
        pos2.y = camScrollTwoDestination;
        cameraTr.position = pos;
        stageCameraTr.position = pos2;
        // Logo
        if(!logoAppeared)
        {
            logoAppeared = true;
            ShowCopyLogo();
        }
        yield return new WaitForSeconds(1f);
        LoadMainMenu();
    }
    void ShowCopyLogo()
    {
        stageCameraTr.GetChild(0).GetChild(1).GetComponent<ParticleSystem>().Play();
    }
    // Start is called before the first frame update
    void Start()
    {
        Color c = skipRenderer.color;
        c.a = 0;
        skipRenderer.color = c;
        DataShare.LoadMusic("",false);
        PlayerControl.currentHealth = GameMaster.maxHealth;
        StartCoroutine(ICreditsSequence());
    }

    void FixedUpdate()
    {
        if(MGInput.GetButtonDown(MGInput.controls.Player.Jump))
        {
            if(skip)
            {
                LoadMainMenu();
                print("Skipped");
            }
            else StartCoroutine(ISkipCooldown());
        }
    }

    void LoadMainMenu()
    {
        StopAllCoroutines();
        DataShare.LoadMusic("",false);
        DataShare.LoadSceneWithTransition("TitleScreen");
    }
}
