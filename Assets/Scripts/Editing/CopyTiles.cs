using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class CopyTiles : MonoBehaviour
{
    #if UNITY_EDITOR
    Tilemap curTilemap;
    [Tooltip ("Main tilemap to copy from (will attempt to find one automatically if left blank).")]
    public Tilemap mainTilemap;
    [Tooltip ("Tiles that will be excluded during copying.")]
    public TileBase[] blacklistedTiles = new TileBase[0];
    [Tooltip ("Press this to make a copy of the assigned tilemap.")]
    public bool Copy = false;

    void OnValidate() { UnityEditor.EditorApplication.delayCall = _OnValidate; }
    void _OnValidate()
    {
        if(Copy)
        {
            Copy = false;
            if(mainTilemap == null)
            {
                mainTilemap = GameObject.Find("MainMap").GetComponent<Tilemap>();
            }
            curTilemap.ClearAllTiles();
            mainTilemap.CompressBounds();
            BoundsInt bounds = mainTilemap.cellBounds;
            TileBase[] tileArray = mainTilemap.GetTilesBlock(bounds);
            // Get rid of blacklisted tiles
            for(int i = 0; i<tileArray.Length;i++)
            {
                foreach(TileBase tb in blacklistedTiles)
                {
                    if(tileArray[i] == tb)
                    {
                        tileArray[i] = null;
                        break;
                    }
                }
            }
            
            curTilemap.SetTilesBlock(bounds,tileArray);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        curTilemap = transform.GetComponent<Tilemap>();
    }
    #endif
}
