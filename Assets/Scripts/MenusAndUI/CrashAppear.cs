using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrashAppear : MonoBehaviour
{
    Image image;
    AudioSource audioSource;
    TextMeshProUGUI text;
    [SerializeField] byte crashScreenAmount = 13;
    [SerializeField] TextAsset textAsset;

    System.Random rand = new System.Random();
    bool autoTyping = false;
    bool waiting = false;

    Coroutine UnderscoreAnimateCor;
    // End underscore animation
    IEnumerator IUnderscoreAnimate()
    {
        float timeToWait = 0.25f;
        while(true)
        {
            int LastChar = text.text.Length-1;
            if(text.text[LastChar] == '_')
            {
                text.text = text.text.Remove(LastChar,1);
            }
            else
            {
                text.text+='_';
            }

            while(timeToWait>0)
            {
                timeToWait-=Time.unscaledDeltaTime;
                yield return 0;
            }
            timeToWait = 0.25f;
        }
    }
    void Wait(float timeToWait)
    {
        if(waitCor != null) StopCoroutine(waitCor);
        waitCor = StartCoroutine(IWait(timeToWait));
    }
    Coroutine waitCor;
    IEnumerator IWait(float timeToWait)
    {
        waiting = true;
        while(timeToWait>0)
        {
            timeToWait-=Time.unscaledDeltaTime;
            yield return 0;
        }
        waiting = false;
    }
    IEnumerator ITextDisplay(bool noSound)
    {
        image.color = Color.white;
        yield return 0;
        // Show crash screen first
        if(noSound)
        {
            Wait(5f);
            yield return 0;    
            yield return new WaitUntil(()=>!waiting);
        }
        else
        {
            yield return 0;
            yield return new WaitUntil(()=>!audioSource.isPlaying);
        }

        // Start terminal screen
        image.color = Color.black;
        string[] textAsLines = textAsset.text.Split((char)13);
        text.text = "";
        yield return 0;
        text.gameObject.SetActive(true);
        text.text = "";
        for (int i = 0; i < textAsLines.Length; i++)
        {
            text.text+=textAsLines[i];
            yield return 0;
            yield return 0;
        }

        // Wait for any key
        Wait(0.15f);
        yield return 0;
        yield return new WaitUntil(()=>!waiting);

        text.text += "\n\nPress any key to continue.";
        UnderscoreAnimateCor = StartCoroutine(IUnderscoreAnimate());

        yield return new WaitUntil(()=>MGInput.AnyKeyDown());
        int LastChar = text.text.Length-1;
        if(text.text[LastChar] == '_')
        {
            text.text = text.text.Remove(LastChar,1);
        }

        text.text += "\nC:"+"\\"+"con"+"\\";
        autoTyping = true;

        Wait(1.5f);
        yield return 0;
        yield return new WaitUntil(()=>!waiting);

        StartCoroutine(autoType("Bone2.exe"));
        yield return new WaitUntil(()=>!autoTyping);

        Wait(1.5f);
        yield return 0;
        yield return new WaitUntil(()=>!waiting);

        StopCoroutine(UnderscoreAnimateCor);
        text.text = "";

        GameMaster.ReloadLevelCrash();
        //Debug.Log("Ended");
    }

    IEnumerator autoType(string toType)
    {
        for(int i = 0;i<toType.Length;i++)
        {
            string s = text.text;
            if(s[s.Length-1]=='_')
            {
                s = s.Remove(s.Length-1,1);
                text.text = s;
            }
            text.text += toType[i];
            if(i==toType.Length-1)
            {
                autoTyping = false;
                yield break;
            }
            Wait(0.1f);
            yield return 0;
            yield return new WaitUntil(()=>!waiting);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        image = transform.GetChild(0).GetComponent<Image>();
        audioSource = transform.GetComponent<AudioSource>();
        text = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        text.gameObject.SetActive(false);

        string path = "Images/";
        byte RandomByte = (byte)(rand.Next(crashScreenAmount)+1);
        string filename = "crash screen " + RandomByte.ToString();

        var sprite = Resources.Load<Sprite>(path+filename);
        image.sprite = sprite;

        path = "Audio/";
        var sound = Resources.Load<AudioClip>(path+filename);

        print("File name: "+filename+" has audio: "+(sound!=null));
        if(sound!=null)
        {
            audioSource.clip = sound;
            audioSource.Play();
        }
        DataShare.IncreaseRunTimer(7); // Punishment for crashing
        StartCoroutine(ITextDisplay(sound == null));
    }
}
