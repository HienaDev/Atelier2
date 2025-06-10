using UnityEngine;

public class CursorFollower : MonoBehaviour
{
    private RectTransform rectTransform;
    private Canvas canvas;

    void Start()
    {
        // Get references
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // Hide system cursor
        Cursor.visible = false;
    }

    void Update()
    {
        // Convert mouse position to UI space
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out mousePos
        );

        rectTransform.anchoredPosition = mousePos;
    }
}
