using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class CurveLifetime : MonoBehaviour
{
    CurveAnimator[] curveElements;
    [Range(1,10)]
    [SerializeField] float CurveProgressionSpeed;
    [SerializeField] float progression;
    [SerializeField] bool DestroyOnEnd = false;

    float Progression()
    {
        return progression;
    }
    Coroutine lifeCor;
    IEnumerator ILife()
    {
        yield return 0;
        while(progression<1)
        {
            progression += Time.deltaTime*CurveProgressionSpeed;
            yield return 0;
        }
        progression = 1;
        if(DestroyOnEnd) Destroy(gameObject);
    }
    void Awake()
    {
        curveElements = GetComponents<CurveAnimator>();
        foreach (var item in curveElements)
        {
            item.progression = Progression;
        }
    }
    void OnEnable()
    {
        progression = 0;
        #if UNITY_EDITOR
        if(Application.isPlaying)
        #endif
        lifeCor = StartCoroutine(ILife());
    }
    void OnDisable()
    {
        #if UNITY_EDITOR
        if(Application.isPlaying)
        #endif
        StopCoroutine(lifeCor);
    }
}
