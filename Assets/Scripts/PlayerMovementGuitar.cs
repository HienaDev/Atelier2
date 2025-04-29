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

    private void Awake()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        HandleMovementInput();
        HandleDashInput();

        dashCooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            HandleDash();
        }
        else
        {
            HandleMovement();
        }
    }

    private void HandleMovementInput()
    {
        float moveZ = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(0f, moveY, moveZ).normalized;
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = rb.position + moveDirection * movSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
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
        Vector3 dashPosition = rb.position + moveDirection * dashSpeed * Time.fixedDeltaTime;
        rb.MovePosition(dashPosition);

        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0f)
        {
            isDashing = false;
        }
    }
}