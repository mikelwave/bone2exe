using UnityEngine;
using UnityEngine.Tilemaps;

public class BackgroundMove : MonoBehaviour
{
    public bool moveX = true;
    public float movementSpeedX = 1;
    public bool moveY = false;
    public float movementSpeedY = 1;
    Vector2 gridSize;
    Tilemap tilemap;
    Matrix4x4 matrix;

    // Start is called before the first frame update
    void Start()
    {
        gridSize = (Vector2)transform.parent.GetComponent<Grid>().cellSize;
        tilemap = GetComponent<Tilemap>();
        matrix = tilemap.orientationMatrix;
    }

    // Update is called once per frame
    void Update()
    {
        if(!DataShare.movingBGs) return;
        // Get position offset
        Vector4 pos = matrix.GetColumn(3);
        if(moveX)
        {
            pos.x = Mathf.Repeat(pos.x+(Time.deltaTime*movementSpeedX),gridSize.x);
        }
        if(moveY)
        {
            pos.y = Mathf.Repeat(pos.y+(Time.deltaTime*movementSpeedY),gridSize.y);
        }
        // Assign updated offset
        matrix.SetColumn(3,pos);
        tilemap.orientationMatrix = matrix;
    }
}
