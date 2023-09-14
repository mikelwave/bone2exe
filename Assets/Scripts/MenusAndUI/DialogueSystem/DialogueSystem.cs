using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

class LastIndexedChar
{
    int index = -1;
    char replacedChar;

    public LastIndexedChar(){}
    public char ReplacedChar {get {return replacedChar;} set {replacedChar = value;}}
    public int Index {get {return index;} set {index = value;}}
}
[System.Serializable]

public class Profile
{
    bool active = false;
    [Tooltip("Possible expressions used by this profile. (index position matters)")]
    [SerializeField] Sprite[] faces;
    [Space]
    [Tooltip("Display name for the character")]
    [SerializeField] string CharacterName = "";
    [SerializeField] string CharacterSound = "";

    Image profileImage;
    RectTransform profileRectTransform;
    MonoBehaviour instance;
    Vector2 IdlePos;
    Coroutine slideCor;

    // Assign values
    public Profile(Sprite[] faces, Image profileImage)
    {
        this.faces = faces;
        this.profileImage = profileImage;
        active = false;
    }
    public Profile()
    {
        active = false;
    }
    public void Init(Image profileImage, MonoBehaviour instance, Vector2 IdlePos)
    {
        this.instance = instance;
        this.profileImage = profileImage;
        profileRectTransform = this.profileImage.GetComponent<RectTransform>();
        this.IdlePos = IdlePos;
        UpdateFace(false,true,false);
    }
    public void SetFaceGraphics(Sprite[] faces)
    {
        this.faces = faces;
    }
    public void SetNameAndVoice(string name, string voice)
    {
        CharacterName = name;
        CharacterSound = voice;
    }
    public void SetFace(int ID)
    {
        ID = Mathf.Clamp(ID, 0, faces.Length);
        if(profileImage.sprite == faces[ID]) return;
        profileImage.sprite = faces[ID];
        profileImage.SetNativeSize();
    }
    public void UpdateFace(bool show, bool instant, bool canBump)
    {
        if(active != show || instant)
        {
            // Slide in/out
            if(!instant)
            {
                if(slideCor!=null) instance.StopCoroutine(slideCor);
                slideCor = instance.StartCoroutine(ISlide(show));
            }
            else 
            {
                Vector2 pos = profileRectTransform.anchoredPosition;
                float offset = show ? IdlePos.x*-0.5f : 0;
                pos.x = IdlePos.x + offset;

                profileRectTransform.anchoredPosition = pos;
                profileRectTransform.gameObject.SetActive(show);
            }
        }
        else if (canBump && show == true && slideCor == null) instance.StartCoroutine(IBump());
        active = show;
    }
    public string GetName { get { return CharacterName;} }
    public string GetVoice { get {return CharacterSound;} }
    // Slide on and offscreen (X and color only)

    IEnumerator ISlide(bool slideIn)
    {
        if(slideIn) profileRectTransform.gameObject.SetActive(true);

        Vector2 pos = profileRectTransform.anchoredPosition;
        float offset = IdlePos.x*-0.5f;

        float startPoint = pos.x + (slideIn ? offset : 0);
        float endPoint = IdlePos.x + (slideIn ? 0 : offset);
        Color startColor = new Color(1,1,1,!slideIn ? 1 : 0);
        Color endColor = new Color(1,1,1,slideIn ? 1 : 0);

        float progress = 0;
        while(progress < 1)
        {
            progress+=Time.deltaTime*3f;
            float step = Mathf.SmoothStep(0,1,progress);
            pos.x = Mathf.Lerp(startPoint,endPoint,step);
            profileImage.color = Color.Lerp(startColor,endColor,step);
            profileRectTransform.anchoredPosition = pos;
            yield return 0;
        }
        profileRectTransform.anchoredPosition = pos;
        profileImage.color = endColor;

        if(!slideIn) profileRectTransform.gameObject.SetActive(false);
        slideCor = null;
    }

    // New line talk animation (Y only)
    IEnumerator IBump()
    {
        Vector2 pos = profileRectTransform.anchoredPosition;

        float startPoint = IdlePos.y;
        float endPoint = IdlePos.y + 15;

        float progress = 0;
        while(progress < 1)
        {
            progress+=Time.deltaTime*9f;
            float step = Mathf.SmoothStep(0,1,progress);
            pos.y = Mathf.Lerp(startPoint,endPoint,step);
            profileRectTransform.anchoredPosition = pos;
            yield return 0;
        }
        progress = 0;
        startPoint = endPoint;
        endPoint = IdlePos.y;

        while(progress < 1)
        {
            progress+=Time.deltaTime*9f;
            float step = Mathf.SmoothStep(0,1,progress);
            pos.y = Mathf.Lerp(startPoint,endPoint,step);
            profileRectTransform.anchoredPosition = pos;
            yield return 0;
        }
        profileRectTransform.anchoredPosition = pos;
    }
}
public class DialogueSystem : TextReveal
{
    #region vars
    [Space]
    [Header("Dialogue system")]
    [Space]
    public TextAsset TextFile;
    string textAsString;
    List<string> textAsLines;

