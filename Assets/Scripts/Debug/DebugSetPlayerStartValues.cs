using UnityEngine;

public class DebugSetPlayerStartValues : MonoBehaviour
{
    #if UNITY_EDITOR
    public int startLives = 5;
    [Range (0,2)]
    public int startHealth = 2;
    // Start is called before the first frame update
    void Awake()
    {
        if(Time.realtimeSinceStartup<1f)
        {
            if(!CheatInput.LockLives)
            PlayerControl.currentLives = startLives;
            if(!CheatInput.LockHealth)
            PlayerControl.currentHealth = startHealth;
        }
    }
    #endif
}
