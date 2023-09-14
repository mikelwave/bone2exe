using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class AssignToObject : MonoBehaviour
{
    public string ObjectName = "Enemies";
    // Start is called before the first frame update
    void Awake()
    {
        Transform parent = GameObject.Find(ObjectName).transform;
        if(parent!=null)transform.SetParent(parent);
    }
}
