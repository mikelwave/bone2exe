using UnityEngine;

// Script for loading data at the start of the game
public class DataLoad : MonoBehaviour
{
    System.Random rnd = new System.Random();
    void AddStartForce(int childID,float speed)
    {
        Vector2 force = Vector2.one;
        if(rnd.Next(2) == 1) force.x = -1;
        if(rnd.Next(2) == 1) force.y = -1;
        transform.GetChild(childID).position = new Vector2(rnd.Next(-9,10),rnd.Next(-8,9));

        transform.GetChild(childID).GetComponent<Rigidbody2D>().AddForce(force*speed,ForceMode2D.Impulse);
    }
    void Start()
    {
        AddStartForce(0,150);
        AddStartForce(1,100);
        DataShare.soundsLoadedCallback = OnSoundsLoaded;
    }
    void OnSoundsLoaded()
    {
        DataShare.soundsLoadedCallback-=OnSoundsLoaded;
        StartCoroutine(ILoad());
    }
    System.Collections.IEnumerator ILoad()
    {
        yield return 0;
        DataShare.SetEncryptedKey();
        DataShare.LoadFromFile();
        yield return new WaitUntil(() => SaveLoadData.loadComplete);
        CutscenePlayer.LoadCutscene("intro","TitleScreen");
    }
}
