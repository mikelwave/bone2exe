using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#region classes
class MrMixKey
{
    TextMeshPro textMesh;
    TargetHit targetHit;
    Animator anim;
    public Transform keyTransform;
    public float spinOffset = 0;
    public MrMixKey(Transform keyTransform)
    {
        this.keyTransform = keyTransform;
        textMesh = keyTransform.GetChild(0).GetComponent<TextMeshPro>();
        targetHit = keyTransform.GetComponent<TargetHit>();
        anim = keyTransform.GetComponent<Animator>();
    }
    public void Despawn()
    {
        if(keyTransform.gameObject.activeInHierarchy)
        anim.SetTrigger("Despawn");
    }
    public void Spawn()
    {
        if(keyTransform.gameObject.activeInHierarchy)
        {
            anim.Rebind();
            anim.Update(0f);
            anim.SetTrigger("Spawn");
        }
    }
    public void SetText(char c)
    {
        keyTransform.name = c.ToString();
        textMesh.text = c.ToString();
    }
    public void ToggleKeyHittable(bool hittable)
    {
        targetHit.Activated = !hittable;
    }

}
class WordList
{
    public List<string> words;
    public WordList()
    {
        words = new List<string>();
    }
    public void CheckWords()
    {
        string str = "";
        foreach(string s in words)
        {
            str += s + '\n';
        }
        Debug.Log(str);
    }
}
#endregion
public class MrMixKeys : MonoBehaviour
{
    #region vars
    public bool Active = false;
    [SerializeField] TextAsset words;
    [SerializeField] byte keyStoreAmount = 11;
    [SerializeField] byte wordIndex = 0;
    [SerializeField] Sprite[] wordIcons;
    public byte phaseIndex = 0;
    [SerializeField] float maxCircleRadius = 5;
    [SerializeField] float spinSpeed = 1;
    float angle = 0;
    float currentCircleRadius = 0;
    string wordToType = "";
    byte charIndexToType = 0;

    MrMixKey[] keys;
    byte activeKeys = 0;
    List <WordList> wordLists;
    Transform TypeThis;
    TextMeshPro TypeProgress;
    SpriteRenderer iconRenderer;

    public delegate void WordCompleteEvent();
    public WordCompleteEvent wordCompleteEvent;
    delegate void UpdateEvent();
    UpdateEvent updateEvent;
    #endregion

    #region IEnumerators
    void WordKeysAppear(bool appear)
    {
        if(wordAppearCor!=null) StopCoroutine(wordAppearCor);
        wordAppearCor = StartCoroutine(IWordAppearEffect(appear));
    }
    Coroutine wordAppearCor;
    IEnumerator IWordAppearEffect(bool appear)
    {
        if(appear)
        {
            updateEvent = SpinKeys;
            Active = true;
            foreach(MrMixKey k in keys)
            k.Spawn();
            DataShare.PlaySound("MrMix_KeysAppear",transform.position,false);
        }
        else
        {
            Active = false;
            yield return 0;
            foreach(MrMixKey k in keys)
            k.Despawn();
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
            TypeThis.gameObject.SetActive(false);

            foreach(MrMixKey k in keys)
            k.keyTransform.gameObject.SetActive(false);
            updateEvent = null;
        }
    }
    #endregion
    #region functions

    void SpinKeys()
    {
        angle = Mathf.Repeat(angle+Time.deltaTime*spinSpeed,1);
        for(int i = 0; i<activeKeys;i++)
        {
            float localAngle = Mathf.Repeat(angle+keys[i].spinOffset,1) * Mathf.PI * 2;

            Vector2 pos;
            pos.x = Mathf.Sin(localAngle) * currentCircleRadius;
            pos.y = Mathf.Cos(localAngle) * currentCircleRadius;

            keys[i].keyTransform.localPosition = pos;
        }
    }

    void GetWords()
    {
        // Split text into lines
        string TextAsString = words.text;
        List<string> TextAsLines = new List<string>();
        TextAsLines.AddRange(TextAsString.Split("\n"[0]));

        // Assign each line to a list
        wordLists = new List<WordList>();
        WordList tempList = new WordList();

        foreach(string s in TextAsLines)
        {
            if(s.StartsWith('#')) // Prepare new list and assign current one if exists
            {
                if(tempList.words.Count != 0)
                {
                    wordLists.Add(tempList);
                    tempList = new WordList();
                }
            }
            else tempList.words.Add(s.Trim((char)13).ToUpper());
        }
        if(tempList.words.Count != 0)
        {
            wordLists.Add(tempList);
        }

        ///foreach(WordList w in wordLists) w.CheckWords();
    }
    void GenerateKeys()
    {
        keys = new MrMixKey[keyStoreAmount];
        GameObject sample = transform.GetChild(0).gameObject;
        MrMixKey sampleKey = new MrMixKey(sample.transform);
        sample.GetComponent<MrMixKeyData>().main = this;
        keys[0] = sampleKey;
        sample.SetActive(false);
        for(int i = 1; i<keyStoreAmount;i++)
        {
            Transform newKey = Instantiate(sample,transform.position,Quaternion.identity).transform;
            newKey.SetParent(transform);
            sampleKey = new MrMixKey(newKey);
            newKey.GetComponent<MrMixKeyData>().main = this;
            keys[i] = sampleKey;
            newKey.gameObject.SetActive(false);
        }

    }
    
