using System.Collections;

// Game settings are stored here
[System.Serializable]
public class Setting<T>
{
    public string SettingName = "";
    T value;
    public Setting(T value, string SettingName)
    {
        this.SettingName = SettingName;
        this.value = value;
    }
    public T Get()
    {
        return value;
    }
    public void Set(T value)
    {
        this.value = value;
    }
}
[System.Serializable]
public class GameSettings
{
    public Setting<float> MusicVolume = new Setting<float>(1f,"MusicVolume"); // Music volume
    public Setting<bool> MusicToggle = new Setting<bool>(true,"MusicToggle"); // Music toggle
    public Setting<int> MusicType = new Setting<int>(0,"MusicType"); // Music type (0-1)
    public Setting<int> Soundfont = new Setting<int>(0,"Soundfont"); // Soundfont (0-?)
    public Setting<float> SFXVolume = new Setting<float>(1f,"SFXVolume"); // SFX volume
    public Setting<bool> SFXToggle = new Setting<bool>(true,"SFXToggle"); // SFX toggle
    public Setting<int> VSync = new Setting<int>(1,"VSync"); // VSync (0-2)
    public Setting<bool> Fullscreen = new Setting<bool>(false,"Fullscreen"); // Fullscreen
    public Setting<int> Resolution = new Setting<int>(0,"Resolution"); // Resolution (0-7)
    public Setting<int> DiagonalAim = new Setting<int>(0,"DiagonalAim"); // DiagonalAim (0-1)
    public Setting<bool> MovingBG = new Setting<bool>(true,"MovingBG"); // Moving BG (0-1)
    public Setting<bool> Screenshake = new Setting<bool>(true,"Screenshake"); // Screenshake (0-1)
    public Setting<bool> PixelScaling = new Setting<bool>(true,"PixelScaling"); // Pixel scaling (0-1)

    public static GameSettings self;
    public static ArrayList settingsList;
    
    public void Init()
    {
        self = this;
    }
    public void LoadArray()
    {
        settingsList = new ArrayList()
        {
            self.MusicVolume,
            self.MusicToggle,
            self.MusicType,
            self.Soundfont,
            self.SFXVolume,
            self.SFXToggle,
            self.VSync,
            self.Fullscreen,
            self.Resolution,
            self.DiagonalAim,
            self.MovingBG,
            self.Screenshake,
            self.PixelScaling
        };
    }

    //Int type
    public static Setting <int> GetSettingInt(string s)
    {
        for (int i = 0; i < settingsList.Count; i++)
        {
            if(settingsList[i] is Setting<int>)
            {
                Setting<int> setting = settingsList[i] as Setting<int>;
                if(setting.SettingName == s) return setting;
            }
        }
        return null;
    }
    public static void SetSettingInt(string s, int value)
    {
        for (int i = 0; i < settingsList.Count; i++)
        {
            if(settingsList[i] is Setting<int>)
            {
                Setting<int> setting = settingsList[i] as Setting<int>;
                if(setting.SettingName == s)
                {
                    setting.Set(value);
                    settingsList[i] = setting;
                    return;
                }
            }
        }
    }

    // Float type
    public static Setting <float> GetSettingFloat(string s)
    {
        for (int i = 0; i < settingsList.Count; i++)
        {
            if(settingsList[i] is Setting<float>)
            {
                Setting<float> setting = settingsList[i] as Setting<float>;
                if(setting.SettingName == s) return setting;
            }
        }
        return null;
    }
    public static void SetSettingFloat(string s, float value)
    {
        for (int i = 0; i < settingsList.Count; i++)
        {
            if(settingsList[i] is Setting<float>)
            {
                Setting<float> setting = settingsList[i] as Setting<float>;
                if(setting.SettingName == s)
                {
                    setting.Set(value);
                    settingsList[i] = setting;
                    return;
                }
            }
        }
    }

    // Bool type
    public static Setting <bool> GetSettingBool(string s)
    {
        for (int i = 0; i < settingsList.Count; i++)
        {
            if(settingsList[i] is Setting<bool>)
            {
                Setting<bool> setting = settingsList[i] as Setting<bool>;
                if(setting.SettingName == s) return setting;
            }
        }
        return null;
    }
    public static void SetSettingBool(string s, bool value)
    {
        for (int i = 0; i < settingsList.Count; i++)
        {
            if(settingsList[i] is Setting<bool>)
            {
                Setting<bool> setting = settingsList[i] as Setting<bool>;
                if(setting.SettingName == s)
                {
                    setting.Set(value);
                    settingsList[i] = setting;
                    return;
                }
            }
        }
    }
}
