using System.Collections;
using UnityEngine;

public class Transition : MonoBehaviour
{
    float Idle = -480;
    float offset = 40;
    public float transitionSpeed = 5f;
    public static Vector3 offsetVector = new Vector3(480,270,10);
    static GameObject canvas;
    static RectTransform maskTransform;
    static Transition self;
    static Coroutine transitionCor;
    Transform camTransform;
    IEnumerator ITransition(bool appear)
    {
        this.enabled = true;
        Vector2 startPos = maskTransform.anchoredPosition;
        if(appear) startPos.x = Idle + offset;

        Vector2 endPos = startPos;

        endPos.x = Idle+(appear ? 0 : -offset);
        float progress = 0;

        if(appear)canvas.SetActive(true);

        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*transitionSpeed;
            float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
            maskTransform.anchoredPosition = Vector3.Lerp(startPos,endPos,mathStep);
            yield return 0;
        }
        maskTransform.anchoredPosition = endPos;
        if(!appear)canvas.SetActive(false);
        this.enabled = false;
    }
    void LateUpdate()
    {
        if(camTransform != null)
        {
           ///print("Cam transform: "+camTransform.name +" "+ canvas.transform.position);
            canvas.transform.position = camTransform.position+offsetVector;
        }
        ///else print("Cam transform: null "+ canvas.transform.position);
    }
    void Init()
    {
        canvas = transform.GetChild(0).gameObject;
        maskTransform = canvas.transform.GetChild(0).GetComponent<RectTransform>();
        self = this;
        this.enabled = false;
    }
    public static void TransitionEvent(bool appear)
    {
        if(transitionCor!=null) self.StopCoroutine(transitionCor);
        transitionCor = self.StartCoroutine(self.ITransition(appear));
    }
    public static void TransitionEvent(bool appear, Camera camera)
    {
        canvas.GetComponent<Canvas>().worldCamera = camera;
        canvas.transform.position = camera.transform.position+offsetVector;
        if(transitionCor!=null) self.StopCoroutine(transitionCor);
        transitionCor = self.StartCoroutine(self.ITransition(appear));
    }
    public static void RePositionCamera(Camera camera, Vector3 position)
    {
        if(canvas == null) return;
        ///print("Reposition camera: "+camera.transform.name+" "+camera.transform.position);
        canvas.GetComponent<Canvas>().worldCamera = camera;
        canvas.transform.position = position;
        self.camTransform = camera.transform;
    }
}