    Vector2 pos = Vector2.up*0.5f;

    protected RectTransform BG;
    protected RectTransform TextMeshRect;
    protected TextMeshProUGUI CharName;
    protected GameObject nextArrow;

    [Space]
    [Header ("Character profiles")]
    [Space]

    [SerializeField] protected Profile profileLeft = new Profile();
    [SerializeField] protected Profile profileRight = new Profile();
    protected byte targetProfile = 0;
    float delay = 0;

    protected const float BGOffHeight = 375;
    protected const float BGOnHeight = 155;
    char blankChar = '‎';
    string appendChars = ".?!,";
    bool nextLine = false;
    bool acceptInputs = true;
    public static byte Event = 0;
    bool firstLine = true;

    public delegate void PostConvoEvent();
    public PostConvoEvent postConvoEvent;

    // Camera damping event
    public delegate void SetDamping(float damping);
    public SetDamping setDamping;

    public delegate void PreTalkEvent(bool delay);
    public PreTalkEvent preTalkEvent;

    public delegate void TriggerEvent();
    public static TriggerEvent triggerEvent;

    public static DialogueSystem self;

    #endregion
    public static void ResetEvent()
    {
        Event = 0;
    }
    void Update()
    {
        if(acceptInputs && MGInput.GetButton(MGInput.controls.Player.Jump))
        {
            acceptInputs = false;
            ///print("Button press");
            if(!isRangeMax && textDisplaying)
            {
                skipping = true;
                textDisplaying = false;
            }
            else
            {
                nextLine = true;
            }
        }
        else if(!MGInput.GetButton(MGInput.controls.Player.Jump))
        {
            acceptInputs = true;
        }
    }

    protected virtual IEnumerator IMoveBG(bool show,bool startConvo,float delay)
    {
        if(delay > 0) yield return new WaitForSeconds(delay);
        if(show) BG.gameObject.SetActive(true);

        Vector2 pos = BG.anchoredPosition;

        float startPoint = show ? BGOffHeight : BGOnHeight;
        float endPoint = !show ? BGOffHeight : BGOnHeight;

        float progress = 0;
        while(progress < 1)
        {
            progress+=Time.deltaTime*3f;
            float step = Mathf.SmoothStep(0,1,progress);
            pos.y = Mathf.Lerp(startPoint,endPoint,step);
            BG.anchoredPosition = pos;
            yield return 0;
        }
        BG.anchoredPosition = pos;
        if(show && startConvo) StartConvo();
        else if(!show) BG.gameObject.SetActive(false);
    }
    public void Awake()
    {
        self = this;
    }
    protected virtual void Init()
    {
        BG = transform.GetChild(0).GetComponent<RectTransform>();
        Vector2 pos = BG.anchoredPosition;
        pos.y = BGOffHeight;
        BG.anchoredPosition = pos;
        BG.gameObject.SetActive(false);
        nextArrow = transform.GetChild(5).gameObject;
        nextArrow.SetActive(false);

        TextMeshRect = transform.GetChild(4).GetComponent<RectTransform>();
        m_TextComponent = TextMeshRect.GetComponent<TextMeshProUGUI>();
        CharName = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        CharName.text = "";
        m_TextComponent.text = "";

        profileLeft.Init(transform.GetChild(1).GetComponent<Image>(),this,new Vector2(142,-106));
        profileRight.Init(transform.GetChild(2).GetComponent<Image>(),this,new Vector2(-142,-106));
        ResetEvent();

        PrepareText(TextFile);
    }

