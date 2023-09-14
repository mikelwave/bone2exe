using UnityEngine;

public class TutorialGraphicsChange : MonoBehaviour
{
    [SerializeField] Sprite otherSprite;
    // Start is called before the first frame update
    void Start()
    {
        if(DataShare.aimControlHold)
        {
            GetComponent<SpriteRenderer>().sprite = otherSprite;
        }
    }
}
