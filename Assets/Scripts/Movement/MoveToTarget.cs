using UnityEngine;

public class MoveToTarget : MonoBehaviour
{
    public bool destroyOnEnd = true;
    public bool LerpX = true;
    public bool LerpY = false;
    public float Speed = 10;
    public bool LerpXFromStart = false;
    float progress = 0;
    Vector3 startPos;
    Vector3 targetPosition;
    public Transform target;

    // Initialized by outside script
    public void Init(Transform target)
    {
        targetPosition = target.position;
    }
    void OnEnable()
    {
        startPos = transform.position;
        if(target==null) targetPosition = transform.position;
    }
    void OnDisable()
    {
        progress = 0;
    }
    // Update is called once per frame
    void Update()
    {
        if(target!=null) targetPosition = target.position;
        // Step
        progress += Time.deltaTime*Speed;

        Vector3 pos = transform.position;

        if(LerpX)
        {
            if(LerpXFromStart)
            pos.x = Mathf.Lerp(startPos.x,targetPosition.x,progress);
            else pos.x = Mathf.Lerp(pos.x,targetPosition.x,progress);
        }
        if(LerpY) pos.y = Mathf.Lerp(startPos.y,targetPosition.y,progress);

        transform.position = pos;
        if(progress>=1 && destroyOnEnd)Destroy(this);
    }
}
