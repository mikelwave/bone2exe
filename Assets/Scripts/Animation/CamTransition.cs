using UnityEngine;

// Attached to the player transform
public class CamTransition : MonoBehaviour
{
    public CamControl camControl;
    Transform cameraTransform;
    public delegate void OnTransitionBeginEvent();
    public OnTransitionBeginEvent onTransitionBeginEvent;

    public delegate void OnTransitionExitEvent();
    public OnTransitionExitEvent onTransitionExitEvent;
    Rigidbody2D rb;
    BoxCollider2D boxCollider2D;
    Collider2D SSMap;
    PlayerControl playerControl;
    float savedYVelocity = 0;

    CameraGrid grid;
    Vector3Int gridPos = Vector3Int.zero;
    public Vector3Int GetGridPos { get {return gridPos;}}
    public Vector2 GetWorldPos { get {return new Vector2(gridPos.x*grid.gridSize.x,gridPos.y*grid.gridSize.y);}}

    public float GetCameraY()
    {
        ///print(GetWorldPos.y + " " + cameraTransform.position.y);
        if(camControl.cameraFollowMode == CamControl.CameraFollowMode.FollowVertical)
        return GetWorldPos.y;
        else return cameraTransform.position.y;

    }
    public bool CheckInBounds(Vector3 pos)
    {
        if(grid==null)grid = camControl.grid;
        Vector3Int newGridPos = Vector3Int.zero;
        newGridPos.x = (int)(Mathf.Round((pos.x)/grid.gridSize.x));
        newGridPos.y = (int)(Mathf.Round((pos.y)/grid.gridSize.y));

        ///print("New grid pos: "+newGridPos);

        if(camControl.curCamCell==newGridPos)
        {
            return true;
        }
        gridPos = newGridPos;
        return false;
    }
    public bool CompareGridPos(Vector3 pos)
    {
        if(grid==null)grid = camControl.grid;
        Vector3Int newGridPos = Vector3Int.zero;
        newGridPos.x = (int)(Mathf.Round((pos.x)/grid.gridSize.x));//*grid.gridSize.x);
        newGridPos.y = (int)(Mathf.Round((pos.y)/grid.gridSize.y));//*grid.gridSize.y);

        if(gridPos==newGridPos)
        {
            #if UNITY_EDITOR
            Debug.DrawLine(pos,(Vector2Int)newGridPos*grid.gridSize,Color.green);
            #endif
            return true;
        }
        else
        {
            #if UNITY_EDITOR
            Debug.DrawLine(pos,(Vector2Int)newGridPos*grid.gridSize,Color.red);
            #endif
            gridPos = newGridPos;
            return false;
        }
    }
    public void ResetBoxCollider()
    {
        boxCollider2D.enabled = false;
        boxCollider2D.enabled = true;
        if(playerControl.grounded) Physics2D.IgnoreCollision(boxCollider2D,SSMap,false);
    }
    void Start()
    {
        playerControl = transform.parent.GetComponent<PlayerControl>();
        camControl = GameObject.FindWithTag("MainCamera").GetComponent<CamControl>();
        SSMap = GameObject.FindWithTag("SSMap").GetComponent<Collider2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(boxCollider2D,SSMap,false);
        cameraTransform = camControl.transform;
        camControl.moveWithCameraEvent += MoveWithCamOffset;
        camControl.onTransitionExitEvent += ResetBoxCollider;
        grid = camControl.grid;

        rb = transform.parent.GetComponent<Rigidbody2D>();
    }
    Vector3 MoveWithCamOffset(Vector3 offset)
    {
        // Offset = normalized direction;
        float YMultiplier = playerControl.GetLadderVal > 1 ? 1.8f : 1f;
        if(offset.y != -1)
        YMultiplier = (offset.y == 0 || !MGInput.GetButton(MGInput.controls.Player.Jump)) ? 1.8f : 3f;
        Vector2 distance = new Vector2(1.3f,YMultiplier);
        Vector2 startPos = transform.parent.position;
        Vector2 colliderSize = PlayerControl.col.size;

        Vector2 destination = startPos + offset * distance;
        ///print("Offset: "+offset);

        if(offset == Vector3.up || offset.x != 0)
        {
            if(offset.y == 1) startPos-=(Vector2)offset;

            ///Debug.DrawLine(startPos,startPos + (Vector2)offset*distance,Color.red,1f);

            RaycastHit2D[] hit = Physics2D.BoxCastAll(startPos,colliderSize,
            0,(Vector2)offset,offset.x == 0 ? distance.y : distance.x+Mathf.Abs(offset.x),128);

            foreach(RaycastHit2D h in hit)
            {
                if(h.collider.tag == "MainMap")
                {
                    destination = h.centroid;
                    Debug.DrawLine(startPos,destination,Color.magenta,1f);
                    break;
                }
            }
            ///print("Movement: "+(destination - startPos));
        }

        // Cancel transition if destination can't fit player on screen
        if(offset.y == 1 && (destination - startPos).y<=PlayerControl.col.size.y)
        {
            destination = Vector3.up*-999;
        }
        else
        {
            if(offset == Vector3.up)
            {
                PlayerControl.freezeJump = true;
                rb.velocity = new Vector2(rb.velocity.x,0);
            }
        }

        return destination;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        string otherName = other.name;
        if(otherName.Contains("CamTransition"))
        {
            savedYVelocity = rb.velocity.y;

            if(CamControl.midTransition && camControl.curCamCell == gridPos) return;
            bool special = otherName.Contains("special");
            
            if(special)
            {
                ///print("Special transition");
                camControl.CorrectCamCell();
            }

            Vector3 pos = other.transform.position;
            Vector3Int targetCell = camControl.curCamCell;
            Vector2 offset = new Vector2
            (Mathf.Clamp(rb.velocity.x*100,-1,1),
             Mathf.Clamp(rb.velocity.y*100,-1,1));
            pos = pos + (Vector3)offset;
            ///print(offset);

            // Snap to grid
            if(grid==null)grid = camControl.grid;
            if(grid==null) return;
            pos.x = Mathf.Round((pos.x)/grid.gridSize.x);
            pos.y = Mathf.Round((pos.y)/grid.gridSize.y);
            ///Debug.DrawLine(transform.position,pos,Color.red,4f);

            targetCell = new Vector3Int(Mathf.RoundToInt(pos.x),Mathf.RoundToInt(pos.y),targetCell.z);

            if(targetCell == camControl.curCamCell) return;
            //Check if can move in that direction
            if(camControl.Evaluate(targetCell))
            {
                ///print("Move success");
                camControl.StaticTransition(targetCell,special);
            }
        }
    }
}
