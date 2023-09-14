using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MoveToPoint : MonoBehaviour
{
    public Vector2 targetPoint = Vector2.zero;
    [SerializeField] bool playOnEnable = true;
    public float speed = 1;
    Coroutine Movement;

    protected float XMultiplier = 1;
    protected float YMultiplier = 1;
    protected float progress = 0;
    protected delegate void UpdateEvent();
    protected UpdateEvent updateEvent;
    public float Progress {get {return progress;}}

    [Space]
    [SerializeField]
    UnityEvent endEvent;
    IEnumerator IMovement()
    {
        progress = 0;
        Vector3 startPos = transform.position;
        Vector3 pos = startPos;
        
        while(progress<1)
        {
            progress += Time.deltaTime * speed;
            updateEvent?.Invoke();
            pos.x = Mathf.Lerp(startPos.x,targetPoint.x*XMultiplier,progress);
            pos.y = Mathf.Lerp(startPos.y,targetPoint.y*YMultiplier,progress);
            pos.z = startPos.z;
            transform.position = pos;
            yield return 0;
        }
        transform.position = pos;
        endEvent?.Invoke();
    }
    public void Play()
    {
        Movement = StartCoroutine(IMovement());
    }
    protected virtual void OnEnable()
    {
        if(playOnEnable)
        Play();
    }
    void OnDisable()
    {
        if(Movement != null)StopCoroutine(Movement);
    }

    #if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        // Draws a blue line from this transform to the target
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, (targetPoint));
    }
    #endif
}