    // Active character profile switching
    // 0 - none, 1 - left, 2 - right
    protected virtual void SetProfile(byte ID,bool instant)
    {
        CharName.alignment = ID == 1 ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
        CharName.text = ID == 1 ? profileRight.GetName : ID == 2 ? profileLeft.GetName : "";
        CharVoiceSound = ID == 1 ? profileRight.GetVoice : ID == 2 ? profileLeft.GetVoice : "";
        Vector3 vec = TextMeshRect.anchoredPosition;
        // ID - 0 = 0, 1 = *-1, 2+ = * 1
        vec.x = (ID == 0 ? 0 : ID == 1 ? -1 : 1) * 110.1f;
        TextMeshRect.anchoredPosition = vec;
        m_TextComponent.alignment = ID == 1 ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;

        // Animate pictures
        profileLeft.UpdateFace(ID == 2,instant,ID == 2);
        profileRight.UpdateFace(ID == 1,instant,ID == 1);
        targetProfile = ID;
    }
    LastIndexedChar GetLastIndex(string s)
    {
        LastIndexedChar lastIndexedChar = new LastIndexedChar();
        foreach(char c in appendChars)
        {
            int LastCharIndex = s.LastIndexOf(c);
            if(LastCharIndex>lastIndexedChar.Index)
            {
                lastIndexedChar.Index = LastCharIndex;
                lastIndexedChar.ReplacedChar = c;
            }
        }
        return lastIndexedChar;

    }
    string ReplaceInString(string s)
    {
        foreach(char c in appendChars)
        {
            string charS = c.ToString();
            string repeat = new string((blankChar),c == ',' ? 5 : 9);


            s = s.Replace(charS+' ',charS + repeat + ' ' );
            s = s.Replace(charS+(char)13,charS + repeat + (char)13);
        }

        return s;
    }
    string FormatString(string s,int lineIndex)
    {
        bool lastLine = lineIndex+1 >= textAsLines.Count || textAsLines[lineIndex+1].Length<=2;

        if(s.StartsWith("[") || textAsLines[lineIndex].Length<=2) return s;
        else
        {
            if(s.Length == 0) return s;
            
            if(lastLine)
            {
                // Ignore last character's breaks.
                LastIndexedChar lastIndexedChar = GetLastIndex(s);
                if(lastIndexedChar.Index!=-1)
                {
                    s = s.Remove(lastIndexedChar.Index,1).Insert(lastIndexedChar.Index,'ඞ'.ToString());
                }

                s = ReplaceInString(s);

                s = s.Replace('ඞ',lastIndexedChar.ReplacedChar);
            }
            else
            {
                s = ReplaceInString(s);
            }

            return s;
        }
    }
    public static void PrepareText(TextAsset text)
    {
        if(text==null&&self.TextFile==null)
        {
            Debug.LogError("No text file assigned.");
            return;
        }
        self.TextFile = text;
        self.textAsString = self.TextFile.text;
		self.textAsLines = new List<string>();
		self.textAsLines.AddRange(self.textAsString.Split("\n"[0]));
        for(int i = 0;i<self.textAsLines.Count;i++)
        {
            self.textAsLines[i] = self.FormatString(self.textAsLines[i],i);
        }
    }
    public static void SetProfileData(int profileValue, Sprite[] profileSprites, string characterName, string characterSound)
    {
        Profile usedProfile = profileValue == 0 ? self.profileLeft : self.profileRight;
        usedProfile.SetFaceGraphics(profileSprites);
        usedProfile.UpdateFace(false,true,false);
        usedProfile.SetNameAndVoice(characterName,characterSound);
    }
    public static void SetProfileData(int profileValue,Profile newProfile)
    {
        if(self == null) Resources.FindObjectsOfTypeAll<DialogueSystem>()[0].Awake();
        if(profileValue == 0)
        {
            newProfile.Init(self.transform.GetChild(1).GetComponent<Image>(),self,new Vector2(142,-106));
            self.profileLeft = newProfile;
        }
        else
        {
            newProfile.Init(self.transform.GetChild(2).GetComponent<Image>(),self,new Vector2(-142,-106));
            self.profileRight = newProfile;
        }
    }
    int lineIndex = 0;
    readonly string[] stringSeparators = new string[] {", "};
    
