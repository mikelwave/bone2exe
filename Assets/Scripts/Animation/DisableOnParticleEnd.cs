using UnityEngine;

public class DisableOnParticleEnd : MonoBehaviour
{
    [SerializeField]bool DestroyObj = false;
    public GameObject ToDisable;
    void OnParticleSystemStopped()
    {
        if(ToDisable==null) ToDisable = gameObject;
        if(!DestroyObj) ToDisable.SetActive(false);
        else Destroy(ToDisable);
    }
}
