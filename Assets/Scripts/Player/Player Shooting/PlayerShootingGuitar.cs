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
        aimDirection.x = 0;
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
            topHalf.Rotate(0, 180, 0);
        }
    }

    private void Shoot()
    {
        playerSounds.PlayerShoot();
        Transform firePoint = isLeftArm ? leftArmFirePoint : rightArmFirePoint;
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector3 shootDirection = (mouseWorldPosition - transform.position).normalized;
        shootDirection.x = 0;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
        bullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * bulletSpeed;
        isLeftArm = !isLeftArm;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.right, transform.position);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || mainCamera == null) return;
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
        Vector3 center = transform.position;
        Vector3 size = new Vector3(0.01f, 10f, 10f);
        Gizmos.DrawCube(center, size);
        Vector3 mousePos = GetMouseWorldPosition();
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(mousePos, 0.2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, mousePos);
    }
}