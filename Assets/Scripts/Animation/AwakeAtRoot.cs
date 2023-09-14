using UnityEngine;

public class AwakeAtRoot : MonoBehaviour
{
    public void Init()
    {
        transform.SetParent(null);
        gameObject.SetActive(true);
    }
}
