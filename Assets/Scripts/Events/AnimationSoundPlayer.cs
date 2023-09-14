using UnityEngine;

public class AnimationSoundPlayer : MonoBehaviour
{
    public string[] soundStrings;
    [SerializeField] bool globalSound = false;
    public void PlaySound(int ID)
    {
        ID = Mathf.Clamp(ID,0,soundStrings.Length-1);
        if(soundStrings[ID] == string.Empty) return;

        if(!globalSound)
        {
            DataShare.PlaySound(soundStrings[ID],transform.position,false);
        }

        else
        {
            DataShare.PlaySound(soundStrings[ID],false,0.2f,1f);
        }
    }
}
