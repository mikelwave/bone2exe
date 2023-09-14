using UnityEngine;

public class PlaySoundOnCollision : MonoBehaviour
{
    [SerializeField] string soundName = string.Empty;
    void Start()
    {
        if(soundName == string.Empty) Destroy(this);
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        DataShare.PlaySound(soundName,transform.position,false);
    }
}
