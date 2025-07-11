using UnityEngine;

public class VirtualCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    public RectTransform cursor;
    public float speed = 1000f;

    [Header("Integration")]
    public bool hideSystemCursor = true;

    [Header("Input Detection")]
    public float mouseThreshold = 5f; // Minimum mouse movement to register
    public float joystickThreshold = 0.2f; // Minimum input to consider joystick active
    public float inputSwitchDelay = 0.3f; // Delay before switching input types

    Vector2 virtualCursorPos;
    Canvas parentCanvas;
    Camera uiCamera;

    public static VirtualCursor Instance { get; private set; }

    // Input state tracking
    private bool isUsingMouse = false;
    private bool isUsingController = false;
    private float inputSwitchTimer = 0f;
    private Vector2 lastMousePosition;

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

        // Initialize last mouse position
        lastMousePosition = Input.mousePosition;
    }

    void Update()
    {

        Debug.Log("Test joystick: " + Input.GetAxisRaw("LeftTrigger") + Input.GetAxisRaw("RightTrigger"));
        DetectInputType();

        if (isUsingController)
        {
            HandleJoystickInput();
        }
        else if (isUsingMouse)
        {
            HandleMouseInput();
        }

        // Clamp to screen bounds
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        virtualCursorPos.x = Mathf.Clamp(virtualCursorPos.x, -canvasWidth / 2, canvasWidth / 2);
        virtualCursorPos.y = Mathf.Clamp(virtualCursorPos.y, -canvasHeight / 2, canvasHeight / 2);

        cursor.anchoredPosition = virtualCursorPos;
    }

    private void DetectInputType()
    {
        // Check for controller input (both keyboard IJKL and actual controller joystick)
        float moveX = Input.GetAxisRaw("HorizontalRightJoystick") + Input.GetAxisRaw("HorizontalRightJoystickController");
        float moveY = Input.GetAxisRaw("VerticalRightJoystick") + Input.GetAxisRaw("VerticalRightJoystickController");
        Vector2 joystickInput = new Vector2(moveX, moveY);

        // Check for mouse movement
        Vector2 currentMousePosition = Input.mousePosition;
        Vector2 mouseDelta = currentMousePosition - lastMousePosition;

        // Update timer
        if (inputSwitchTimer > 0f)
        {
            inputSwitchTimer -= Time.unscaledDeltaTime;
        }

        // Detect controller input
        if (joystickInput.magnitude > joystickThreshold)
        {
            if (!isUsingController && inputSwitchTimer <= 0f)
            {
                isUsingController = true;
                isUsingMouse = false;
                inputSwitchTimer = inputSwitchDelay;
                Debug.Log("Switched to Controller input");
            }
        }

        // Detect mouse input
        if (mouseDelta.magnitude > mouseThreshold)
        {
            if (!isUsingMouse && inputSwitchTimer <= 0f)
            {
                isUsingMouse = true;
                isUsingController = false;
                inputSwitchTimer = inputSwitchDelay;
                Debug.Log("Switched to Mouse input");
            }
        }

        // If no input detected for a while, allow switching
        if (joystickInput.magnitude <= joystickThreshold && mouseDelta.magnitude <= mouseThreshold)
        {
            // Keep current input type but allow switching if timer expires
        }

        lastMousePosition = currentMousePosition;
    }

    private void HandleJoystickInput()
    {
        // Combine both keyboard (IJKL) and controller joystick input
        float moveX = Input.GetAxisRaw("HorizontalRightJoystick") + Input.GetAxisRaw("HorizontalRightJoystickController");
        float moveY = Input.GetAxisRaw("VerticalRightJoystick") + Input.GetAxisRaw("VerticalRightJoystickController");
        Vector2 input = new Vector2(moveX, moveY);

        if (input.magnitude > 0.01f)
        {
            virtualCursorPos += input * speed * Time.unscaledDeltaTime;
        }
    }

    private void HandleMouseInput()
    {
        Vector2 mouseScreenPos = Input.mousePosition;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.GetComponent<RectTransform>(),
            mouseScreenPos,
            uiCamera,
            out Vector2 mouseCanvasPos))
        {
            virtualCursorPos = mouseCanvasPos;
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

    // Public getters for input state (useful for UI feedback)
    public bool IsUsingMouse => isUsingMouse;
    public bool IsUsingController => isUsingController;

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