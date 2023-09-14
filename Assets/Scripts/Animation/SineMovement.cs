using UnityEngine;

public class SineMovement : MonoBehaviour
{
    public float sineSpeed = 1;
    public float sineAmplitude = 1;
    public float heightOffset = 0;
    float sineProgress = 0;
    [SerializeField] bool affectedByMovingBgs = false;

    // Update is called once per frame
    void Update()
    {
        if(affectedByMovingBgs && !DataShare.movingBGs) return;
        Vector3 pos = transform.localPosition;

        sineProgress = Mathf.Repeat(sineProgress+(Time.deltaTime*sineSpeed),Mathf.PI*2);
        pos.y = Mathf.Sin(sineProgress)*sineAmplitude + heightOffset;

        transform.localPosition = pos;
    }
}
