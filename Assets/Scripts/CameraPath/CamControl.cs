using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour
{
    public CameraGrid grid;
    public List <Vector3Int> tileSpots;
    public Vector3Int curCamCell;

    // Paths
    public List<Vector3Range> horizontalPaths;
    public List<Vector3Range> verticalPaths;

    public delegate Vector3 CalculateWorldPos(Vector3Int tilePos);
    public CalculateWorldPos calculateWorldPos;
    public delegate Vector3 MoveWithCameraEvent(Vector3 point);
    public MoveWithCameraEvent moveWithCameraEvent;
    Rigidbody2D targetRb;
    public float transitionSpeed = 1;

    public delegate void OnTransitionBeginEvent();
    public OnTransitionBeginEvent onTransitionBeginEvent;

    public delegate void OnTransitionExitEvent();
    public OnTransitionExitEvent onTransitionExitEvent;
    public Coroutine cameraTransition;

    Vector3Range activeRange;
    public enum CameraFollowMode {Static,FollowHorizontal,FollowVertical};
    public CameraFollowMode cameraFollowMode;
    Transform followTarget;

    delegate Vector3 FollowTargetFunc(bool set);
    FollowTargetFunc followTargetFunc;

    // Collision
    BoxCollider2D objActivator;
    public static Bounds objActivatorBounds;
    Transform objActivatorTransform;

    // Self
    public static Transform cameraTransform;
    public static CamControl self;

    public static bool midTransition = false;

    static Coroutine shakeCor;

    [SerializeField] bool inactive = false;

    float curShakeAmount = 0;
    public IEnumerator IShake(float ShakeAmount, float shakeTime)
    {
        float curShake = 0;
        curShakeAmount = ShakeAmount;
        short multiplier = 1;
        Vector3 offset = Vector2.one;
        float startZ = cameraTransform.localPosition.z;
        offset.z = startZ;
        while(curShake<shakeTime)
        {
            if(Time.timeScale != 0)
            {
                float timeRemaining = (shakeTime-curShake)/shakeTime;
                offset.x = Random.Range(-1,1.01f)*ShakeAmount*timeRemaining;
                offset.y = Random.Range(-1,1.01f)*ShakeAmount*timeRemaining;
                cameraTransform.localPosition = offset;
                curShake+=Time.deltaTime;
                multiplier*=-1;
            }
            yield return 0;
        }
        curShakeAmount = 0;
        cameraTransform.localPosition = Vector3.forward*startZ;
    }
    public static void ShakeCamera(float ShakeAmount, float shakeTime)
    {
        if(!DataShare.screenshake) return;
        if(shakeCor!=null && ShakeAmount > self.curShakeAmount) self.StopCoroutine(shakeCor);
        self.StartCoroutine(self.IShake(ShakeAmount,shakeTime));
    }

    void Awake()
    {
        if(objActivatorTransform != null) return;
        CamControl.self = this;
        CamControl.cameraTransform = transform.GetChild(0);
        if(inactive) return;
        followTarget = GameObject.FindWithTag("Player").transform;
        targetRb = followTarget.GetComponent<Rigidbody2D>();
        objActivator = transform.parent.GetChild(2).GetComponent<BoxCollider2D>();
        objActivatorTransform = objActivator.transform;
        objActivatorTransform.gameObject.SetActive(false);
        
        objActivatorTransform.position = transform.position;
        objActivator.size = new Vector2(36f,20);
        objActivatorBounds = objActivator.bounds;
        objActivatorTransform.localScale = Vector3.one;
        objActivatorTransform.SetParent(null);
        objActivatorTransform.gameObject.SetActive(true);
    }
    void Update()
    {
        if(cameraTransition==null)
        followTargetFunc?.Invoke(true);
    }
    Vector3 FollowTargetHor(bool set)
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(followTarget.position.x,activeRange.Min.x,activeRange.Max.x);
        if(set)
        {
            transform.position = pos;
        }
        return pos;
    }
    Vector3 FollowTargetVer(bool set)
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(followTarget.position.y,activeRange.Min.y,activeRange.Max.y);
        if(set)
        {
            transform.position = pos;
        }
        return pos;
    }
    public void CorrectCamCell()
    {
        Vector3 pos = transform.localPosition/grid.gridSize;
        curCamCell = new Vector3Int(Mathf.RoundToInt(pos.x),Mathf.RoundToInt(pos.y),0);
    }
    public Vector3Int GetPosInCamCells(Vector3 pos)
    {
        pos = pos/grid.gridSize;
        return new Vector3Int(Mathf.RoundToInt(pos.x),Mathf.RoundToInt(pos.y),0);
    }
    IEnumerator ICameraTransition(Vector3 newPos,Vector3Int tilePos,bool special,Vector3 targetEndPos)
    {
        midTransition = true;
        onTransitionBeginEvent?.Invoke();
        float progress = 0;
        Vector3 startPos = transform.position;
        Vector3 targetStartPos = followTarget.position;
        Vector3 direction = (Vector3)(tilePos-curCamCell);
        if(special)
        {
            SpecialSetup(newPos);
        }
        else
        {
            RepositionObjActivator(Vector3.one,newPos);
        }
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*transitionSpeed;
            float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
            Vector3 newTargetPos = followTarget.position;

            if(special)
            {
                if(cameraFollowMode == CameraFollowMode.FollowHorizontal) newPos.x = FollowTargetHor(false).x;
                else if(cameraFollowMode == CameraFollowMode.FollowVertical) newPos.y = FollowTargetVer(false).y;
            }

            if(direction != Vector3.up)
            {
                if(direction.x!=0)
                newTargetPos.x = Mathf.Lerp(targetStartPos.x,targetEndPos.x,mathStep);
                if(direction.y!=0)
                newTargetPos.y = Mathf.Lerp(targetStartPos.y,targetEndPos.y,mathStep);
                followTarget.position = newTargetPos;
            }
            else
            {
                newTargetPos.x = targetRb.position.x+(targetRb.velocity.x*Time.fixedDeltaTime);
                newTargetPos.y = Mathf.Lerp(targetStartPos.y,targetEndPos.y,mathStep);
                targetRb.MovePosition(newTargetPos);
            }

            transform.position = Vector3.Lerp(startPos,newPos,mathStep);

            yield return 0;
        }
        PlayerControl.freezeJump = false;
        onTransitionExitEvent?.Invoke();
        midTransition = false;
        cameraTransition = null;
    }
    void RepositionObjActivator(Vector3 scale, Vector3 pos)
    {
        ///print("Bounds defined");
        if(objActivatorTransform == null) Awake();
        objActivatorTransform.localScale = scale;
        objActivatorTransform.position = pos;
        objActivatorBounds = new Bounds(objActivatorTransform.position,scale*objActivator.size);
        ///Debug.DrawLine(objActivatorBounds.min,objActivatorBounds.max,Color.green,5f);
        ///print("ObjActivator pos: "+pos+ " size: "+scale);
    }
    
    public bool Evaluate(Vector3Int newPoint)
    {
        foreach(Vector3Int cell in tileSpots)
        {
            ///print("Cell check: "+cell);
            if(cell==newPoint)
            {
                ///print("Match: "+newPoint+", "+cell);
                return true;
            }
        }
        print("No match for cell: "+newPoint);
        return false;
    }  
    public void SpecialSetup(Vector3 pos)
    {
        ///print("Special setup for pos: "+pos);
        Vector3 ActivatorSize = Vector3.one;
        Vector3 ActivatorPos = Vector3.zero;

        // Horizontal
        if(horizontalPaths!=null)
        for(int i = 0;i<horizontalPaths.Count;i++)
        {
            ///print("Checking path: "+pos+", "+horizontalPaths[i].Min+" - "+horizontalPaths[i].Max);
            // Y must match
            // X clamp must not be changed
            if(pos.y==horizontalPaths[i].Min.y
            && Mathf.Clamp(pos.x,horizontalPaths[i].Min.x,horizontalPaths[i].Max.x) == pos.x)
            {
                activeRange = horizontalPaths[i];
                ActivatorSize.x = (Mathf.Abs(activeRange.Min.x-activeRange.Max.x)/36f) + 1;
                ActivatorPos = new Vector3((activeRange.Min.x+activeRange.Max.x)/2f,activeRange.Min.y,0);
                RepositionObjActivator(ActivatorSize,ActivatorPos);

                followTargetFunc = FollowTargetHor;
                cameraFollowMode = CameraFollowMode.FollowHorizontal;
                ///print("Matching path: "+pos+", "+activeRange.Min+" - "+activeRange.Max);
                return;
            }
            else continue;
        }
        // Vertical
        if(verticalPaths!=null)
        for(int i = 0;i<verticalPaths.Count;i++)
        {
            // X must match
            // Y clamp must not be changed
            if(pos.x==verticalPaths[i].Min.x
            && Mathf.Clamp(pos.y,verticalPaths[i].Min.y,verticalPaths[i].Max.y) == pos.y)
            {
                activeRange = verticalPaths[i];
                ActivatorSize.y = (Mathf.Abs(activeRange.Min.y-activeRange.Max.y)/20f) + 1;
                ActivatorPos = new Vector3(activeRange.Min.x,(activeRange.Min.y+activeRange.Max.y)/2f,0);
                RepositionObjActivator(ActivatorSize,ActivatorPos);

                followTargetFunc = FollowTargetVer;
                cameraFollowMode = CameraFollowMode.FollowVertical;
                // /print("Matching path: "+pos+", "+activeRange.Min+" - "+activeRange.Max);
                return;
            }
            else continue;
        }
        RepositionObjActivator(ActivatorSize,pos);
        #if UNITY_EDITOR
        print("No matching paths found");
        #endif
    }
    public void StaticTransition(Vector3Int tilePos, bool special)
    {
        Vector3 direction = (Vector3)(tilePos-curCamCell);
        Vector3 targetEndPos = moveWithCameraEvent.Invoke(direction);
        if(targetEndPos == Vector3.up*-999)
        {
            PlayerControl.freezeJump = false;
            cameraTransition = null;
            return;
        }

        activeRange = null;
        cameraFollowMode = CameraFollowMode.Static;
        followTargetFunc = null;

        if(cameraTransition!=null)StopCoroutine(cameraTransition);

        Vector3 targetWorldPos = calculateWorldPos.Invoke(tilePos);
        ///print("Target world pos: "+targetWorldPos +" target tile pos: "+tilePos);

        cameraTransition = StartCoroutine(ICameraTransition(targetWorldPos,tilePos,special,targetEndPos));
        curCamCell = tilePos;

    }
}
