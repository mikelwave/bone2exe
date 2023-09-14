using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraSetup : MonoBehaviour
{
    CamControl camControl;
    CameraGrid grid;
    Transform cam;
    List<CamTile> tiles;
    Tilemap t;
    Vector2 gridHalfSize;
    public GameObject[] Transitions;
    public List<Vector3Int> horizontalTiles;
    public List<Vector3Int> verticalTiles;
    public void SnapCam()
    {
        Transform target;
        Transform player = GameObject.FindWithTag("Player").transform;
        player.GetComponent<PlayerControl>().FindSpawn();
        if(player!=null)target = player;
        else target = cam;
        // Snap camera to grid
        Vector3 gridSize = grid.gridSize;
        Vector3 pos = target.position;
        pos.x = Mathf.Round((pos.x)/gridSize.x)*gridSize.x;
        pos.y = Mathf.Round((pos.y)/gridSize.y)*gridSize.y;

        ///print("Snap cam: "+pos);
        cam.position = pos;
        camControl.CorrectCamCell();
    }
    Vector3 CalculateWorldPos(Vector3Int tilePos)
    {
        return (Vector3)(grid.gridSize*(Vector3)tilePos);
    }
    Vector3 CalculateWorldPosNoOffset(Vector3Int tilePos)
    {
        return (Vector3)(grid.gridSize*(Vector3)tilePos);
    }
    // 0 - false, 1 - true, normal, 2 - true, special
    byte EvaluationTransitionPlace(string baseName, string twinName,Vector2Int direction)
    {
        if(Mathf.Abs(direction.x+direction.y)>1) return 0;

        // If types are non static, and match but on axis they don't scroll in, return true
        if(!baseName.ToLower().Contains("static") && baseName == twinName)
        {
            if((baseName.ToLower().Contains("ver") && direction.x != 0)
            ||(baseName.ToLower().Contains("hor") && direction.y != 0)) return 2;

            return 0;
        }
        // If types don't match or either type has static in the name - return true
        if(baseName != twinName) return 2;

        return 1;
    }
    void FindAdjacent()
    {
        camControl.tileSpots = new List<Vector3Int>();
        tiles = new List<CamTile>();
        horizontalTiles = new List<Vector3Int>();
        verticalTiles = new List<Vector3Int>();
        t = transform.GetChild(1).GetChild(0).GetComponent<Tilemap>();
        camControl.calculateWorldPos = CalculateWorldPos;

        #if UNITY_EDITOR
        t.CompressBounds();
        #endif

        Vector3Int pos = Vector3Int.zero;
        BoundsInt bounds = t.cellBounds;
        ///print("Bounds: "+bounds.min+ " "+bounds.max);

        for(int y = bounds.min.y; y<bounds.max.y; y++)
        {
            for(int x = bounds.min.x; x<bounds.max.x; x++)
            {
                pos.x = x;
                pos.y = y;
                TileBase tile = t.GetTile(pos);
                if(tile!=null)
                {
                    tiles.Add(new CamTile(tile.name,(Vector2Int)pos));
                    ///print((Vector2)t.CellToWorld(pos)+(grid.gridSize/2));
                    string name = tile.name.ToLower();
                    if(name.Contains("horizontal"))
                    {
                        horizontalTiles.Add(pos);
                    }
                    else if(name.Contains("vertical"))
                    {
                        verticalTiles.Add(pos);
                    }
                }
            }
        }
        if(tiles.Count==0) return;

        // Find if tiles have adjacents
        gridHalfSize = (grid.gridSize/2);
        ///print("Half size: "+gridHalfSize);
        while(tiles.Count>1)
        {
            Vector3Int tilePos = (Vector3Int)tiles[0].Position;
            string tileBaseName = t.GetTile(tilePos).name;
            ///print("Tile "+tilePos);

            // Remove from future checks
            camControl.tileSpots.Add((tilePos));

            tiles.Remove(tiles[0]);

            foreach(CamTile twin in tiles)
            {
                Vector3Int twinPos = (Vector3Int)twin.Position;
                string twinBasename = t.GetTile(twinPos).name;
                byte evaluateVal = EvaluationTransitionPlace(tileBaseName,twinBasename,(Vector2Int)(twinPos - tilePos));
                if(evaluateVal > 0)
                {
                    bool special = evaluateVal == 2;

                    float Distance = Mathf.Round(Vector3Int.Distance(tilePos,twinPos) * 10) / 10;
                    ///print("Check tile: "+tilePos+", Test pos: "+twinPos+ ", Distance: "+Distance);
                    if(Distance==1)
                    {
                        ///print("Twin pos: "+twinPos+ " world pos: "+ (t.CellToWorld(twinPos)+(Vector3)gridHalfSize+new Vector3(1,3,0))+" test: "+CalculateWorldPos(twinPos));
                        Vector2Int direction = (Vector2Int)(twinPos - tilePos);

                        // Spawn obj
                        GameObject obj = Instantiate(Transitions[direction.x!=0?0:1]);
                        if(special)obj.name+="special";
                        obj.transform.SetParent(transform);

                        // Horizontal only
                        if(direction.x == 1)
                        {
                            bool extend = (t.GetTile(twinPos+Vector3Int.up) == null && t.GetTile(tilePos+Vector3Int.up) == null );
                            if(extend)
                            {
                                EdgeCollider2D edge = obj.GetComponent<EdgeCollider2D>();
                                Vector2[] points = edge.points;
                                points[0].y = 30;
                                edge.points = points;
                            }
                            ///print("Extend out of bounds for pos"+ (twinPos+Vector3Int.up) +": "+extend);
                        }
                        obj.transform.position = ((Vector3)(grid.gridSize*(Vector2Int)twinPos))-(Vector3)(gridHalfSize*direction);

                        ///print(direction);
                        //camControl.tileSpots.Add(twinPos);
                        #if UNITY_EDITOR
                        Debug.DrawLine((Vector2)t.CellToWorld(tilePos)+gridHalfSize,(Vector2)t.CellToWorld(twinPos)+gridHalfSize,special ? Color.yellow : Color.white,10f);
                        #endif
                    }
                }
            }
        }
        camControl.tileSpots.Add(((Vector3Int)tiles[0].Position));

        // Line paths
        if(horizontalTiles.Count!=0)
        {
            camControl.horizontalPaths = new List<Vector3Range>();
            horizontalTiles.Sort((a,b) => a.y.CompareTo(b.y));
            Vector3 horMin = Vector3.zero;
            Vector3 horMax = Vector3.zero;
            bool newPath = true;
            int PathID = 0;
            // Make horizontal paths
            for(int i = 0; i<horizontalTiles.Count;i++)
            {
                bool acceptable = i<horizontalTiles.Count-1
                && (horizontalTiles[i+1].y == horizontalTiles[i].y
                && horizontalTiles[i+1].x - horizontalTiles[i].x == 1);
                // Start new path
                if(newPath)
                {
                    if(acceptable)
                    {
                        newPath = false;
                        horMin = CalculateWorldPosNoOffset(horizontalTiles[i]);
                    }
                    continue;
                }
                // If Y value is the same & Difference between previous and current X = 1
                else
                {
                    ///print("ID: "+PathID+" Tiles: "+horizontalTiles[i-1]+", "+horizontalTiles[i]+
                    ///" Y match: "+(horizontalTiles[i].y == horizontalTiles[i-1].y)+
                    ///" Difference match: "+(horizontalTiles[i].x - horizontalTiles[i-1].x == 1));
                    if(acceptable)
                    continue;

                    // Mark passed point as the end of path
                    else
                    {
                        ///print("End tile: "+horizontalTiles[i]);
                        horMax = CalculateWorldPosNoOffset(horizontalTiles[i]);
                        newPath = true;
                        PathID++;
                        camControl.horizontalPaths.Add(new Vector3Range(horMin,horMax));
                        #if UNITY_EDITOR
                        ///print("Horizontal path "+PathID+" "+horMin+" - "+horMax);
                        Debug.DrawLine(horMin,horMax,Color.green,10f);
                        #endif
                        
                        continue;
                    }
                }
            }
        }
        if(verticalTiles.Count!=0)
        {
            camControl.verticalPaths = new List<Vector3Range>();
            verticalTiles.Sort((a,b) => a.x.CompareTo(b.x));
            Vector3 verMin = Vector3.zero;
            Vector3 verMax = Vector3.zero;
            bool newPath = true;
            int PathID = 0;
            // Make vertical paths
            for(int i = 0; i<verticalTiles.Count;i++)
            {
                bool acceptable = i<verticalTiles.Count-1
                && (verticalTiles[i+1].x == verticalTiles[i].x
                && verticalTiles[i+1].y - verticalTiles[i].y == 1);
                // Start new path
                if(newPath)
                {
                    if(acceptable)
                    {
                        newPath = false;
                        verMin = CalculateWorldPosNoOffset(verticalTiles[i]);
                    }
                    continue;
                }
                // If Y value is the same & Difference between previous and current X = 1
                else
                {
                    ///print("ID: "+PathID+" Tiles: "+verticalTiles[i-1]+", "+verticalTiles[i]+
                    ///" Y match: "+(verticalTiles[i].y == verticalTiles[i-1].y)+
                    ///" Difference match: "+(verticalTiles[i].x - verticalTiles[i-1].x == 1));
                    if(acceptable)
                    continue;

                    // Mark passed point as the end of path
                    else
                    {
                        ///print("End tile: "+verticalTiles[i]);
                        verMax = CalculateWorldPosNoOffset(verticalTiles[i]);
                        newPath = true;
                        PathID++;
                        camControl.verticalPaths.Add(new Vector3Range(verMin,verMax));
                        #if UNITY_EDITOR
                        ///print("vertical path "+PathID+" "+verMin+" - "+verMax);
                        Debug.DrawLine(verMin,verMax,Color.red,10f);
                        #endif
                        
                        continue;
                    }
                }
            }
        }
        camControl.SpecialSetup(camControl.transform.position);
    }
    // Start is called before the first frame update
    void Awake()
    {
        grid = GetComponent<CameraGrid>();
        cam = transform.GetChild(0);
        camControl = transform.GetChild(0).GetComponent<CamControl>();
        Camera camera1 = camControl.transform.GetChild(0).GetComponent<Camera>();
        if(camera1.orthographic)
        {
            camera1.orthographic = false;
            Debug.Log("Forcing perspective type camera.");
        }
        camControl.grid = grid;

        SnapCam();
        FindAdjacent();
        transform.GetChild(1).gameObject.SetActive(false);
        ///print("Camera setup time: "+Time.realtimeSinceStartup);
    }
    void Start()
    {
        if(!DataShare.pixelScaling) DataShare.TogglePixelPerfectScaling(false);
    }
}
