using UnityEngine;

public class BGLoop : MonoBehaviour
{
    public Vector2 speeds = Vector2.one;
    public Vector2 offset = new Vector2(2.7775f,2.7775f);
    Transform child;
    Vector3 pos;
    // Start is called before the first frame update
    void Start()
    {
        child = transform.GetChild(0);
        pos = child.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(!DataShare.movingBGs) return;
        float step = Time.unscaledDeltaTime;
        pos.x = Mathf.Repeat(pos.x+step*speeds.x,offset.x*2);
        pos.y = Mathf.Repeat(pos.y+step*speeds.y,offset.y*2);

        child.localPosition = pos-(Vector3)offset;
    }
}
