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
        var canvasTransform = GetComponentInParent<Canvas>().transform;
        transform.SetParent(canvasTransform, true);
        transform.SetAsLastSibling();
        defaultpos = new Vector2(transform.position.x, transform.position.y);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 让物体跟随鼠标移动
        transform.position = eventData.position;
    }
    public void OnPointerUp(PointerEventData eventData)

    {
       
       checkandSwap();
       // transform.position = defaultpos;
    }


    private List<Transform> getabilitySlotParents()
    {
        return UIManeger.Instance.abilitySlotParents;
    }

    //检查是否碰到了其他其他槽位
    private bool checkcollision(Transform slot)
    {
        RectTransform rect1 = GetComponent<RectTransform>();
        RectTransform rect2 = slot.GetComponent<RectTransform>();

        return RectTransformUtility.RectangleContainsScreenPoint(rect2, rect1.position, null);
    }

    //检查是否碰到了其他槽位，并且交换位置
    private void checkandSwap()
    {
        List<Transform> slots = getabilitySlotParents();
        foreach (Transform slot in slots)
        {
            if (checkcollision(slot))
            {
                //记录当前物体的父物体
                Transform originalParent = transform.parent;
                //如果槽位中已经有物体了，就交换位置
                if (slot.childCount > 0)
                {
                    Transform other = slot.GetChild(0);
                    Transform otherOriginalParent = other.parent;
                    other.SetParent(originalParent, true);
                    other.position = originalParent.position;
                    transform.SetParent(slot, true);
                    transform.position = slot.position;
                    return;
                }
                else
                {
                    //如果槽位中没有物体，就直接放进去
                    transform.SetParent(slot, true);
                    transform.position = slot.position;
                    return;
                }
            }
        }
        //如果没有碰到任何槽位，回到原来的位置
        transform.position = defaultpos;
    }
}
