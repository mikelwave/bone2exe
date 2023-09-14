using UnityEngine;
using UnityEngine.UI;

// Attach this script to the object with the button image
public class IconUI : MonoBehaviour
{
    [SerializeField]
    Sprite[] iconSprite = new Sprite[2];
    Image iconImage;
    // Start is called before the first frame update
    void OnEnable()
    {
        if(iconImage!=null) return;
        iconImage = GetComponent<Image>();
        MenuButton menuButton = transform.parent.GetComponent<MenuButton>();
        menuButton.hoverEvent = Event;
    }
    void Event(bool In)
    {
        iconImage.sprite = iconSprite[In ? 1 : 0];
    }
}
