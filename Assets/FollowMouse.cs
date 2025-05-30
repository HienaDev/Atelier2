using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    RectTransform rectTransform;
    Camera uiCamera;

    void Start()
    {
        Cursor.visible = false; // Hide the cursor if desired
        rectTransform = GetComponent<RectTransform>();
        uiCamera = Camera.main; // or assign your UI camera manually
    }

    void Update()
    {
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            Input.mousePosition,
            uiCamera,
            out mousePos
        );

        rectTransform.localPosition = mousePos;
    }
}
