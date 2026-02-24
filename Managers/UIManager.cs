using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool MouseOverUI = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        MouseOverUI = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MouseOverUI = false;
    }
}