    public bool ShowWord()
    {
        activeKeys = 0;
        charIndexToType = 0;
        List <char> buttonChars = new List<char>();
        if(wordIndex>=wordLists[phaseIndex].words.Count)
        {
            wordIndex = 0;
            return true;
        }

        string s = wordLists[phaseIndex].words[wordIndex];
        wordToType = s;
        for(int i = 0;i<s.Length;i++)
        {
            if(!buttonChars.Contains(s[i])) buttonChars.Add(s[i]);
        }
        // Randomize array
        for(int i = 0; i<buttonChars.Count; i++)
        {
            int oldIndex = i;
            int newIndex = Random.Range(0,buttonChars.Count);
            char temp = buttonChars[oldIndex];

            buttonChars[oldIndex] = buttonChars[newIndex];
            buttonChars[newIndex] = temp;
        }
        // Use char array for buttons
        ///string str = "";
        for(int i = 0; i<buttonChars.Count; i++)
        {
            ///str += "\"" + (byte)buttonChars[i] + "\"";
            keys[i].SetText(buttonChars[i]);
            keys[i].keyTransform.gameObject.SetActive(true);
        }
        ///print (str);
        activeKeys = (byte) buttonChars.Count;

        // Set the spin offsets for keys
        float spinDelta = 1/(float)activeKeys;
        for(int i = 0;i < buttonChars.Count; i++)
        {
            keys[i].spinOffset = i*spinDelta;
        }
        // Show word to type
        ShowWordProgress();
        iconRenderer.sprite = wordIcons[(int)(wordIndex+1)+(int)(phaseIndex)*3];
        TypeThis.gameObject.SetActive(true);

        WordKeysAppear(true);
        ToggleKeysHittable(true);

        // Increase the word index
        wordIndex = (byte) Mathf.Repeat(wordIndex+=1,wordLists[phaseIndex].words.Count+1);
        return false;
    }
    void ToggleKeysHittable(bool hittable)
    {
        foreach(MrMixKey k in keys)
        {
            k.ToggleKeyHittable(hittable);       
        }
    }
    public void KeyPress(char c,Vector3 pos)
    {
        if(!Active || charIndexToType >= wordToType.Length) return;
        // Compare the pressed key with the required char at current index
        if(wordToType[(int)charIndexToType] == c)
        {
            charIndexToType++;
            ShowWordProgress();
            DataShare.PlaySound("MrMix_KeyPress",pos,false,1,
            Mathf.Lerp(1,1.5f,
            ((float)(wordToType.Length) - (float)(wordToType.Length-charIndexToType)) / (float)(wordToType.Length)));
            if(charIndexToType >= wordToType.Length)
            {
                wordCompleteEvent?.Invoke();
                WordKeysAppear(false);
                ToggleKeysHittable(false);
                
            }
        }
        else // Didn't match the index, reset
        {
            DataShare.PlaySound("MrMix_KeyPressFail",pos,false,1,1);
            ResetWord();
        }
    }
    public void ResetWord()
    {
        if(!Active) return;
        charIndexToType = 0;
        ///print("Failed to type correct char.");
        ShowWordProgress();
    }
    void ShowWordProgress()
    {
        TypeProgress.text = "";
        // red color
        string s = "<color=#FF0000>";

        for(int i = 0;i<wordToType.Length;i++)
        {
            if(i == charIndexToType)
            {
                s += "</color>";
            }
            s += wordToType[i];
        }
        TypeProgress.text = s;
    }
    // Start is called before the first frame update
    void Start()
    {
        TypeThis = transform.GetChild(1);
        TypeProgress = TypeThis.GetChild(0).GetComponent<TextMeshPro>();
        iconRenderer = TypeThis.GetChild(1).GetComponent<SpriteRenderer>();
        GetWords();
        GenerateKeys();
    }

    // Update is called once per frame
    void Update()
    {
        updateEvent?.Invoke();
    }
    #endregion
}
