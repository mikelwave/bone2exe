using UnityEngine;
using TMPro;

public class LevelIntroText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Assign text
        string text = GameMaster.GetLevelTitle();
        ///print("Level name: "+text);
        if(text == "") Destroy(gameObject);

        transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        // Assign camera
        GetComponent<Canvas>().worldCamera = GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetComponent<Camera>();
    }
}
