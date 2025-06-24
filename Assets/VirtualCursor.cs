using UnityEngine;

public class VirtualCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    public RectTransform cursor;
    public float speed = 1000f;

    [Header("Integration")]
    public bool hideSystemCursor = true;
    public bool enableMouseInput = true;
    public float mouseThreshold = 5f; // Minimum mouse movement to register

    [Header("Joystick Override Settings")]
    public float joystickThreshold = 0.2f;     // Minimum input to consider joystick active
    public float joystickTimeout = 0.3f;       // Seconds before mouse can resume after joystick

    Vector2 virtualCursorPos;
    Canvas parentCanvas;
    Camera uiCamera;

    public static VirtualCursor Instance { get; private set; }

    private bool joystickRecentlyUsed = false;
    private float joystickTimer = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        virtualCursorPos = cursor.anchoredPosition;

        parentCanvas = cursor.GetComponentInParent<Canvas>();
        uiCamera = parentCanvas.worldCamera;

        if (hideSystemCursor)
        {
            Cursor.visible = false;
        }

        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        HandleJoystickInput();
        HandleMouseInput();

        // Clamp to screen bounds
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        virtualCursorPos.x = Mathf.Clamp(virtualCursorPos.x, -canvasWidth / 2, canvasWidth / 2);
        virtualCursorPos.y = Mathf.Clamp(virtualCursorPos.y, -canvasHeight / 2, canvasHeight / 2);

        cursor.anchoredPosition = virtualCursorPos;
    }

    private void HandleJoystickInput()
    {
        float moveX = Input.GetAxis("HorizontalRightJoystick");
        float moveY = Input.GetAxis("VerticalRightJoystick");

        Vector2 input = new Vector2(moveX, moveY);

        if (input.magnitude > 0.01f)
        {
            joystickRecentlyUsed = true;
            joystickTimer = joystickTimeout;
            virtualCursorPos += input * speed * Time.deltaTime;
        }
        else
        {
            if (joystickTimer > 0f)
            {
                joystickTimer -= Time.deltaTime;
            }
            else
            {
                joystickRecentlyUsed = false;
            }
        }
    }

    private void HandleMouseInput()
    {
        if (!enableMouseInput || joystickRecentlyUsed)
            return;

        Vector2 mouseScreenPos = Input.mousePosition;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.GetComponent<RectTransform>(),
            mouseScreenPos,
            uiCamera,
            out Vector2 mouseCanvasPos))
        {
            Vector2 mouseDelta = mouseCanvasPos - virtualCursorPos;
            if (mouseDelta.magnitude > mouseThreshold)
            {
                virtualCursorPos = mouseCanvasPos;
            }
        }
    }

    public Vector3 GetScreenPosition()
    {
        Vector3 worldPos = cursor.TransformPoint(Vector3.zero);
        return uiCamera != null ? uiCamera.WorldToScreenPoint(worldPos) : worldPos;
    }

    public Vector3 GetWorldPosition(Camera camera, float distance = 10f)
    {
        Vector3 screenPos = GetScreenPosition();
        return camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));
    }

    void OnDestroy()
    {
        if (hideSystemCursor)
        {
            Cursor.visible = true;
        }
        Cursor.lockState = CursorLockMode.None;

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
