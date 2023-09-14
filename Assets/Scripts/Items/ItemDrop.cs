using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    public GameObject itemDropNormal;
    public GameObject itemDromDark;
    public float dropItemVelocity = 5;
    public Material normalMaterial;
    bool dropped = false;
    public void DropItem()
    {
        if(dropped) return;
        dropped = true;
        transform.GetChild(0).GetComponent<SpriteRenderer>().material = normalMaterial;
        GameObject obj = Instantiate(GameObject.FindWithTag("GameMaster").GetComponent<GameMaster>().worldMode == GameMaster.WorldMode.LightMode ? itemDropNormal : itemDromDark,transform.position,Quaternion.identity);
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if(rb!=null)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(Vector2.up*dropItemVelocity,ForceMode2D.Impulse);
        }
    }
}
