using UnityEngine;

public class Shake : MonoBehaviour
{
    public void Set(float XRadius, float YRadius, float delay)
    {
        this.XRadius = XRadius;
        this.YRadius = YRadius;
        this.delay = delay;
    }
    Vector3 origin, newPos;
    [SerializeField] float XRadius = 1;
    [SerializeField] float YRadius = 1;
    [SerializeField] float delay = 0;
    float delta = 0;
    void SetNewPos()
    {
        const float DoublePI = Mathf.PI*2;
        newPos = origin +
        new Vector3(Mathf.Sin(DoublePI*Random.value)*XRadius,Mathf.Cos(DoublePI*Random.value)*YRadius,0);
    }
    // Start is called before the first frame update
    void Start()
    {
        origin = transform.localPosition;
        SetNewPos();
    }

    // Update is called once per frame after all other updates
    void LateUpdate()
    {
        delta+=Time.deltaTime;
        if(delta>=delay)
        {
            delta = 0;
            SetNewPos();
        }
        else if (delta>=delay/2) // Return to normal pos
        {
            newPos = origin;
        }
        transform.localPosition = newPos;
    }
    void OnDestroy()
    {
        transform.localPosition = origin;
    }
}
