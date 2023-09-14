using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenWrap : MonoBehaviour
{
    Vector2 XRange,YRange;
    // Start is called before the first frame update
    void Start()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        Vector2 pos = transform.position;

        XRange = Vector2.one * col.size.x * transform.localScale.x /2;
        XRange.x *= -1;
        XRange.x -= pos.x;
        XRange.y += pos.x;

        YRange = Vector2.one * col.size.y * transform.localScale.y /2;
        YRange.x *= -1;
        YRange.x -= pos.y;
        YRange.y += pos.y;
    }
    void OnTriggerExit2D(Collider2D other)
    {
        Transform tr = other.transform;
        Vector3 pos = tr.position;

        if(pos.x <= XRange.x) pos.x += XRange.y*2;
        else if(pos.x >= XRange.y) pos.x += XRange.x*2;

        if(pos.y <= YRange.x) pos.y += YRange.y*2;
        else if(pos.y >= YRange.y) pos.y += YRange.x*2;

        tr.position = pos;
    }
}
