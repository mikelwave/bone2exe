using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class SpinnerItem
{
    SpinnerEffect main;
    SpriteRenderer spriteRenderer;
    GameObject obj;
    public GameObject gameObject {get {return obj;}}
    public Transform transform {get {return obj.transform;}}
    public SpinnerItem(GameObject obj, SpinnerEffect main)
    {
        this.obj = obj;
        this.main = main;
        spriteRenderer = obj.transform.GetChild(0).GetComponent<SpriteRenderer>();
    }
    public void Spawn()
    {
        main.StartCoroutine(IAppear(true));
    }
    public void Despawn()
    {
        main.StartCoroutine(IAppear(false));
    }
    IEnumerator IAppear(bool appear)
    {
        if(appear) gameObject.SetActive(true);
        Color c = spriteRenderer.color;
        c.a = appear ? 0 : 1;
        float alphaIn = c.a;
        float alphaOut = appear ? 1 : 0;
        float progress = 0;
        while(progress < 1)
        {
            progress += Time.deltaTime;
            c.a = Mathf.Lerp(alphaIn,alphaOut,progress);
            spriteRenderer.color = c;
            yield return 0;
        }
        c.a = alphaOut;
        spriteRenderer.color = c;
    }
}
[System.Serializable]
public class SpinnerEffect : MonoBehaviour
{
    byte DisplayAmount = 11;
    [SerializeField] float maxCircleRadius = 5;
    float spinSpeed = 0.5f;
    SpinnerItem[] items;
    float spinDelta = 0;
    float angle = 0;
    float currentCircleRadius = 0;

    delegate void UpdateEvent();
    UpdateEvent updateEvent;
    public SpinnerEffect()
    {

    }
    public void Init(byte generateAmount)
    {
        DisplayAmount = generateAmount;
        maxCircleRadius = generateAmount <= 1 ? 0 : Mathf.Clamp((float)generateAmount/4,2,10);
        GenerateItems();
        Destroy(GameObject.Find("PauseMenu"));
    }

    // Update is called once per frame
    void Update()
    {
        updateEvent?.Invoke();
    }
    public void Appear(bool appear)
    {
        if(wordAppearCor!=null) StopCoroutine(wordAppearCor);
        wordAppearCor = StartCoroutine(IAppearEffect(appear));
    }
    Coroutine wordAppearCor;
    IEnumerator IAppearEffect(bool appear)
    {
        if(appear)
        {
            updateEvent = Spin;
            foreach(SpinnerItem item in items)
            item.Spawn();

            DataShare.PlaySound("MrMix_KeysAppear",transform.position,false);
        }
        else
        {
            yield return 0;
            foreach(SpinnerItem item in items)
            item.Despawn();

            DataShare.PlaySound("MrMix_KeysDisappear",transform.position,false);
        }
        float startCircleRadius = currentCircleRadius;
        float endCircleRadius = appear ? maxCircleRadius : 0;
        float progress = 0;
        while(progress<1)
        {
            progress += Time.deltaTime;
            float mathStep = Mathf.SmoothStep(0,1,progress);
            currentCircleRadius = Mathf.Lerp(startCircleRadius,endCircleRadius,mathStep);

            yield return 0;
        }
        if(!appear)
        {

            foreach(SpinnerItem item in items)
            item.gameObject.SetActive(false);
            updateEvent = null;
        }
    }
    void Spin()
    {
        angle = Mathf.Repeat(angle+Time.deltaTime*spinSpeed,1);
        for(int i = 0; i<DisplayAmount;i++)
        {
            float localAngle = Mathf.Repeat(angle+spinDelta*i,1) * Mathf.PI * 2;

            Vector2 pos;
            pos.x = Mathf.Sin(localAngle) * currentCircleRadius;
            pos.y = Mathf.Cos(localAngle) * currentCircleRadius;

            items[i].transform.localPosition = pos;
        }
    }
    void GenerateItems()
    {
        items = new SpinnerItem[DisplayAmount];
        GameObject sample = transform.GetChild(0).gameObject;
        SpinnerItem sampleItem = new SpinnerItem(sample,this);

        items[0] = sampleItem;

        spinDelta = 1/(float)DisplayAmount;
        sample.SetActive(false);
        for(int i = 1; i<DisplayAmount;i++)
        {
            Transform newItem = Instantiate(sample,sample.transform.position,Quaternion.identity).transform;
            newItem.SetParent(transform);
            sampleItem = new SpinnerItem(newItem.gameObject,this);
            items[i] = sampleItem;
            newItem.gameObject.SetActive(false);
        }

    }
}
public class TierlistMenu : MonoBehaviour
{
    [SerializeField] DialogueSystem dialogueSystem;
    [SerializeField] Transform SpinnerEffectRoot;
    SpinnerEffect spinnerEffect;
    GameObject Cursor;

    // Requirements for each tier: S, A , B, C, D, F is automatic 0
    [SerializeField] byte[] tierRequierments = new byte[5] {41,30,11,6,1};
    byte targetTier = 0;

