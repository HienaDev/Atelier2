using UnityEngine;

public class PlayerShootingQuark : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftArmFirePoint;
    [SerializeField] private Transform rightArmFirePoint;
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform model; // Model that flips
    [SerializeField] private Camera mainCamera;

    private bool isLeftArm = true;
    private bool isFacingRight = true;

    [SerializeField] private float shootingCooldown = 0.15f;
    private float lastShotTime = 0f;

    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float armRotationSpeed = 15f;

    void Update()
    {
        AimArms();
        FlipBody();

        if (Input.GetButton("Fire1") && Time.time - lastShotTime >= shootingCooldown)
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    private void AimArms()
    {
        // Get the mouse world position
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector3 aimDirection = (mouseWorldPosition - leftArm.position).normalized;
        aimDirection.x = 0; // Keep aiming in the Z-Y plane

        // Rotate arms to follow the mouse cursor
        Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
        leftArm.rotation = Quaternion.Slerp(leftArm.rotation, targetRotation, Time.deltaTime * armRotationSpeed);
        rightArm.rotation = Quaternion.Slerp(rightArm.rotation, targetRotation, Time.deltaTime * armRotationSpeed);
    }

    private void FlipBody()
    {
        // Check if the cursor is on the left or right side of the player
        bool shouldFaceRight = GetMouseWorldPosition().z > model.position.z;

        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            model.Rotate(0, 180, 0); // Instantly flip the body
        }
    }

    private void Shoot()
    {
        // Get the fire point (alternate arms)
        Transform firePoint = isLeftArm ? leftArmFirePoint : rightArmFirePoint;

        // Get shooting direction
        Vector3 shootDirection = (GetMouseWorldPosition() - firePoint.position).normalized;
        shootDirection.x = 0; // Ensure bullets move in the Z-Y plane

        // Instantiate and fire the bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
        bullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * bulletSpeed;

        // Alternate arms
        isLeftArm = !isLeftArm;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.right, Vector3.zero); // Plane aligned with Z-Y axis

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}