using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CheatInput : MonoBehaviour
{
    public static System.Random rand = new System.Random();
    public static readonly string[] codes =
    {
        "evilaudio",    // Random sounds
        "immadoot",     // Engage super mode
        "vitaem##",     // Set lives - (non additive)
        "vitae##",      // Set lives + (non additive)
        "calci#",       // Set HP
        "sandbag",      // Lock HP
        "wananana",     // God mode
        "schrodin",     // Lock life changes
        "chalng",       // Weapons deal 10 damage
        "gotohell",     // You die in one hit
        "idr",          // Reset all
        "evileraudio",  // Super random sounds
    };
    static int charPoint = 0;
    static string typedCode = "";
    static string valueCode = "";
    static int codeDigitInput = -1; // Digit used for codes with digits

    // Cheats

    // If not 0, don't save stats, only completion which can go only up to 1
    // 1 - reset on scene load
    // 2 - persist between scene loads
    // 3 - same as 2, but on reset set to 1
    public static byte CheatVal = 0;
    static byte CheatTogglesActive = 0;
    public static bool LockHealth = false;
    public static bool LockLives = false;
    public static bool OneHitDeaths = false;
    public static bool SuperWeps = false;
    public static bool GodMode = false;
    public static short SoundPlayOffset;

    static void ShowMessage(string par, bool isOn)
    {
        DataShare.ShowText(par.ToUpper()+": "+(isOn ? "ON" : "OFF"));
    }
    static void ShowMessage(string par)
    {
        DataShare.ShowText(par.ToUpper());
    }

    static void SetCheatLevel(short ToSet)
    {
        byte oldLevel = CheatVal;

        CheatVal = (byte)Mathf.Clamp(ToSet,0,(short)CheatVal);

        if(CheatVal == 0 && oldLevel != 0)
        DataShare.ShowText('\n'+"Saving stats: ENABLED");
        
        #if UNITY_EDITOR
        Debug.Log("Cheat level: "+CheatVal);
        #endif
    }
    static void IncreaseCheatLevel(short AddVal)
    {
        if(CheatVal == 0) SaveCanvasAppear.Insert += "Saving stats: DISABLED"+'\n';
        if(CheatVal != AddVal) CheatVal = (byte)Mathf.Min((short)CheatVal+AddVal,CheatTogglesActive>1 ? 3 : 2);
        
        #if UNITY_EDITOR
        Debug.Log("Cheat level: "+CheatVal);
        #endif
    }
    public static void DecreaseCheatLevel(short SubtractVal)
    {
        byte oldLevel = CheatVal;
        CheatVal = (byte)Mathf.Clamp(CheatVal-SubtractVal,0,(short)CheatVal);

        if(CheatVal == 0 && oldLevel != 0)
        DataShare.ShowText("Saving stats: ENABLED");


        #if UNITY_EDITOR
        Debug.Log("Cheat level: "+CheatVal);
        #endif
    }

    static void CodeInput()
    {
        foreach(KeyControl k in Keyboard.current.allKeys)
        { 
            if(MGInput.GetKeyDown(k))
            {
                string v = k.ToString();
                try
                {
                    if(v.Contains("numpad")) v = v.Substring(v.Length-1);
                    else v = v.Substring(v.LastIndexOf('/')+1);

                    //print(v);
                }
                catch (System.IndexOutOfRangeException)
                {
                    continue;
                }

                char c = v[0];
                c = System.Char.ToLower(c);

                // Validate code
                bool valid = false;
                for(int i = 0;i<codes.Length;i++)
                {
                    string s = codes[i];
                    string substr = s;
                    if(charPoint<s.Length)
                    {
                        substr = substr.Substring(0,charPoint);
                    }
                    if(charPoint>0&&typedCode!=substr)
                    {
                        if(typedCode=="") break;
                        continue;
                    }
                    // Check if typed char is a digit, if it is - only type it if it's not the end of the code string
                    
                    char a = '!';
                    if(charPoint<s.Length)
                    a = s[charPoint];

                    // Valid char
                    if(a=='#' && char.IsDigit(c)||a==c)
                    {
                        valid = true;
                        if(a=='#')
                        {
                            typedCode+=a;
                            valueCode+=c;
                            charPoint++;
                            if(charPoint==s.Length)
                            {
                                #if UNITY_EDITOR
                                Debug.Log("Code: "+s+" Value: "+valueCode);
                                #endif
                                int val = -1;
                                int.TryParse(valueCode,out val);
                                if(codeDigitInput==-1)
                                IDCode(s,val,i);
                            }
                            break;
                        }
                        else
                        {
                            typedCode+=c;
                            charPoint++;
                        }

                        EvalCode(typedCode);
                        ///print("Charpoint: "+charPoint+" Typedcode: "+typedCode+" SubString: "+substr);
                        break;
                    }
                }
                ///print(typedCode+" "+typedCode.Length+" cpoint: "+charPoint);
                if(!valid)
                {
                    #if UNITY_EDITOR
                    if(typedCode.Length>2) Debug.Log("Invalid code: "+typedCode);
                    #endif
                    ResetCode();
                    typedCode+=c;
                    charPoint++;
                }
            }
        }
    }
    static void IDCode(string inCode,int id,int bufferID)
    {
        codeDigitInput = id;

        #if UNITY_EDITOR
        print("ID Code: "+inCode+id);
        #endif

        EvalCode(inCode);
    }
    static void ResetCode()
    {
        codeDigitInput = -1;
        charPoint = 0;
        typedCode = "";
        valueCode = "";
    }
    static void EvalCode(string inCode)
    {
        for(int i = 0;i<codes.Length;i++)
        {
            if(inCode==codes[i])
            {
                //print(inCode+" "+i);
                switch(i)
                {
                    default:
                    break;
                    
                    // Random sounds
                    case 0:
                    if(SoundPlayOffset <= 0)
                    SoundPlayOffset = (short)rand.Next(DataShare.audioFilesLength);
                    else SoundPlayOffset = 0;

                    ShowMessage("Random sounds",SoundPlayOffset != 0);
                    break;

                    // Super mode
                    case 1:
                    if(HUD.self == null)
                    {
                        ShowMessage("Needs to be in a level");
                        return;
                    }
                    PlayerControl.SetLives(PlayerControl.LivesRange.y+1,false,false);
                    IncreaseCheatLevel(1);
                    ShowMessage("Super mode");

                    break;

                    // Set lives -
                    case 2:
                    codeDigitInput = Mathf.Clamp(-codeDigitInput,PlayerControl.LivesRange.x,PlayerControl.LivesRange.y);
                    PlayerControl.SetLives(codeDigitInput,false,false);
                    IncreaseCheatLevel(2);
                    ShowMessage("Set lives: "+codeDigitInput);
                    
                    break;

                    // Set lives +
                    case 3:
                    codeDigitInput = Mathf.Clamp(codeDigitInput,PlayerControl.LivesRange.x,PlayerControl.LivesRange.y);
                    PlayerControl.SetLives(codeDigitInput,false,false);
                    IncreaseCheatLevel(2);
                    ShowMessage("Set lives: "+codeDigitInput);
                    break;

                    // Set health
                    case 4:
                    codeDigitInput = Mathf.Clamp(codeDigitInput-1,0,GameMaster.maxHealth);
                    PlayerControl.SetHP(codeDigitInput,false);
                    IncreaseCheatLevel(1);
                    ShowMessage("Set health: "+(codeDigitInput+1));
                    break;

                    // Lock health
                    case 5:
                    LockHealth = !LockHealth;
                    IncreaseCheatLevel((short)(LockHealth ? 2 : -1));
                    CheatTogglesActive = (byte)(CheatTogglesActive + (LockHealth ? 1 : -1));
                    ShowMessage("Locked health",LockHealth);
                    break;

                    // God mode
                    case 6:
                    GodMode = !GodMode;
                    IncreaseCheatLevel((short)(GodMode ? 2 : -1));
                    CheatTogglesActive = (byte)(CheatTogglesActive + (GodMode ? 1 : -1));
                    ShowMessage("God mode",GodMode);
                    break;

                    // Lock life changes
                    case 7:
                    LockLives = !LockLives;
                    IncreaseCheatLevel((short)(LockLives ? 2 : -1));
                    CheatTogglesActive = (byte)(CheatTogglesActive + (LockLives ? 1 : -1));
                    ShowMessage("Locked lives",LockLives);
                    break;

                    // Super weapon damage
                    case 8:
                    SuperWeps = !SuperWeps;
                    IncreaseCheatLevel((short)(SuperWeps ? 2 : -1));
                    CheatTogglesActive = (byte)(CheatTogglesActive + (SuperWeps ? 1 : -1));
                    ShowMessage("Super weapons",SuperWeps);
                    break;

                    // One hit deaths
                    case 9:
                    OneHitDeaths = !OneHitDeaths;
                    ShowMessage("One hit deaths",OneHitDeaths);
                    break;

                    // Reset
                    case 10:
                    SoundPlayOffset = 0;
                    OneHitDeaths = false;

                    if(CheatVal == 0 && CheatTogglesActive == 0)
                    {
                        ResetCode();
                        return;
                    }
                    GodMode = false;
                    LockHealth = false;
                    LockLives = false;
                    SuperWeps = false;
                    CheatTogglesActive = 0;

                    SetCheatLevel((byte)(CheatVal>0 ? 1 : 0));
                    ShowMessage("All cheats disabled");
                    break;

                    // Super random sounds
                    case 11:
                    if(SoundPlayOffset >= 0)
                    SoundPlayOffset = -1;
                    else SoundPlayOffset = 0;
                    ShowMessage("Very random sounds",SoundPlayOffset != 0);
                    break;
                }
                DataShare.PlaySound("Cheat",false,0.1f,1f);
                ResetCode();
                return;
            }
        }
    }
    void OnEnable()
    {
        ResetCode();
    }
    void FixedUpdate()
    {
        if(Time.timeScale != 0)
        CodeInput();
    }
    void Update()
    {
        if(Time.timeScale == 0)
        CodeInput();
    }
}
