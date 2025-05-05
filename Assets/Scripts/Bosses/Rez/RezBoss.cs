using UnityEngine;

public class RezBoss : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform[] firePoints; // Multiple fire points
    [SerializeField] private Transform player; // Reference to the player

    [Header("Shooting Settings")]
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float shootingCooldown = 2f;

    private float lastShotTime = 0f;

    void Update()
    {
        ShootProjectile();
    }

    private void ShootProjectile()
    {
        if (Time.time - lastShotTime >= shootingCooldown && firePoints.Length > 0)
        {
            if (player == null) return;

            // Select a random firePoint
            Transform selectedFirePoint = firePoints[Random.Range(0, firePoints.Length)];

            // Calculate direction toward the player
            Vector3 shootDirection = (player.position - selectedFirePoint.position).normalized;

            // Instantiate projectile and set rotation
            GameObject projectile = Instantiate(projectilePrefab, selectedFirePoint.position, Quaternion.LookRotation(shootDirection));
            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            // Ensure no gravity & apply velocity
            rb.useGravity = false;
            rb.linearVelocity = shootDirection * projectileSpeed;

            // Destroy projectile after a set time
            Destroy(projectile, shootingCooldown);

            lastShotTime = Time.time; // Reset cooldown
        }
    }
}