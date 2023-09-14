using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "OptionsGraphics", menuName = "ScriptableObjects/OptionsGraphics", order = 0)]
public class OptionsGraphics : ScriptableObject
{
    [Header ("Off index 0, On index 1+")]
    [Space]

    [Header ("Generic color")]
    [Space]
    [SerializeField] Color[] Colors = new Color[3];

    [Header ("Slider graphics")]
    [Space]
    [SerializeField] Sprite[] SliderKnob = new Sprite[3];
    [SerializeField] Sprite[] SliderBG = new Sprite[2];
    [Space]

    [Header ("Toggle graphics")]
    [Space]
    [SerializeField] Sprite[] ToggleCheckmark = new Sprite[2];
    [SerializeField] Sprite[] ToggleBG = new Sprite[3];

    [Space]
    [Header ("Arrow graphics")]
    [Space]
    [SerializeField] Sprite[] Arrow = new Sprite[3];
    public static OptionsGraphics self;

    [Space]
    [Header ("Small button graphics")]
    [Space]
    [SerializeField] Sprite[] SmallButton = new Sprite[4];

    [Space]
    [Header ("Big button graphics")]
    [Space]
    [SerializeField] Sprite[] BigButton = new Sprite[4];

    public void Init()
    {
        self = this;
    }
    // Generic color (0 = green, 1 = blue)
    public static Color GetColorOff(int colorType)
    {
        return self.Colors[colorType];
    }
    public static Color GetColorOn()
    {
        return self.Colors[2];
    }

    // Slider
    public static Sprite[] GetSliderKnobBoil()
    {
        return new Sprite[]{self.SliderKnob[1],self.SliderKnob[2]};
    }
    public static Sprite GetSliderKnobOff()
    {
        return self.SliderKnob[0];
    }
    public static Sprite GetSliderBG()
    {
        return self.SliderBG[1];
    }
    public static Sprite GetSliderBGOff()
    {
        return self.SliderBG[0];
    }

    // Toggle
    public static Sprite[] GetToggleBGBoil()
    {
        return new Sprite[]{self.ToggleBG[1],self.ToggleBG[2]};
    }
    public static Sprite GetToggleBGOff()
    {
        return self.ToggleBG[0];
    }
    public static Sprite GetToggleCheck(bool isOn)
    {
        return self.ToggleCheckmark[isOn ? 1 : 0];
    }

    // Arrow
    public static Sprite[] GetArrowBoil()
    {
        return new Sprite[]{self.Arrow[1],self.Arrow[2]};
    }
    public static Sprite GetArrowOff()
    {
        return self.Arrow[0];
    }

    // Small button (0 = green, 1 = blue)
    public static Sprite GetSmallButtonOff(int buttonType)
    {
        return self.SmallButton[buttonType];
    }
    public static Sprite[] GetSmallButtonBoil()
    {
        return new Sprite[]{self.SmallButton[2],self.SmallButton[3]};
    }

    // Big button
    public static Sprite GetBigButtonOff(int buttonType)
    {
        return self.BigButton[buttonType];
    }
    public static Sprite[] GetBigButtonBoil()
    {
        return new Sprite[]{self.BigButton[2],self.BigButton[3]};
    }
}
