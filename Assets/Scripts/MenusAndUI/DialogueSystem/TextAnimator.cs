using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TagEffect
{
    public int endIndex = -1;
    public string tag = "";

    public TagEffect(int endIndex, string tag)
    {
        this.endIndex = endIndex;
        this.tag = tag;
    }
    public TagEffect()
    {
        endIndex = -1;
        tag = "";
    }
}
public class TextAnimator : MonoBehaviour
{
    string[] customTags = {"normal","smooth"};

    [Header("Text animator")]
    [Space]

    public List <Vector2Int> normalTextRanges;
    public List <Vector2Int> smoothTextRanges;
    public Vector2Int sineTextRange;
    protected TMP_Text m_TextComponent;

    protected virtual void KillText(){}
    protected virtual void ShowText(bool delay){}

    public TagEffect findTagEffect(int index)
    {
        TagEffect t = new TagEffect();

        // Normal
        for(int i = 0; i< normalTextRanges.Count;i++)
        {
            if(Mathf.Clamp(index,normalTextRanges[i].x,normalTextRanges[i].y)==index)
            {
                t.endIndex = normalTextRanges[i].y;
                t.tag = customTags[0];
                return t;
            }
        }

        // Smooth
        for(int i = 0; i< smoothTextRanges.Count;i++)
        {
            if(Mathf.Clamp(index,smoothTextRanges[i].x,smoothTextRanges[i].y)==index)
            {
                t.endIndex = smoothTextRanges[i].y;
                t.tag = customTags[1];
                return t;
            }
        }

        return t;
    }

    bool isEligibleTag(string tag)
    {
        ///print("Checking tag: "+tag);
        for(int i = 0;i<customTags.Length;i++)
        {
            if(tag.ToLower()==customTags[i])
            return true;
        }
        return false;
    }
    void ApplyTagEffect(string tag, int startRange, int EndRange)
    {
        switch(tag)
        {
            default: break;

            case "normal":
            normalTextRanges.Add(new Vector2Int(startRange,EndRange));
            break;

            case "smooth":
            smoothTextRanges.Add(new Vector2Int(startRange,EndRange));
            break;

            case "sine":
            sineTextRange = new Vector2Int(startRange,EndRange);
            break;
        }
    }

    public void ScanText(bool delay)
    {
        // Reset values
        normalTextRanges.Clear();
        smoothTextRanges.Clear();
        sineTextRange = new Vector2Int(-1,-1);

        string text = m_TextComponent.text;
        string displayText = "";

        // Split the whole text into parts based off the <> tags 
        // Even numbers in the array are text, odd numbers are tags
        int textLength = 0;

        string currentTag = "";
        int tagEffectStart = 0;
        int tagEffectEnd = 0;
        int normalEffectStart = 0;
        int normalEffectEnd = -1;
        if(text=="")
        {
            KillText();
            return;
        }
        string[] subTexts = text.Split('<', '>');

        ///foreach(string t in subTexts)
        ///print(t);

        for(int i = 0;i<subTexts.Length;i++)
        {
            // Normal text
            if (i % 2 == 0)
            {
                if(subTexts[i].Contains("~"))
                {
                    ///Debug.Log("Pre: "+subTexts[i]);
                    for(int j = 0;j<subTexts[i].Length;j++)
                    {
                        if(subTexts[i][j]=='~')
                        {
                            if(sineTextRange.x==-1)
                            sineTextRange.x = textLength+j;

                            else sineTextRange.y = textLength+j;
                        }
                    }
                    subTexts[i] = subTexts[i].Replace("~","");
                    ///Debug.Log("Post: "+subTexts[i]+" Text range: "+sineTextRange);
                }
                displayText += subTexts[i];
                textLength+=subTexts[i].Length;
            }
            else
            {
                // Tag ender
                if(subTexts[i].StartsWith("/"))
                {
                    // Non-custom tag
                    if (!isEligibleTag(subTexts[i].Trim('/')))
                    {
                        displayText += $"<{subTexts[i]}>";
                    }
                    else
                    {
                        if(subTexts[i].Trim('/').ToLower()==currentTag)
                        {
                            ///print("Tag ender: "+subTexts[i]);
                            tagEffectEnd = textLength-1;
                            ApplyTagEffect(currentTag,tagEffectStart,tagEffectEnd);

                            // Reset values
                            currentTag = "";
                            tagEffectStart = 0;
                            tagEffectEnd = 0;
                            normalEffectStart = textLength+1;
                        }
                    }
                }
                else if (!isEligibleTag(subTexts[i].Replace(" ", "")))
                {
                    displayText += $"<{subTexts[i]}>";
                }

                else // Valid tag
                {
                    // Not currently inside a tag
                    if(currentTag=="")
                    {
                        currentTag = subTexts[i].ToLower();

                        normalEffectEnd = textLength-1;
                        if(normalEffectStart!=-1)
                        {
                            ApplyTagEffect("smooth",normalEffectStart,normalEffectEnd);
                            normalEffectStart = -1;
                            normalEffectEnd = 0;
                        }
                        tagEffectStart = textLength;
                        ///print("Tag: " + currentTag);
                    }
                }
            }
            if(i==subTexts.Length-1 && normalEffectStart != -1)
            {
                ApplyTagEffect("smooth",normalEffectStart,textLength-1);
                normalEffectStart = -1;
            }
        }

        m_TextComponent.text = displayText;
        ShowText(delay);
    }
}
