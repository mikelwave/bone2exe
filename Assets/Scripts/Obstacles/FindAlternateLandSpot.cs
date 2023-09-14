using UnityEngine;
using System.Collections;

[RequireComponent (typeof (MoveToPointCurve))]
public class FindAlternateLandSpot : MonoBehaviour
{
    [Tooltip ("The chance to pick alternate landing spot above destination point")]
    [Range (0,10)]
    [SerializeField] int alternateGroundPickChance = 5;
    MoveToPointCurve moveToPoint;

    [SerializeField] AnimationCurve alternatePathCurve;
    IEnumerator IFindAlternateSpot()
    {
        yield return 0;
        RaycastHit2D hit = Physics2D.Raycast(moveToPoint.targetPoint+Vector2.up*0.5f,Vector2.up,8,128);

        if(hit.collider != null)
        {
            Debug.DrawLine(moveToPoint.targetPoint,hit.point,Color.green,2f);
            moveToPoint.targetPoint.y = Mathf.Floor(hit.point.y) + 1.5f;
            moveToPoint.YMultiplierCurve = alternatePathCurve;
            yield break;
        }
        Debug.DrawLine(moveToPoint.targetPoint,Vector2.up*10,Color.red,2f);
    }
    void OnEnable()
    {
        // If rolled value is higher than chance, ignore
        int value = Random.Range(0,10);
        if(value > alternateGroundPickChance) return;

        if(moveToPoint ==null) moveToPoint = GetComponent<MoveToPointCurve>();
        StartCoroutine(IFindAlternateSpot());
    }
}
