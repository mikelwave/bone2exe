using UnityEngine;

[RequireComponent (typeof(PlayerItemTouch))]
public class OneUpItem : MonoBehaviour
{
    void Start()
    {
        PlayerItemTouch playerItemTouch = GetComponent<PlayerItemTouch>();
        playerItemTouch.itemPickupEvent = LivesPlayerEvent;
        playerItemTouch.checkIfCanPickUp = CheckLives;
    }
    void LivesPlayerEvent()
    {
        PlayerControl.SetLives(1,true);
    }
    bool CheckLives()
    {
        // Check if super mode inactive
        return GameMaster.superModeTime == 0;
    }
}
