using UnityEngine;

/// <summary>
/// Move the camera
/// </summary>
public class MoveCamera : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float shiftSpeedScale;

    [SerializeField] private KeyCode left = KeyCode.A;
    [SerializeField] private KeyCode right = KeyCode.D;
    [SerializeField] private KeyCode down = KeyCode.E;
    [SerializeField] private KeyCode up = KeyCode.Q;
    [SerializeField] private KeyCode back = KeyCode.S;
    [SerializeField] private KeyCode foward = KeyCode.W;

    [SerializeField] private float verticalMouseSensitivity;
    [SerializeField] private float horizontalMouseSensitivity;

    private float     shiftSpeed;
    private float     defaultSpeed;
    private Vector3   velocity;
    private Rigidbody rb;
    private Transform cam;

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        cam = Camera.main.transform;

        shiftSpeed = speed * shiftSpeedScale;
        defaultSpeed = speed;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        Move();
        RotateCamera();
    }

    /// <summary>
    /// Move the camera
    /// </summary>
    private void Move()
    {
        velocity = Vector3.zero;

        if (Input.GetKey(KeyCode.LeftShift)) speed = shiftSpeed;
        else speed = defaultSpeed;

        if (Input.GetKey(left)) velocity.x = -speed;
        if (Input.GetKey(right)) velocity.x = speed;


        if (Input.GetKey(back)) velocity.z = -speed;
        if (Input.GetKey(foward)) velocity.z = speed;


        if (Input.GetKey(up)) velocity.y = speed;
        if (Input.GetKey(down)) velocity.y = -speed;

        transform.position += cam.TransformDirection(velocity) * Time.deltaTime;
    }

    /// <summary>
    /// Rotate the camera
    /// </summary>
    private void RotateCamera()
    {
        if (Input.GetMouseButton(1))
        {
            HideCursor();

            Vector3 rotation = cam.localEulerAngles;
   
            rotation.x -= Input.GetAxis("Mouse Y") * verticalMouseSensitivity;
            rotation.y += Input.GetAxis("Mouse X") * horizontalMouseSensitivity;

            cam.localEulerAngles = rotation;
        }
        else
        {
            ShowCursor();
        }
    }
    
    /// <summary>
    /// Hide the cursor
    /// </summary>
    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Show the cursor
    /// </summary>
    public void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Lock the cursor in the center
    /// </summary>
    public void LockCursorCenter()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }
}
