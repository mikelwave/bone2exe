using UnityEngine;

public class DisableEvent : MonoBehaviour
{
    public GameObject mainObject;
    [SerializeField] bool destroy = true;
    [SerializeField] bool returnToMain = false;
    public Transform mainParent = null;
    void Start()
    {
        if(mainObject == null) mainObject = gameObject;
        mainParent = mainObject.transform.parent;
    }
    public void Disable()
    {
        if(destroy)Destroy(mainObject);
        else
        {
            mainObject.SetActive(false);
            if(returnToMain)
            {
                mainObject.transform.SetParent(mainParent);
                mainObject.transform.localPosition = Vector3.zero;
            }
        }
    }
}
