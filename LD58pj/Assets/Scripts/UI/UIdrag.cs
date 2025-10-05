using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using UnityEngine.EventSystems;

public class UIdrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    Vector2 defaultpos;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("鼠标按下了这个物体！");
        defaultpos = new Vector2(transform.position.x, transform.position.y);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 让物体跟随鼠标移动
        transform.position = eventData.position;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        // 让物体跟随鼠标移动
        transform.position = defaultpos;
    }
}
