using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Cursor : MonoBehaviour, IPointerDownHandler
{
    Camera mainCam;
    Canvas canvas;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Left click
        print("Left click");
    }
    // Start is called before the first frame update
    void Start()
    {
        canvas = transform.parent.GetComponent<Canvas>();
        mainCam = GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector2 pos;

        //Translate mouse position to world position
        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (
            canvas.transform as RectTransform,
            Mouse.current.position.ReadValue(),
            canvas.worldCamera,
            out pos
        );

        transform.position = canvas.transform.TransformPoint(pos);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        print(other.name + " Enter");
    }
    void OnTriggerExit2D(Collider2D other)
    {
        print(other.name + " Exit");
    }
}
