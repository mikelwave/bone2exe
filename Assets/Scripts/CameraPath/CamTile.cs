using UnityEngine;
public class CamTile
{
    Vector2Int position;
    string tileName;

    public string TileName { get { return tileName; } }
    public Vector2Int Position { get {return position; } }
    public CamTile(string tileName, Vector2Int position)
    {
        this.tileName = tileName;
        this.position = position;
    }
}
