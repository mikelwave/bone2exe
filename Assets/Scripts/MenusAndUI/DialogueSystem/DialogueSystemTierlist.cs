using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueSystemTierlist : DialogueSystem
{   
    // Active character profile switching
    // 0 - none, 1 - left, 2 - right
    protected override void SetProfile(byte ID,bool instant)
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
        targetProfile = ID;
        if(ID == 0 && Event <= 1)
        {
            return;
        }
        profileLeft.UpdateFace(ID != 0,instant,ID == 2);
        profileRight.UpdateFace(ID == 1,instant,ID == 1);
    }

    protected override void Init()
    {
        BG = transform.GetChild(0).GetComponent<RectTransform>();
        nextArrow = transform.GetChild(5).gameObject;
        nextArrow.SetActive(false);

        TextMeshRect = transform.GetChild(4).GetComponent<RectTransform>();
        m_TextComponent = TextMeshRect.GetComponent<TextMeshProUGUI>();
        CharName = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        CharName.text = "";
        m_TextComponent.text = "";

        profileLeft.Init(transform.GetChild(1).GetComponent<Image>(),this,new Vector2(69,80));
        profileRight.Init(transform.GetChild(2).GetComponent<Image>(),this,new Vector2(-142,80));
        ResetEvent();

        PrepareText(TextFile);
    }
    protected override IEnumerator IMoveBG(bool show,bool startConvo,float delay)
    {
        if(delay > 0) yield return new WaitForSeconds(delay);
        if(show) BG.gameObject.SetActive(true);
        else yield break;

        if(show && startConvo)
        {
            profileLeft.UpdateFace(true,false,false);
            StartConvo();
        }
    }
}
