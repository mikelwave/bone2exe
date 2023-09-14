using UnityEngine;
using System.Collections.Generic;

public class ObjectEnabler : MonoBehaviour
{
    bool Active = false;
    [SerializeField] List <MonoBehaviour> UnaffectedComponents;
    List <MonoBehaviour> components;
    GameObject[] children;
    [HideInInspector]
    public bool canDespawn = true;
    Collider2D col;
    bool resetting = false;
    public void Reset()
    {
        if(col == null) return;
        resetting = true;
        col.enabled = false;
        col.enabled = true;
        resetting = false;
    }
    void Start()
    {
        col = GetComponent<Collider2D>();
        children = new GameObject[transform.childCount];
        MonoBehaviour[] componentsList = transform.GetComponents<MonoBehaviour>();
        components = new List<MonoBehaviour>();

        if(UnaffectedComponents.Count != 0)
        for (int i = 0; i < componentsList.Length; i++)
        {
            if(!UnaffectedComponents.Contains(componentsList[i]))
            {
                components.Add(componentsList[i]);
            }
        }
        else
        {
            for (int i = 0; i < componentsList.Length; i++)
            {
                components.Add(componentsList[i]);
            }
        }
        
        for(int i = 0;i<children.Length;i++)
        {
            children[i] = transform.GetChild(i).gameObject;
        }
        if(!Active) Spawn(false);
    }
    public void Spawn(bool spawn)
    {
        if(components==null)return;
        foreach(MonoBehaviour c in components)
        {
            if(c!=null)
            c.enabled = spawn;
        }
        foreach(GameObject o in children)
        {
            if(o!=null)
            o.SetActive(spawn);
        }
        this.enabled = true;
        Active = spawn;
    }
    public void Despawn()
    {
        foreach(MonoBehaviour c in components)
        {
            c.enabled = false;
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(!resetting && other.name=="ObjActivator" && !Active)
        {
            Spawn(true);
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(!resetting && other.name=="ObjActivator" && canDespawn && Active)
        {
            Spawn(false);
        }
    }
}
