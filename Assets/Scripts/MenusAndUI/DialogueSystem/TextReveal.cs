using System.Collections;
using UnityEngine;
using TMPro;

public class TextReveal : TextAnimator
{

    [Space]
    [Header("Text reveal")]
    [Space]
    [SerializeField] float charDelay = 1.0f;
    [SerializeField] int RolloverCharacterSpread = 10;
    [SerializeField] float verticalOffset = -2;
    [SerializeField] float sineHeight = 1;
    [SerializeField] float sineSpeed = 1;
    [SerializeField] float sineCharDelay = 0.1f;
    protected string CharVoiceSound = "";

    protected bool isRangeMax = false;
    protected bool textDisplaying = false;
    protected bool skipping = false;

    void SetStartAlpha(int characterCount, TMP_TextInfo textInfo)
    {
        Color32[] newVertexColors;
        Vector3[] newVertices;
        float offset = verticalOffset*2;
        // Used to determine what effect to have on current part of text
        TagEffect curTagEffect = new TagEffect();

        // Set alpha of all chars to 0 on start
        for(int i = 0;i<characterCount;i++)
        {
            // Get char info
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // Skip hidden chars
            if(!charInfo.isVisible) continue;

            // if currentTagMode is blank, find out what range the current tag is part of
            if(curTagEffect.endIndex < i)
            {
                curTagEffect = findTagEffect(i);
                if(curTagEffect.tag == "")
                {
                    curTagEffect.tag = "normal";
                    print("Warning: no tag");
                }
                // /print("Current tag mode: "+curTagEffect.tag+", ends at: "+curTagEffect.endIndex);
            }
            switch(curTagEffect.tag)
            {
                default:
                // Get current char material index
                int materialIndex = charInfo.materialReferenceIndex;

                // Get vertex colors of mesh used by char
                newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                // Get index of first vertex used by char
                int vertexIndex = charInfo.vertexIndex;

                // Set new alpha value
                newVertexColors[vertexIndex+0].a = 0;
                newVertexColors[vertexIndex+1].a = 0;
                newVertexColors[vertexIndex+2].a = 0;
                newVertexColors[vertexIndex+3].a = 0;
                break;

                case "smooth":
                // Get current char material index
                materialIndex = charInfo.materialReferenceIndex;

                // Get vertex colors of mesh used by char
                newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                // Get index of first vertex used by char
                vertexIndex = charInfo.vertexIndex;

                // Set new alpha value
                newVertexColors[vertexIndex+0].a = 0;
                newVertexColors[vertexIndex+1].a = 0;
                newVertexColors[vertexIndex+2].a = 0;
                newVertexColors[vertexIndex+3].a = 0;

                // Get vertices
                newVertices = textInfo.meshInfo[materialIndex].vertices;

                // Set new vertex offsets
                newVertices[vertexIndex+0].y -= offset;
                newVertices[vertexIndex+1].y -= offset;
                newVertices[vertexIndex+2].y -= offset;
                newVertices[vertexIndex+3].y -= offset;
                break;
            }
        }
        m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32 | TMP_VertexDataUpdateFlags.Vertices);
    }
    Coroutine ShowTextCor;
    IEnumerator IShowText(bool delay)
    {
        m_TextComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = m_TextComponent.textInfo;
        if(delay) yield return 0;
        Color32[] newVertexColors;
        Vector3[] newVertices;
        Vector3[][] oldVertices = null;

        int currentCharacter = 0;
        int startingCharacterRange = currentCharacter;
        isRangeMax = false;

        int characterCount = textInfo.characterCount;

        // Used to determine what effect to have on current part of text
        TagEffect curTagEffect = new TagEffect();
        m_TextComponent.color = new Color(1,1,1,0);

        yield return 0;
        ///m_TextComponent.color = new Color(1,1,1,1);
        SetStartAlpha(characterCount,textInfo);
        bool sineText = sineTextRange!=Vector2Int.one*-1;

        // Declare reference to sinewave movement vertices
        if(sineText)
        {
           oldVertices = new Vector3[sineTextRange.y-sineTextRange.x][];
           ///print("Declaring vertices copy");
            for(int i = 0;i<oldVertices.Length;i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[sineTextRange.x+i];

                // Skip hidden chars
                if(!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                Vector3[] vertices = new Vector3[]{
                    textInfo.meshInfo[materialIndex].vertices[vertexIndex],
                    textInfo.meshInfo[materialIndex].vertices[vertexIndex+1],
                    textInfo.meshInfo[materialIndex].vertices[vertexIndex+2],
                    textInfo.meshInfo[materialIndex].vertices[vertexIndex+3]
                };
                
                oldVertices[i] = vertices;
            }
        }
        textDisplaying = true;
        float charDelayWait = 0;
        while(!isRangeMax || sineText)
        {
            float verticalHalfOffset = verticalOffset/2;
            // Spread should not exceed the number of characters.
            byte fadeSteps = (byte)((Mathf.Max(1,255 / RolloverCharacterSpread)));
            byte alpha = 0;

            for(int i = startingCharacterRange; i < currentCharacter; i++)
            {
                if(!isRangeMax)
                {
                    // Get char info
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                    // Skip hidden chars
                    if(!charInfo.isVisible) continue;

                    // If currentTagMode is blank, find out what range the current tag is part of
                    if(curTagEffect.endIndex < i)
                    {
                        curTagEffect = findTagEffect(i);
                        if(curTagEffect.tag == "")
                        {
                            // if(curTagEffect.endIndex==-1) break;
                            curTagEffect.tag = "normal";
                            ///print("Warning: no tag");
                        }
                        ///print("Current tag mode: "+curTagEffect.tag+", ends at: "+curTagEffect.endIndex);
                    }
                    alpha = 0;
                    switch(curTagEffect.tag)
                    {
                        default:
                        // Get current char material index
                        int materialIndex = charInfo.materialReferenceIndex;

                        // Get vertex colors of mesh used by char
                        newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                        // Get index of first vertex used by char
                        int vertexIndex = charInfo.vertexIndex;

                        // Get the current char's alpha value
                        alpha = 255;

                        // Set new alpha value
                        newVertexColors[vertexIndex+0].a = alpha;
                        newVertexColors[vertexIndex+1].a = alpha;
                        newVertexColors[vertexIndex+2].a = alpha;
                        newVertexColors[vertexIndex+3].a = alpha;
                        break;

                        case "smooth":
                        ///print("sine text: "+sineText);

                        // Get current char material index
                        materialIndex = charInfo.materialReferenceIndex;

                        // Get vertex colors of mesh used by char
                        newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                        // Get index of first vertex used by char
                        vertexIndex = charInfo.vertexIndex;

                        // Get the current char's alpha value
                        alpha = (byte)Mathf.Clamp(newVertexColors[vertexIndex + 0].a + fadeSteps/2, 0, 255);

                        // Set new alpha value
                        newVertexColors[vertexIndex+0].a = alpha;
                        newVertexColors[vertexIndex+1].a = alpha;
                        newVertexColors[vertexIndex+2].a = alpha;
                        newVertexColors[vertexIndex+3].a = alpha;

                        // Get vertices
                        newVertices = textInfo.meshInfo[materialIndex].vertices;

                        // Get the current char's vertical offset
                        float offset = Mathf.Clamp(1-(((float)alpha)/255),0,1);

                        // Set new vertex offsets
                        if(!sineText || (Mathf.Clamp(i,sineTextRange.x,sineTextRange.y-2) != i))
                        {
                            newVertices[vertexIndex+0].y += offset*verticalHalfOffset;
                            newVertices[vertexIndex+1].y += offset*verticalHalfOffset;
                            newVertices[vertexIndex+2].y += offset*verticalHalfOffset;
                            newVertices[vertexIndex+3].y += offset*verticalHalfOffset;
                        }

                        // Tint vertex colors
                        /*
                        newVertexColors[vertexIndex+0] = (Color)newVertexColors[vertexIndex+0] * ColorTint;
                        newVertexColors[vertexIndex+1] = (Color)newVertexColors[vertexIndex+1] * ColorTint;
                        newVertexColors[vertexIndex+2] = (Color)newVertexColors[vertexIndex+2] * ColorTint;
                        newVertexColors[vertexIndex+3] = (Color)newVertexColors[vertexIndex+3] * ColorTint;*/
                        break;
                    }
                    /*
                    if(alpha==1)
                    {
                        startingCharacterRange++;
                        print("Starting: "+startingCharacterRange+" Count: "+characterCount);
                        if(startingCharacterRange == characterCount)
                        {
                            // Update mesh vertext data last time
                            m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32 | TMP_VertexDataUpdateFlags.Vertices);
                            // Reset text object back to original state.
                            m_TextComponent.ForceMeshUpdate();
                            isRangeMax = true; // ends coroutine
                            yield break;
                        }
                    }*/
                }
            }
            if(sineText)
            {
                Vector2Int range = sineTextRange;
                for(int i = range.x; i<range.y-1; i++)
                {
                    // Get char info
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                    // Skip hidden chars
                    if(!charInfo.isVisible) continue;

                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;
                    int offsetIndex = i-range.x;

                    ///print("Length: "+newVertices.Length);

                    // Get the current char's vertical offset
                    float offset = GetSineLevel(i);
                    

                    // Get vertices
                    newVertices = textInfo.meshInfo[materialIndex].vertices;

                    oldVertices[offsetIndex].CopyTo(newVertices,vertexIndex);

                    float calc = verticalOffset*2+offset;
                    // Set new vertex offsets
                    newVertices[vertexIndex+0].y += calc;
                    newVertices[vertexIndex+1].y += calc;
                    newVertices[vertexIndex+2].y += calc;
                    newVertices[vertexIndex+3].y += calc;

                    ///textInfo.meshInfo[materialIndex].vertices = newVertices;


                    ///print(oldVertices[i-range.x][vertexIndex]+" "+newVertices[vertexIndex]+" "+ textInfo.meshInfo[materialIndex].vertices[vertexIndex]);
                }
                if(skipping && isRangeMax)
                {
                    yield return 0;
                }
            }
            // Update changed vertex colors
            m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32 | TMP_VertexDataUpdateFlags.Vertices);

            // print("Alpha: "+alpha+" Current char: "+currentCharacter+" char count: "+characterCount+" isRangeMax: "+isRangeMax);
            if(!isRangeMax && currentCharacter+1==characterCount && (alpha == 255 || alpha == 0 && currentCharacter+1 == characterCount))
            {
                isRangeMax = true;
                textDisplaying = false;
            }
            if(!skipping)
            {
                if(currentCharacter == 0 && characterCount>3 && charDelayWait == 0)
                {
                    DataShare.PlaySound(CharVoiceSound,false,0.1f,1);
                }
                charDelayWait+=Time.deltaTime;
                if(charDelayWait>=charDelay)
                {
                    if(currentCharacter+1<characterCount)
                    {
                        currentCharacter++;
                        if(CharVoiceSound!="" && (byte)m_TextComponent.text[currentCharacter] > 32)
                        {
                            DataShare.PlaySound(CharVoiceSound,false,0.1f,1);
                        }
                    }
                    charDelayWait = 0;
                }
                yield return 0;
            }
            else
            {
                if(currentCharacter+1<characterCount)
                {
                    currentCharacter++;
                }
            }
        }
    }
    float GetSineLevel(int offsetIndex)
    {
        float sineOffsetTime = Mathf.Repeat(Time.timeSinceLevelLoad*sineSpeed,Mathf.PI*2);
        return Mathf.Sin(sineOffsetTime-sineCharDelay*offsetIndex)*sineHeight;
    }
    protected override void KillText()
    {
        isRangeMax = false;
        
        if(ShowTextCor!=null)StopCoroutine(ShowTextCor);
        m_TextComponent.enabled = false;

    }
    protected override void ShowText(bool delay)
    {
        if(ShowTextCor!=null)StopCoroutine(ShowTextCor);
        m_TextComponent.enabled = true;
        ShowTextCor = StartCoroutine(IShowText(delay));

    }
}
