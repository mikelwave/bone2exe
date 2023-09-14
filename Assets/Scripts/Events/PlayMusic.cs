using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    [SerializeField] string MusicName;
    // Start is called before the first frame update
    void Start()
    {
        DataShare.LoadMusic(MusicName);
    }
}
