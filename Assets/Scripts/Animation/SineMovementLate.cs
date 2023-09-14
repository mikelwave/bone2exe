using UnityEngine;

public class SineMovementLate : MonoBehaviour
{
    public float sineSpeed = 1;
    public float sineAmplitude = 1;
    public float heightOffset = 0;
    public float startDelay = 0;
    float sineProgress = 0;
    Vector3 startPos = Vector3.zero;
    bool startPosSet = false;
    [SerializeField] bool returnToStartPosOnDisable = false;
    [SerializeField] bool changeStartPosOnEnable = false;

    void OnEnable()
    {
        if(changeStartPosOnEnable)
        {
            startPos = transform.localPosition;
            startPosSet = true;
        }

    }
    void Start()
    {
        if(!changeStartPosOnEnable)
        {
            startPos = transform.localPosition;
            startPosSet = true;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(!startPosSet) return;
        Vector3 pos = Vector3.zero;

        sineProgress = Mathf.Repeat(startDelay+sineProgress+(Time.deltaTime*sineSpeed),Mathf.PI*2);
        pos.y = Mathf.Sin(sineProgress)*sineAmplitude + heightOffset;

        transform.localPosition = startPos + pos;
    }
    void OnDisable()
    {
        if(returnToStartPosOnDisable)
        transform.localPosition = startPos;
    }
}