    IEnumerator IShowSpinnerEffect()
    {
        float progress = 0;
        spinnerEffect.Appear(true);
        yield return new WaitForSeconds(1.5f);
        Vector3 pos = SpinnerEffectRoot.localPosition;
        float startPos = pos.x;
        float endPos = 12.5f;
        while(progress<1)
        {
            progress += Time.deltaTime/2;
            float mathStep = Mathf.SmoothStep(0,1,progress);
            pos.x = Mathf.Lerp(startPos,endPos,mathStep);
            SpinnerEffectRoot.localPosition = pos;
            yield return 0;
        }
        spinnerEffect.Appear(false);
        yield return new WaitForSeconds(1f);

    }

    IEnumerator ICursorShow()
    {
        // Show cursor here
        Cursor.SetActive(true);
        MoveToPoint moveToPoint = Cursor.GetComponent<MoveToPoint>();
        float startSpeed = moveToPoint.speed;
        float endSpeed = startSpeed/2;
        while(moveToPoint.Progress<1)
        {
            moveToPoint.speed = Mathf.Lerp(startSpeed,endSpeed,moveToPoint.Progress);
            yield return 0;
        }
        // Reached end, move tierlist left by 50
        Transform BGHolder = transform.GetChild(0);
        Transform tierTransform = BGHolder.GetChild(targetTier);
        tierTransform.GetChild(0).GetComponent<RectTransform>().anchoredPosition += Vector2.right * 51;

        // Add element to list
        Transform newElement = BGHolder.GetChild(BGHolder.childCount-1);
        newElement.SetParent(tierTransform);
        newElement.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // Show element ghost
        Image elementImage = newElement.GetComponent<Image>();
        elementImage.color = new Color(1,1,1,0.5f);
        newElement.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        //Hide cursor
        elementImage.color = Color.white;
        DataShare.PlaySound("Cursor_click",Cursor.transform.position,false);

        Cursor.transform.GetChild(0).gameObject.SetActive(false);

        //Lerp cursor color
        yield return new WaitForSeconds(0.75f);
        Image CursorImage = Cursor.transform.GetChild(1).GetComponent<Image>();
        Color c = CursorImage.color;
        float startColorAlpha = c.a;
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime*2;
            c.a = Mathf.Lerp(startColorAlpha,0,progress);
            CursorImage.color = c;
            yield return 0;
        }
        Cursor.SetActive(false);

    }

    void Sequence()
    {
        //print("Event: "+DialogueSystem.Event);
        switch(DialogueSystem.Event)
        {
            default:
            break;

            // Reaction
            case 1:
                int startLine = 0;
                if(targetTier != 5) StartCoroutine(IShowSpinnerEffect());
                switch(targetTier)
                {
                    default:
                    break;
                    case 5: startLine = 80; break; // F tier
                    case 4: startLine = 67; break; // D tier
                    case 3: startLine = 55; break; // C tier
                    case 2: startLine = 41; break; // B tier
                    case 1: startLine = 26; break; // A tier
                    case 0: startLine = 12; break; // S tier
                }
                DialogueSystem.StartConvo(startLine,startLine == 80 ? 1.5f : 4.5f);
            break;

            // Cursor appear
            case 2:
            StartCoroutine(ICursorShow());
            break;

            // Queue loading next scene
            case 3:
            DialogueSystem.self.postConvoEvent = LoadCredits;
            break;
        }
    }

    void LoadCredits()
    {
        DialogueSystem.self.postConvoEvent -= LoadCredits;
        StartCoroutine(ILoadScene());
        
    }
    IEnumerator ILoadScene()
    {
        yield return new WaitForSeconds(1f);
        DataShare.LoadSceneWithTransition("Credits");
    }

    char byteTierToChar(byte tierByte)
    {
        switch (tierByte)
        {
            default: return 'F';
            case 4: return 'D';
            case 3: return 'C';
            case 2: return 'B';
            case 1: return 'A';
            case 0: return 'S';
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        DialogueSystem.triggerEvent = Sequence;
        CamControl.cameraTransform = GameObject.FindWithTag("MainCamera").transform.GetChild(0);
        spinnerEffect = SpinnerEffectRoot.gameObject.AddComponent<SpinnerEffect>();
        Cursor = transform.GetChild(1).gameObject;
        Cursor.SetActive(false);

        byte result = (byte)DataShare.GetSpecialCollectCount();
        print("Result: "+result);

        // Compare with found result to determine tier
        for (int i = tierRequierments.Length-1; i >= 0; i--)
        {
            // If current level is too high for result, make it the previous tier
            if(tierRequierments[i] > result)
            {
                targetTier = (byte)(i+1);
                break;
            }
        }
        print("Target tier: "+targetTier + " as char: "+byteTierToChar(targetTier));
        float targetPoint = (8.78f-2*(int)targetTier);
        ///print("Target point y: "+targetPoint);
        Cursor.GetComponent<MoveToPoint>().targetPoint.y = targetPoint;
        switch(targetTier)
        {
            default: Destroy(spinnerEffect.gameObject); break; // F tier
            case 4: spinnerEffect.Init((byte)Mathf.Clamp(result,1,3)); break; // D tier
            case 3: spinnerEffect.Init(4); break; // C tier
            case 2: spinnerEffect.Init(5); break; // B tier
            case 1: spinnerEffect.Init(8); break; // A tier
            case 0: spinnerEffect.Init(15); break; // S tier
        }

        dialogueSystem.gameObject.SetActive(true);
        DialogueSystem.StartConvo(0,0.5f);
    }
    void OnDestroy()
    {
        DialogueSystem.triggerEvent -= Sequence;
    }
}
