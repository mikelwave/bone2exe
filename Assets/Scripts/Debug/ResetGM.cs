using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof (GameMaster))]
public class ResetGM : MonoBehaviour
{
    #if UNITY_EDITOR
    // Start is called before the first frame update
    void Start()
    {
        if(!Application.isPlaying)
        GameMaster.Reset();
    }
    #endif
}
