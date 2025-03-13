using UnityEngine;

public class PlayerShootingRez : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform model; // The entire player model that rotates
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
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        if (mouseWorldPosition == Vector3.zero) return;

        // Compute direction toward the mouse position in 3D space
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
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        if (mouseWorldPosition == Vector3.zero) return;

        // Compute shooting direction
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point; // Get exact 3D world position where the mouse is pointing
        }

        // If no collider is hit, project forward into the world
        return ray.GetPoint(100); // Adjust depth if necessary
    }
}