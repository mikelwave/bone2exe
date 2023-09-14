using UnityEngine;

public class DisableIcon : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if(Application.isPlaying)
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
