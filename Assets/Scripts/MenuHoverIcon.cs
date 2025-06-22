using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform hoverIcon; // assign via inspector
    public Vector2 offset = new Vector2(10, 0); // offset from button

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverIcon != null)
        {
            hoverIcon.gameObject.SetActive(true);
            RectTransform buttonRect = transform as RectTransform;
            hoverIcon.SetParent(buttonRect.parent); // optional, just to match local space
            hoverIcon.anchoredPosition = buttonRect.anchoredPosition + offset;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverIcon != null)
        {
            hoverIcon.gameObject.SetActive(false);
        }
    }
}