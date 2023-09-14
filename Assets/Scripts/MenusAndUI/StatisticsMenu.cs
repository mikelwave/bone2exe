using System.Collections;
using UnityEngine;
using System;
using TMPro;

public class StatisticsMenu : Menu
{
    const float appearSpeed = 2.5f;
    Menu main; // Original menu
    RectTransform rectTransform;
    float boxWidth = 0;
    bool dirButtonUp = true;
    bool arrowControl = true;
    bool[] pages;
    bool altStats = false;
    int currentPage = 0;
    [SerializeField] OptionsGraphics optionsGraphics;
    [SerializeField] LevelSelectButton[] arrowButtons = new LevelSelectButton[2];
    Coroutine pageChange;

    Transform holder;
    RectTransform mainEntriesHolder;
    Transform totalsHolder;
    CanvasGroup mainEntriesHolderCanvasGroup;

    // Entries
    StatisticEntry[] statisticEntries;

    // World header
    TextMeshProUGUI worldHeader;

    // Switch between any and 100 %
    delegate void StatTypeSwitch(byte statsID);
    StatTypeSwitch statTypeSwitch;

    string GetWorldName()
    {
        if(currentPage<DataShare.worldAmount) return "World "+(currentPage+1);
        else if(currentPage>=DataShare.worldAmount && currentPage < DataShare.worldAmount*2) return "World " + (char)(60+currentPage+1);
        else return "Secret " + (currentPage == pages.Length-1 ? "Dark" : "Light");
    }
    void StatSwitchAnimate()
    {
        altStats = !altStats;
        statTypeSwitch?.Invoke((byte)(altStats ? 1 : 0));
    }

