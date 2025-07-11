using UnityEngine;

public class PlayerShootingQuark : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftArmFirePoint;
    [SerializeField] private Transform rightArmFirePoint;
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform topHalf;
    [SerializeField] private Camera mainCamera;

    [Header("Shooting Settings")]
    [SerializeField] private float shootingCooldown = 0.15f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float armRotationSpeed = 15f;

    private float lastShotTime = 0f;
    private bool isLeftArm = true;
    private bool isFacingRight = true;

    private void Start()
    {
        // Auto-assign camera if not set
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        AimArms();
        FlipBodyBasedOnAim();

        if ((Input.GetButton("Fire1") || Input.GetAxisRaw("LeftTrigger") == 1 || Input.GetAxisRaw("RightTrigger") == 1) && Time.time - lastShotTime >= shootingCooldown)
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    private void AimArms()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector3 aimDirection = (mouseWorldPosition - transform.position).normalized;
        aimDirection.x = 0; // Keep aiming in the Z-Y plane

        // Rotate arms towards the aim direction
        Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
        leftArm.rotation = Quaternion.Slerp(leftArm.rotation, targetRotation, Time.deltaTime * armRotationSpeed);
        rightArm.rotation = Quaternion.Slerp(rightArm.rotation, targetRotation, Time.deltaTime * armRotationSpeed);
    }

    private void FlipBodyBasedOnAim()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        bool shouldFaceRight = mouseWorldPosition.z > transform.position.z;

        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            topHalf.Rotate(0, 180, 0); // Instantly flip the body
        }
    }

    private void Shoot()
    {
        Transform firePoint = isLeftArm ? leftArmFirePoint : rightArmFirePoint;

        Vector3 shootDirection = (GetMouseWorldPosition() - firePoint.position).normalized;
        shootDirection.x = 0; // Ensure bullets move in the Z-Y plane

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
        bullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * bulletSpeed;

        isLeftArm = !isLeftArm; // Swap arms for next shot
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPosition;

        // Use virtual cursor if available, otherwise fallback to mouse
        if (VirtualCursor.Instance != null)
        {
            screenPosition = VirtualCursor.Instance.GetScreenPosition();
        }
        else
        {
            screenPosition = Input.mousePosition;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(Vector3.right, transform.position); // Plane aligned with Z-Y axis

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}