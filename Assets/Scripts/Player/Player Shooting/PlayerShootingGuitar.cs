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

    private void OnEnable()
    {
        topHalf.rotation = Quaternion.LookRotation(-transform.right);
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
        aimDirection.x = 0; // Still project onto the YZ plane

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.z) * Mathf.Rad2Deg;

        // Flip angle when facing left
        if (!isFacingRight)
        {
            angle = 180f - angle;
        }

        Quaternion targetRotation = Quaternion.AngleAxis(-angle, Vector3.forward); // Still rotate around Z

        leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, targetRotation, Time.deltaTime * armRotationSpeed);
        rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, targetRotation, Time.deltaTime * armRotationSpeed);
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

        Debug.DrawLine(firePoint.position, mouseWorldPosition, Color.green, 0.1f);

        Vector3 shootDirection = (mouseWorldPosition - firePoint.position).normalized;
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
}