    // Totals
    void AssignTotals()
    {
        // Specials collected
        totalsHolder.GetChild(0).GetComponent<TextMeshProUGUI>().text
        = "<size=50><sprite=5></size> "+ DataShare.GetSpecialCollectCount()+"/"+DataShare.specialsMax;

        // Total deaths
        totalsHolder.GetChild(1).GetComponent<TextMeshProUGUI>().text
        = "Deaths: "+DataShare.totalDeaths.ToString("000");

        // Total time
        TextMeshProUGUI bestTimeText = totalsHolder.GetChild(2).GetComponent<TextMeshProUGUI>();
        if(DataShare.hadRun && DataShare.totalGameTime > 0)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(DataShare.totalGameTime);
            string formattedTime = string.Format(@"{0:h\:mm\:ss\.ff}", timeSpan);

            bestTimeText.text = "Best run:"+'\n'+formattedTime;
        }
        else bestTimeText.text = "Best run:"+'\n'+"--:--.--";
    }
    void AssignDisplays()
    {
        worldHeader = holder.GetChild(2).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        mainEntriesHolder = holder.GetChild(3).GetComponent<RectTransform>();
        mainEntriesHolderCanvasGroup = mainEntriesHolder.GetComponent<CanvasGroup>();
        statisticEntries = mainEntriesHolder.GetChild(0).GetComponentsInChildren<StatisticEntry>();
    }

    IEnumerator IPageChange(int dir)
    {
        currentPage = (int)Mathf.Repeat(currentPage+dir,pages.Length);
        while(!pages[currentPage])
        {
            currentPage = (int)Mathf.Repeat(currentPage+dir,pages.Length);
        }

        DataShare.PlaySound("LS_ZoomOut",false,0.1f,1);
        float targetX = -100 * dir;
        Vector3 pos = mainEntriesHolder.anchoredPosition;
        float progress = 0;

        while(progress < 1)
        {
            progress += Time.deltaTime*8;
            pos.x = Mathf.Lerp(0,targetX,Mathf.SmoothStep(0,1,progress));
            mainEntriesHolderCanvasGroup.alpha = Mathf.Lerp(1,0,progress);
            mainEntriesHolder.anchoredPosition = pos;
            yield return 0;
        }
        progress = 0;
        GetPageDisplay();
        while(progress < 1)
        {
            progress += Time.deltaTime*8;
            pos.x = Mathf.Lerp(-targetX,0,Mathf.SmoothStep(0,1,progress));
            mainEntriesHolderCanvasGroup.alpha = Mathf.Lerp(0,1,progress);
            mainEntriesHolder.anchoredPosition = pos;
            yield return 0;
        }
        mainEntriesHolderCanvasGroup.alpha = 1;
        pos.x = 0;
        mainEntriesHolder.anchoredPosition = pos;

        pageChange = null;
    }
    IEnumerator ITurnOff()
    {
        Vector2 sizeDelta = rectTransform.sizeDelta;
        float progress = 0;
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime*appearSpeed;
            sizeDelta.x = Mathf.Lerp(boxWidth,0,Mathf.SmoothStep(0,1,progress));
            rectTransform.sizeDelta = sizeDelta;
            yield return 0;
        }
        Menu.self = main;
        ToggleButtons(true);
        main.OverwriteBoilAnimationUI();
        main.RestoreControl();
        Destroy(gameObject);
    }
    IEnumerator ITurnOn()
    {
        Vector2 sizeDelta = rectTransform.sizeDelta;
        sizeDelta.x = 0;
        rectTransform.sizeDelta = sizeDelta;

        float progress = 0;
        int maxCounter = 10;
        while(progress<1)
        {
            progress += Time.unscaledDeltaTime*appearSpeed;
            sizeDelta.x = Mathf.Lerp(0,boxWidth,Mathf.SmoothStep(0,1,progress));
            rectTransform.sizeDelta = sizeDelta;
            yield return 0;
            if(maxCounter>0)
            {
                maxCounter--;
                if(maxCounter==0)
                {
                    mainEntriesHolder.gameObject.SetActive(true);
                    totalsHolder.gameObject.SetActive(true);
                    worldHeader.gameObject.SetActive(true);
                }
            }
        }
        mainEntriesHolder.gameObject.SetActive(true);
        totalsHolder.gameObject.SetActive(true);
        worldHeader.gameObject.SetActive(true);
        sizeDelta.x = boxWidth;
        rectTransform.sizeDelta = sizeDelta;
        OpenMenu();
    }

    void GetPageDisplay()
    {
        // Clear delegate
        statTypeSwitch = null;
        worldHeader.text = GetWorldName();

        int levelLength = Mathf.Min(DataShare.GetLevelCount(currentPage));
        bool oneExtra = true;

        // Get world name
        
        for (int i = 0; i < statisticEntries.Length; i++)
        {
            if(i>=levelLength)
            {
                statisticEntries[i].MakeBlank();
                continue;
            }
            LevelStats[] stats = DataShare.GetLevelStats(currentPage,i);
            statisticEntries[i].SetLevelStats(stats,currentPage,i);

            // Check which stats to display, 0 = any%, 1 = 100%
            byte whichPresent = (byte)((stats[0].LevelTime != 0 ? 1 : 0) + (stats[1].LevelTime != 0 ? 2 : 0 ));
            if(whichPresent == 0 && DataShare.GetLevelValue(currentPage,i) == 0)
            {
                if(currentPage < DataShare.worldAmount && DataShare.GetLevelValue(currentPage+DataShare.worldAmount,i) > 0)
                {
                    oneExtra = true;
                }
                else if(oneExtra)
                {
                    oneExtra = false;
                }
                else
                {
                    statisticEntries[i].MakeUnknown();
                    continue;
                }
            }
            int statsID = 0;
            if(whichPresent == 3) // Both present, display on a cycle
            {
                statTypeSwitch += statisticEntries[i].UpdateLevelStats;
            }
            else statsID = Mathf.Clamp(whichPresent - 1,0,1); // Any or All present only

            statisticEntries[i].UpdateLevelStats((byte)statsID);
        }   
    }

    void OnEnable()
    {
        main = Menu.self;

        // Turn off main menu buttons
        ToggleButtons(false);
        Menu.self = this;

        if(OptionsGraphics.self == null) optionsGraphics.Init();
        holder = transform.GetChild(0);
        totalsHolder = holder.GetChild(1);
        rectTransform = holder.GetComponent<RectTransform>();
        boxWidth = rectTransform.sizeDelta.x;
        pages = DataShare.GetWhichWorldsUnlocked();
        pages[0] = true;
        CheckIfHideArrows();
        AssignTotals();
        AssignDisplays();
        GetPageDisplay();

        mainEntriesHolder.gameObject.SetActive(false);
        totalsHolder.gameObject.SetActive(false);
        worldHeader.gameObject.SetActive(false);

        StartCoroutine(ITurnOn());
        InvokeRepeating("StatSwitchAnimate",1,1);
        
    }

    void CheckIfHideArrows()
    {
        byte count = 0;
        for (int i = 0; i < pages.Length; i++)
        {
            if(pages[i]) count++;
        }
        if(count<=1)
        {
            arrowControl = false;
            arrowButtons[0].gameObject.SetActive(false);
            arrowButtons[1].gameObject.SetActive(false);
        }
    }

    protected override void UpdateControl()
    {
        base.UpdateControl();

        if(arrowControl)
        {
            int dir = MGInput.GetDpadXRaw(MGInput.controls.Player.Movement);

            if(dirButtonUp && dir!=0)
            {
                dirButtonUp = false;
                PageSelectNavigation();
            }
            else if(!dirButtonUp && dir == 0)
            {
                dirButtonUp = true;
            }
        }
    }

    public void Back()
    {
        ///print("Back");
        if(pageChange != null) StopCoroutine(pageChange);
        LocalToggleButtons(false);
        StartCoroutine(ITurnOff());
    }

    void PageSelectNavigation()
    {
        int dir = Mathf.RoundToInt(MGInput.GetDpadX(MGInput.controls.Player.Movement));
        if(dir!=0) PageSwitchKeyboard(dir);
    }
    public void PageSwitchKeyboard(int dir)
    {
        if(pageChange!=null) return;
        PageSwitch(dir);
        arrowButtons[(dir+1)/2].FakeOnClick();
    }

    // Called by arrow click or by key press
    public void PageSwitch(int dir)
    {
        if(pageChange!=null) return;

        pageChange = StartCoroutine(IPageChange(dir));
    }
}
