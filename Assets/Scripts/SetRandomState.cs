using UnityEngine;

public class SetRandomState : MonoBehaviour
{
    // Start is called before the first frame update
    void Init()
    {
        Random.InitState(111);
    }
}
