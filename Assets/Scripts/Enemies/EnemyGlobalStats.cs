
using UnityEngine;
using System.Collections.Generic;

public class EnemyGlobalStats : MonoBehaviour
{
    
    public enum Type {Light,Dark};
    [Header("Enemy type stats")]
    [Space]
    public Type type = Type.Light;
    public byte HP = 5;
    protected byte currentHP = 0;
    public byte CurrentHP { get {return currentHP; } }
    public bool respawns = true;
    public bool Active = true;
    public bool lockSpawn = false;
    public bool ignoreBoundsOnSpawn = false;
    protected Animator anim;
    MonoBehaviour[] components;
    List <GameObject> children;
    public bool isDead { get {return currentHP<=0; } }
    public delegate void DespawnEvent();
    public DespawnEvent despawnEvent;
    public delegate void SpawnEvent();
    public SpawnEvent spawnEvent;
    public bool Hurt(byte damage)
    {
        // Returns true if damage was lethal, otherwise false
        if(currentHP<=0) return false;
        if(currentHP>=damage) currentHP -= damage;
        else currentHP = 0;
        return currentHP<=0;
    }
    // Start is called before the first frame update
    protected void Init()
    {
        currentHP = HP;
        anim = GetComponent<Animator>();
        if(anim == null) anim = transform.GetComponentInChildren<Animator>();
        components = transform.GetComponents<MonoBehaviour>();
        children = new List<GameObject>();
        for(int i = 0; i < transform.childCount; i++)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            if(obj.activeInHierarchy)
            children.Add(obj);
        }
    }
    protected virtual void Start()
    {
        ///print(transform.name + " Start");

        // Check if enemies are inside the updated bounds
        if(Active && !ignoreBoundsOnSpawn)
        {
            Spawn(CamControl.objActivatorBounds.Contains(transform.position));
        }
    }
    void OnEnable()
    {
        if(respawns) currentHP = HP;
    }
    public void Spawn(bool spawn)
    {
        if(Active == spawn) return;

        if(spawn) spawnEvent?.Invoke();
        if(components == null || components.Length==0) Init();
        anim.enabled = spawn; // This caused some editor crashes in the past, should inspect

        foreach(MonoBehaviour c in components)
        {
            if(c!=null)
            c.enabled = spawn;
        }
        foreach(GameObject o in children)
        {
            o.SetActive(spawn);
        }
        this.enabled = true;
        Active = spawn;
        
        if(!spawn)
        despawnEvent?.Invoke();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(!lockSpawn && other.name=="ObjActivator")
        {
            Spawn(true);
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(!lockSpawn && other.name=="ObjActivator")
        {
            Spawn(false);
        }
    }
}
