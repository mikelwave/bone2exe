using System.Collections.Generic;
using UnityEngine;

public class TextGenerate : MonoBehaviour
{
    public TextAsset TextFile;
    static List<string> Win1;
    static List<string> Win2;
    static List<string> AnyKey;
    static List<string> Crash;
    static System.Random rnd = new System.Random();
    // Start is called before the first frame update
    void Start()
    {
        Win1 = new List<string>();
        Win2 = new List<string>();
        AnyKey = new List<string>();
        Crash = new List<string>();

        // Text as string
        string[] textLines = TextFile.text.Split("\n"[0]);


        // Assign values
        int mode = -1; // 0 - Win1, 1 - Win2, 2 - Any key, 3 - Crash
        for(int i = 0;i<textLines.Length;i++)
        {
            // switch mode
            if(textLines[i].StartsWith("#"))
            {
                mode++;
                continue;
            }
            switch (mode)
            {
                default: // win 1
                Win1.Add(textLines[i].Substring(0,textLines[i].Length-1));
                break;
                case 1: // win 2
                Win2.Add(textLines[i]);
                break;
                case 2: // any key
                AnyKey.Add(textLines[i]);
                break;
                case 3: // crash text
                Crash.Add(textLines[i]);
                break;
            }
        }
    }
    public static string GetWin()
    {
        return (Win1[rnd.Next(0,Win1.Count)] + " " + Win2[rnd.Next(0,Win2.Count)]);
    }
    public static string GetAnyKey()
    {
        return (AnyKey[rnd.Next(0,AnyKey.Count)]);
    }
    public static string GetCrash()
    {
        if(Crash.Count==0) return "CRASH";
        return (Crash[rnd.Next(0,Crash.Count)]);
    }

}
