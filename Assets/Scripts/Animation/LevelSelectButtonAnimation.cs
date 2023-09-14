using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(BoilAnimationUI))]
public class LevelSelectButtonAnimation : LevelSelectButton
{
    public Image image;
    BoilAnimationUI boilAnimationUI;

    [SerializeField]
    Sprite offImage;
    // Start is called before the first frame update
    protected override void Start()
    {
        if(image==null) image = GetComponent<Image>();
        boilAnimationUI = GetComponent<BoilAnimationUI>();
        boilAnimationUI.enabled = false;

        mouseHoverEvent = SetBoil;
        highlightEvent = SetTransparency;
        
        base.Start();
        
        Init();
    }
    // Called as events by level select button scripts
    void SetTransparency(float from, float to, float progress)
    {
        Color c = image.color;
        c.a = Mathf.Lerp(from,to,progress);
        image.color = c;
    }
    void SetBoil(bool toggle)
    {
        boilAnimationUI.enabled = toggle;
        if(!toggle)image.sprite = offImage;
    }
}