    public static void StartConvo(int startLine, float delay = 1.0f)
    {
        PlayerControl.freezePlayerInput++;
        if(self.BG == null) self.Init();
        self.lineIndex = startLine;
        self.gameObject.SetActive(true);
        self.StartCoroutine(self.IMoveBG(true,true,delay));
    }
    protected void StartConvo()
    {
        if(HUD.self != null && HUD.self.cam != null)
        GetComponent<Canvas>().worldCamera = HUD.self.cam;
        if(convoCor!=null)StopCoroutine(convoCor);

        convoCor = StartCoroutine(IConvo(lineIndex));
    }
    float FloatParse(string inStr)
    {
        float outVal = -999;
        try
        {
            float.TryParse(inStr,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture, out outVal);
        }
        catch (System.FormatException)
        {
            Debug.LogError("Invalid string:" + inStr);
            throw;
        }
        if(outVal == -999) Debug.LogError("Invalid string:" + inStr);

        return(outVal);
    }
    int IntParse(string inStr)
    {
        int outVal = -999;
        try
        {
            int.TryParse(inStr, out outVal);
        }
        catch (System.FormatException)
        {
            Debug.LogError("Invalid string:" + inStr);
            throw;
        }
        if(outVal == -999) Debug.LogError("Invalid string:" + inStr);

        return(outVal);
    }
    IEnumerator IConvoHide(int savedLine, float TimeWait)
    {
        yield return new WaitForSeconds(TimeWait);
        StartConvo(savedLine);
    }
    public Coroutine convoCor;
    IEnumerator IConvo(int startLine)
    {
        string s = "";
        m_TextComponent.text = s;
        bool bump = true;
        yield return 0;
        for(int i = startLine;i<textAsLines.Count;i++)
        {
            string line = textAsLines[i];
            // Settings line
            if(line.Contains("["))
            {
                line = line.Replace("[","");
                line = line.Replace("]","");
                string[] splitText = line.Split(stringSeparators, System.StringSplitOptions.None);
                string[] splitCommand;
                for(int j = 0;j<splitText.Length;j++)
                {
                    splitCommand = splitText[j].TrimEnd((char)13).Split(' ');

                    string sl = splitCommand[0].ToLower();

                    // Determine command type
                    switch(sl)
                    {
                        default:
                        ///print(splitCommand[0].ToLower());
                        break;

                        case "talker":
                        string target = splitCommand[2].Replace(",","").ToLower();
                        if(target.ToLower()=="null")
                        {
                            SetProfile(0,false);
                            bump = false;
                        }
                        else
                        {
                            targetProfile = (byte)(target.Contains("1") ? 1 : 2);
                            SetProfile(targetProfile,false);
                            bump = false;
                        }
                        break;
                        case "emotion":

                            int ID = IntParse(splitCommand[2]);

                            Profile p = targetProfile == 2 ? profileLeft : targetProfile == 1 ? profileRight : null;

                            if(p!=null)
                            {
                                p.SetFace(ID);
                            }
                        break;

                        case "pos":
                            splitCommand = splitCommand[2].Split(',');
                            Vector2 pos = new Vector2(FloatParse(splitCommand[0]),FloatParse(splitCommand[1]));
                        break;

                        case "damping":
                            float damping = FloatParse(splitCommand[2]);
                            setDamping?.Invoke(damping);
                        break;

                        case "fontsize":
                            float size = FloatParse(splitCommand[2]);
                            m_TextComponent.fontSize = size;
                        break;

                        case "delay":
                            delay = FloatParse(splitCommand[2]);
                        break;

                        case "hidetime":
                        float hidetime = FloatParse(splitCommand[2]);
                        StartCoroutine(IConvoHide(i+1,hidetime));

                        KillText();
                        EndConvoReset(false);
                        yield break;

                        case "event":
                        Event++;
                        triggerEvent?.Invoke();
                        break;
                    }
                }
            }

            // Text line
            else
            {
                if(bump)
                {
                    // Bump the portrait
                    if(line.Length>1)
                    SetProfile(targetProfile,false);
                    bump = false;
                }
                // End of text bubble
                if(line.Length==0||(int)line[0]==13)
                {
                    if(s=="")
                    {
                        m_TextComponent.text = s;
                        if(delay!=0)
                        {
                            KillText();
                            yield return new WaitForSeconds(delay);
                            delay = 0;
                        }
                        ScanText(true);

                        EndConvoReset(true);
                        yield break;
                    }
                    if(delay!=0)
                    {
                        KillText();
                        yield return new WaitForSeconds(delay);
                        delay = 0;
                    }
                    ///print("Line end");
                    m_TextComponent.text = s;
                    preTalkEvent?.Invoke(i>3);
                    ScanText(!firstLine);
                    firstLine = false;
                    // Wait until all text is displayed, and then check for a button input
                    yield return 0;
                    yield return new WaitUntil(()=>textDisplaying);
                    ///print("Text is displaying");

                    yield return 0;
                    yield return new WaitUntil(()=>isRangeMax);
                    nextArrow.SetActive(true);
                    ///print("Can progress");
                    if(skipping)
                    {
                        skipping = false;
                        yield return 0;
                    }
                    nextLine = false;
                    yield return new WaitUntil(()=>nextLine);
                    nextLine = false;
                    ///print("Next");
                    s="";
                    nextArrow.SetActive(false);
                    bump = true;
                    continue;
                }
                s+=line+'\n';
            }
        }
        EndConvoReset(true);
    }
    void EndConvoReset(bool freePlayer)
    {
        SetProfile(0,false);
        StartCoroutine(IMoveBG(false,false,0.5f));
        m_TextComponent.text = "";
        CharName.text = "";
        m_TextComponent.ForceMeshUpdate();
        ///print("End of convo");
        // If previous line was empty, it's the end of convo
        convoCor = null;
        postConvoEvent?.Invoke();
        if(freePlayer && PlayerControl.freezePlayerInput>0) PlayerControl.DecPlayerFreeze();
    }
}
