using UnityEngine;

public class PlayerMovementGuitar : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float movSpeed = 5f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    private Vector3 moveDirection;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    void Awake()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (!isDashing)
        {
            HandleMovement();
            HandleDashInput();
        }
        else
        {
            HandleDash();
        }

        dashCooldownTimer -= Time.deltaTime;
    }

    private void HandleMovement()
    {
        float moveZ = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(0, moveY, moveZ).normalized;

        transform.position += moveDirection * movSpeed * Time.deltaTime;
    }

    private void HandleDashInput()
    {
        if (Input.GetButtonDown("Jump") && dashCooldownTimer <= 0f && moveDirection != Vector3.zero)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
        }
    }

    private void HandleDash()
    {
        transform.position += moveDirection * dashSpeed * Time.deltaTime;
        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            isDashing = false;
        }
    }
}