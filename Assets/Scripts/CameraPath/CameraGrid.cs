using UnityEngine;

public class CameraGrid : MonoBehaviour
{
    public Vector2 gridSize = new Vector2(10,10);
    
    #if UNITY_EDITOR
    public float lineWidth = 10f;
    public bool showGrid = true;
    public Color color = new Color(0.5f,0.5f,0.5f,0.25f);
    void OnDrawGizmos()
    {
        if(!showGrid) return;
        Vector3 pos = Camera.current.transform.position;
        Vector2 offset = gridSize/2;
        Gizmos.color = color;

        for(float y = pos.y - 800f; y < pos.y + 800f; y += gridSize.y)
        {
            Gizmos.DrawLine(new Vector3(-lineWidth, Mathf.Floor(y/gridSize.y) * gridSize.y+offset.y, 0f),
                            new Vector3(lineWidth,Mathf.Floor(y/gridSize.y) * gridSize.y+offset.y, 0f));
        }

        for(float x = pos.x - 1200f; x < pos.x + 1200f; x += gridSize.x)
        {
            Gizmos.DrawLine(new Vector3(Mathf.Floor(x/gridSize.x) * gridSize.x+offset.x, -lineWidth, 0f),
                            new Vector3(Mathf.Floor(x/gridSize.x) * gridSize.x+offset.x, lineWidth, 0f));
        }
    }
    #endif
}
