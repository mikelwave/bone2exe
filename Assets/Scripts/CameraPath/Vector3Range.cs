using UnityEngine;

public class Vector3Range
{
    Vector3 min;
    Vector3 max;

    public Vector3 Min { get{ return min; } set { min = value;}}
    public Vector3 Max { get{ return max; } set { max = value;}}
    public Vector3Range(Vector3 min, Vector3 max)
    {
        this.min = min;
        this.max = max;
    }
    public Vector3Range()
    {
        this.min = Vector3.zero;
        this.max = Vector3.zero;
    }
}