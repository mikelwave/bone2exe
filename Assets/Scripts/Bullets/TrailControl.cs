using System.Collections;
using UnityEngine;

[RequireComponent (typeof(TrailRenderer))]
public class TrailControl : MonoBehaviour
{
    IEnumerator ITrailEnable()
    {
        yield return 0;
        trail.enabled = true;
    }
    TrailRenderer trail;
    void Start()
    {
        if(trail == null)
        trail = GetComponent<TrailRenderer>();
    }
    void OnDisable()
    {
        if(trail!=null)
        trail.Clear();
    }
    void OnEnable()
    {
        if(trail == null)Start();
        trail.enabled = false;
        StartCoroutine(ITrailEnable());
    }
}
