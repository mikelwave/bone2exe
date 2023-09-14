using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    [SerializeField]
    bool DestroySpawnerOnSpawn = false;
    [SerializeField]
    bool AssignToSpawnerParent = false;
    [SerializeField]
    GameObject ToSpawn;
    [SerializeField] string spawnSound = "";
    [SerializeField] Vector2 spawnOffset;
    public void Spawn()
    {
        Transform t = Instantiate(ToSpawn,transform.position + (Vector3)spawnOffset,Quaternion.identity).transform;
        if(spawnSound!="")DataShare.PlaySound(spawnSound,transform.position + (Vector3)spawnOffset,false);
        if(AssignToSpawnerParent)
        {
            t.SetParent(transform.parent);
        }
        if(DestroySpawnerOnSpawn)
        {
            Destroy(gameObject);
        }
    }
}
