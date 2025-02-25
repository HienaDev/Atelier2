using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private GameObject particleExplosion;

    private void OnCollisionEnter(Collision collision)
    {
        // Get collision point
        Vector3 hitPoint = collision.contacts[0].point;

        // Get the normal (perpendicular direction of the surface at impact)
        Vector3 impactNormal = collision.contacts[0].normal;

        // Make the particle system's Z-axis face the impact normal
        Quaternion rotation = Quaternion.LookRotation(impactNormal);

        // Instantiate the particle system with correct rotation
        Instantiate(particleExplosion, hitPoint, rotation);

        // Destroy the bullet on impact
        Destroy(gameObject);
    }
}
