using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{

    [Header ("Sprites")]
    [Space]
    public Image Head;
    public Image XIcon;
    public RectTransform livesTextTransform;
    public static TextMeshProUGUI livesText;
    int livesTextBoilInt = 0;
    Image[] bones;
    public Sprite[] headSprites,XIconSprites,BoneSprites;

    // Animation States
    BoilState headNormal = new BoilState(new int[]{0,1});
    BoilState xBoil = new BoilState(new int[]{0,1});
    BoilState headRed = new BoilState(new int[]{2,3});
    BoilState headSuper = new BoilState(new int[]{4,5});
    BoilState headPreSuper = new BoilState(new int[]{7,8});
    BoilState boneNormal = new BoilState(new int[]{0,1});
    BoilState headCrash = new BoilState(new int[]{6});
    BoilState boneFire = new BoilState(new int[]{3,4});
    BoilState boneBroken = new BoilState(new int[]{2});

    BoilState curHeadState;
    BoilState[] curBoneState;

    public static HUD self;
    public static int headStateInt;
    public Camera cam;
    CanvasGroup canvasGroup;

    Transform playerTransform,cameraTransform;
    [SerializeField]
    float heightOffset = 0,XOffset = 0;
    bool transparent = false;
    Coroutine fadeHud;
    IEnumerator IFadeHUD()
    {
        float startAlpha = canvasGroup.alpha;
        float endAlpha = transparent ? 0.3f : 1f;
        float progress = 0;
        float speed = 10f;
        while(progress<1)
        {
            progress += Time.deltaTime*speed;
            canvasGroup.alpha = Mathf.Lerp(startAlpha,endAlpha,progress);
            yield return 0;
        }
        canvasGroup.alpha = endAlpha;
        fadeHud = null;
    }
    void HUDTransparency()
    {
        Vector3 playerPos = playerTransform.position;
        Vector3 cameraPos = cameraTransform.position;
        if((!transparent && playerPos.y>=cameraPos.y+heightOffset && playerPos.x < cameraPos.x-XOffset)
        ||(transparent && playerPos.y<cameraPos.y+heightOffset))
        {
            transparent = !transparent;
            if(fadeHud!=null)StopCoroutine(fadeHud);
            fadeHud = StartCoroutine(IFadeHUD());
        }
    }
    public static void SetHeadState(int index)
    {
        if(self == null) return;
        headStateInt = index;
        switch(index)
        {
            default:
            self.curHeadState = self.headNormal;
            break;
            // pre super
            case 1:
            self.curHeadState = self.headPreSuper;
            break;
            // super
            case 2:
            self.SetBonesFire();
            self.curHeadState = self.headSuper;
            break;
            // red
            case 3:
            self.curHeadState = self.headRed;
            break;
            // crash
            case 4:
            self.CancelInvoke("Animate");
            self.curHeadState = self.headCrash;
            self.Head.sprite = self.headSprites[self.curHeadState.GetIndex(0)];
            livesText.text = "DEAD";
            break;
        }
    }
    // If bone value changed
    bool SetBoneState(int boneIndex,int index)
    {
        BoilState oldState = curBoneState[boneIndex];
        switch(index)
        {
            default:
            curBoneState[boneIndex] = boneNormal;
            break;
            // fire
            case 1:
            curBoneState[boneIndex] = boneFire;
            break;
            // broken
            case 2:
            curBoneState[boneIndex] = boneBroken;
            break;
        }
        return !(curBoneState[boneIndex] == oldState);
    }
    // Start is called before the first frame update
    void Awake()
    {
        self = this;
        livesText = livesTextTransform.GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        if(canvasGroup == null)
        {
            canvasGroup = self.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        cameraTransform = GameObject.FindWithTag("MainCamera").transform.GetChild(0);
        cam = cameraTransform.GetComponent<Camera>();
        GetComponent<Canvas>().worldCamera = cam;

        playerTransform = GameObject.FindWithTag("Player").transform;


        // Generate health
        Transform holder = transform.GetChild(1);
        bones = new Image[GameMaster.maxHealth+1];
        GameObject sample = holder.GetChild(0).gameObject;
        bones[0] = sample.GetComponent<Image>();
        curBoneState = new BoilState[GameMaster.maxHealth+1];

        for(int i = 0; i<GameMaster.maxHealth;i++)
        {
            GameObject obj = Instantiate(sample);
            obj.transform.SetParent(holder);
            obj.GetComponent<RectTransform>().localPosition = new Vector3(23*(i+2),-96,0);
            obj.transform.localScale = Vector3.one;
            bones[i+1] = obj.GetComponent<Image>();

        }
        
        InvokeRepeating("Animate",0,0.1f);
        InvokeRepeating("HUDTransparency",0,0.1f);
    }
    public static void SetHealthDisplay(int curHealth,bool animate)
    {
        if(self == null) return;
        self.SetBones(curHealth,animate);
    }
    public static void SetLivesDisplay(int currentLives,bool animate)
    {
        if(self == null) return;
        if(animate)
        {
            livesText.GetComponent<Animation>().Play();
        }
        livesText.text = currentLives.ToString("");
    }
    void SetBones(int curHealth,bool animate)
    {
        for(int i = 0; i<bones.Length; i++)
        {
            bones[i].enabled = true;
            if(SetBoneState(i,i <= curHealth ? 0 : 2) && animate)
            {
                bones[i].GetComponent<Animation>().Play();
            }
        }
    }
    void SetBonesFire()
    {
        int curHealth = PlayerControl.currentHealth; 
        for(int i = 0; i<bones.Length; i++)
        {
            SetBoneState(i,1);
            bones[i].GetComponent<Animation>().Play();
        }
    }

    // Animate
    void Animate()
    {
        Head.sprite = headSprites[curHeadState.GetIndexAndIncrease()];
        Head.SetNativeSize();
        XIcon.sprite = XIconSprites[xBoil.GetIndexAndIncrease()];
        for(int i = 0; i<bones.Length;i++)
        {
            bones[i].sprite = BoneSprites[curBoneState[i].GetIndexAndIncrease()];
        }
        if(PlayerControl.currentHealth%2==1) curBoneState[0].IncreaseIndex();

        livesTextBoilInt = (int)Mathf.Repeat(livesTextBoilInt+=1,2);

        Vector3 rotation = Vector3.zero;
        rotation.z = livesTextBoilInt == 0 ? 0 : -1.5f;
        livesTextTransform.eulerAngles = rotation;
    }
}
