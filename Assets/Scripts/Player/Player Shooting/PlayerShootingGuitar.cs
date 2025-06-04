using UnityEditor;
using UnityEngine;

public class PlayerShootingGuitar : MonoBehaviour
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

    private PlayerSounds playerSounds;

    private Vector3 mousePosition;


    private void Start()
    {
        playerSounds = GetComponent<PlayerSounds>();
    }

    void Update()
    {
        AimArms(); 
        FlipBodyBasedOnAim();

        if (Input.GetButton("Fire1") && Time.time - lastShotTime >= shootingCooldown)
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

        playerSounds.PlayerShoot();
        Transform firePoint = isLeftArm ? leftArmFirePoint : rightArmFirePoint;

        Vector3 shootDirection = (GetMouseWorldPosition() - firePoint.position).normalized;
        shootDirection.x = 0; // Ensure bullets move in the Z-Y plane

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
        bullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * bulletSpeed;

        isLeftArm = !isLeftArm; // Swap arms for next shot
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.right, transform.position); // Plane aligned with Z-Y axis



        if (plane.Raycast(ray, out float distance))
        {
            mousePosition = ray.GetPoint(distance);
            Debug.Log(ray.GetPoint(distance));
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(mousePosition, 1f);


        // Draw the Z-Y plane at the player's position
        Vector3 center = transform.position;
        Vector3 up = Vector3.up;
        Vector3 forward = Vector3.forward;
        float planeSize = 5f;

        Vector3 topLeft = center + up * planeSize + forward * planeSize;
        Vector3 topRight = center + up * planeSize - forward * planeSize;
        Vector3 bottomLeft = center - up * planeSize + forward * planeSize;
        Vector3 bottomRight = center - up * planeSize - forward * planeSize;

        Handles.color = new Color(0f, 0.5f, 1f, 0.2f); // semi-transparent cyan
        Handles.DrawAAConvexPolygon(topLeft, topRight, bottomRight, bottomLeft);
        Handles.color = Color.cyan;
        Handles.DrawLine(topLeft, topRight);
        Handles.DrawLine(topRight, bottomRight);
        Handles.DrawLine(bottomRight, bottomLeft);
        Handles.DrawLine(bottomLeft, topLeft);

    }

}