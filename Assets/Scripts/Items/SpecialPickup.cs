using UnityEngine;

[RequireComponent (typeof(PlayerItemTouch))]
public class SpecialPickup : MonoBehaviour
{
    void Start()
    {
        PlayerItemTouch playerItemTouch = GetComponent<PlayerItemTouch>();
        playerItemTouch.itemPickupEvent = SpecialPickupEvent;
        playerItemTouch.checkIfCanPickUp = CheckPickup;
    }
    void SpecialPickupEvent()
    {
        GameObject obj = transform.GetChild(0).GetChild(0).gameObject;
        obj.transform.SetParent(null);
        obj.SetActive(true);
        GameMaster.Special = true;
    }
    bool CheckPickup()
    {
        return true;
    }
}
