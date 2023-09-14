using UnityEngine;

public class SineMovementUI : MonoBehaviour
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
    RectTransform rectTransform;

    void OnEnable()
    {
        if(changeStartPosOnEnable)
        {
            rectTransform = GetComponent<RectTransform>();
            startPos = rectTransform.anchoredPosition;
            startPosSet = true;
        }

    }
    void Start()
    {
        if(!changeStartPosOnEnable)
        {
            rectTransform = GetComponent<RectTransform>();
            startPos = rectTransform.anchoredPosition;
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

        rectTransform.anchoredPosition = startPos + pos;
    }
    void OnDisable()
    {
        if(returnToStartPosOnDisable)
        rectTransform.anchoredPosition = startPos;
    }
}
