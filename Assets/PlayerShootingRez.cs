using UnityEngine;

public class PlayerShootingRez : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform model; // The entire player model that rotates
    [SerializeField] private Transform target; // The GameObject used as the aiming reference
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftArmFirePoint;
    [SerializeField] private Transform rightArmFirePoint;

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float shootingCooldown = 0.15f;

    private float lastShotTime = 0f;
    private bool isLeftArm = true;

    void Update()
    {
        RotateModelToMouse();

        if (Input.GetButton("Fire1") && Time.time - lastShotTime >= shootingCooldown)
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    private void RotateModelToMouse()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition(); // Always get a valid position

        // Compute direction toward the mouse position on the target or fallback
        Vector3 lookDirection = (mouseWorldPosition - model.position).normalized;

        // Apply rotation
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            model.rotation = Quaternion.Slerp(model.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void Shoot()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition(); // Always get a valid position

        // Compute shooting direction toward the target or fallback
        Vector3 shootDirection = (mouseWorldPosition - GetFirePoint().position).normalized;

        // Instantiate and fire the bullet
        GameObject bullet = Instantiate(bulletPrefab, GetFirePoint().position, Quaternion.LookRotation(shootDirection));
        bullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * bulletSpeed;

        // Alternate between left and right arm
        isLeftArm = !isLeftArm;
    }

    private Transform GetFirePoint()
    {
        return isLeftArm ? leftArmFirePoint : rightArmFirePoint;
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Create a ray from the mouse cursor into world space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Raycast against the target's collider
        if (target != null && target.TryGetComponent<Collider>(out Collider targetCollider))
        {
            if (targetCollider.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                return hit.point; // Get the exact 3D world position on the target GameObject
            }
        }

        // If no valid hit, fallback to shooting in the general direction of the target
        return ray.GetPoint(100); // Project ray 100 units forward
    }
}