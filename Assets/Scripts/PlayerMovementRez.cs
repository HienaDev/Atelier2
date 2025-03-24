using UnityEngine;

public class PlayerMovementRez : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float boostSpeed = 20f;
    [SerializeField] private float boostDuration = 0.5f;
    [SerializeField] private float boostCooldown = 2f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isBoosting = false;
    private float lastBoostTime = -100f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleMovement();
        HandleBoost();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); // Left (-1) / Right (1)
        float moveY = Input.GetAxisRaw("Vertical");   // Up (1) / Down (-1)

        // Restrict movement to the Y-X plane (no Z movement)
        moveDirection = new Vector3(moveX, moveY, 0).normalized;

        // Apply movement with linear velocity (Unity 6 requirement)
        float speed = isBoosting ? boostSpeed : moveSpeed;
        rb.linearVelocity = moveDirection * speed;
    }

    private void HandleBoost()
    {
        if (Input.GetButtonDown("Jump") && Time.time - lastBoostTime > boostCooldown)
        {
            StartCoroutine(Boost());
        }
    }

    private System.Collections.IEnumerator Boost()
    {
        isBoosting = true;
        lastBoostTime = Time.time;
        yield return new WaitForSeconds(boostDuration);
        isBoosting = false;
    }
}
