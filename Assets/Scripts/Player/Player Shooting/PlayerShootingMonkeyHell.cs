using UnityEngine;

public class PlayerShootingMonkeyHell : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ArmRecoil leftArmRecoil;
    [SerializeField] private Transform leftArmFirePoint;
    [SerializeField] private ArmRecoil rightArmRecoil;
    [SerializeField] private Transform rightArmFirePoint;
    [SerializeField] private float shootingCooldown = 0.15f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private GameObject topHalf;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Aiming Settings")]
    [SerializeField] private Camera playerCamera; // Assign your main camera
    [SerializeField] private LayerMask aimingLayerMask = -1; // What layers can be aimed at
    [SerializeField] private float maxAimDistance = 100f; // Max distance for aiming raycast

    private bool isLeftArm = true;
    private float justShot = 0f;
    private PlayerSounds playerSounds;

    private void Start()
    {
        playerSounds = GetComponent<PlayerSounds>();

        // Auto-assign camera if not set
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        // Get target position using virtual cursor or mouse
        Vector3 targetPosition = GetAimPosition();

        // Get direction from object to the target point, but only on the X-Z plane
        Vector3 direction = targetPosition - topHalf.transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            // Create target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // Smoothly rotate towards the target
            topHalf.transform.rotation = Quaternion.Slerp(topHalf.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        if (Input.GetButton("Fire1") && Time.time - justShot >= shootingCooldown)
        {
            Shoot();
            justShot = Time.time;
        }
    }

    /// <summary>
    /// Gets the world position to aim at, using virtual cursor if available, otherwise mouse
    /// </summary>
    private Vector3 GetAimPosition()
    {
        Vector3 screenPosition;

        // Check if virtual cursor is available and active
        if (VirtualCursor.Instance != null)
        {
            screenPosition = VirtualCursor.Instance.GetScreenPosition();
        }
        else
        {
            // Fallback to regular mouse input
            screenPosition = Input.mousePosition;
        }

        // Convert screen position to world position using raycast
        Ray ray = playerCamera.ScreenPointToRay(screenPosition);

        // Try to hit something in the world
        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimingLayerMask))
        {
            return hit.point;
        }
        else
        {
            // If no hit, project forward from camera
            return ray.origin + ray.direction * maxAimDistance;
        }
    }

    /// <summary>
    /// Alternative method if you want to use a fixed Y plane for aiming
    /// </summary>
    private Vector3 GetAimPositionOnPlane(float yPlane = 0f)
    {
        Vector3 screenPosition;

        if (VirtualCursor.Instance != null)
        {
            screenPosition = VirtualCursor.Instance.GetScreenPosition();
        }
        else
        {
            screenPosition = Input.mousePosition;
        }

        Ray ray = playerCamera.ScreenPointToRay(screenPosition);

        // Calculate intersection with Y plane
        if (ray.direction.y != 0)
        {
            float distance = (yPlane - ray.origin.y) / ray.direction.y;
            if (distance > 0)
            {
                return ray.origin + ray.direction * distance;
            }
        }

        // Fallback if can't intersect with plane
        return ray.origin + ray.direction * maxAimDistance;
    }

    public void Shoot()
    {
        playerSounds.PlayerShoot();

        if (isLeftArm)
        {
            GameObject bullet = Instantiate(bulletPrefab, leftArmFirePoint.position, leftArmFirePoint.rotation);
            bullet.GetComponent<Rigidbody>().linearVelocity = bullet.transform.forward * bulletSpeed;
            leftArmRecoil.ApplyRecoil();
            isLeftArm = false;
        }
        else
        {
            GameObject bullet = Instantiate(bulletPrefab, rightArmFirePoint.position, rightArmFirePoint.rotation);
            bullet.GetComponent<Rigidbody>().linearVelocity = bullet.transform.forward * bulletSpeed;
            rightArmRecoil.ApplyRecoil();
            isLeftArm = true;
        }
    }
}