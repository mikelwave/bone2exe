using UnityEngine;

[RequireComponent (typeof(PlayerItemTouch))]
public class HealthItem : MonoBehaviour
{
    void Start()
    {
        PlayerItemTouch playerItemTouch = GetComponent<PlayerItemTouch>();
        playerItemTouch.itemPickupEvent = HealPlayerEvent;
        playerItemTouch.checkIfCanPickUp = CheckHealth;
    }
    void HealPlayerEvent()
    {
        PlayerControl.SetHP(1);
    }
    bool CheckHealth()
    {
        return PlayerControl.currentHealth < GameMaster.maxHealth;
    }
}
