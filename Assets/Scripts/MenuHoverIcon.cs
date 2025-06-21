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
            hoverIcon.position = (transform as RectTransform).position + (Vector3)offset;
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