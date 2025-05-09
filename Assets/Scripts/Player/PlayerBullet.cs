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

    private void OnTriggerEnter(Collider other)
    {
        DamageBoss damageBoss = other.GetComponent<DamageBoss>();

        //Debug.Log(other.name);
        if (damageBoss != null)
        {
            damageBoss.DealDamage(1);
        }

        BlowCollumnUp blowCollumnUp = other.GetComponent<BlowCollumnUp>();

        if (blowCollumnUp != null)
        {
            blowCollumnUp.DealDamage(1);
        }

        // Get the closest point on the collider
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        // Estimate normal using raycast
        Vector3 impactNormal = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (hitPoint - transform.position).normalized, out hit))
        {
            impactNormal = hit.normal;
        }
        else
        {
            // Fallback normal (e.g., away from the object)
            impactNormal = (hitPoint - transform.position).normalized;
        }

        Quaternion rotation;
        if (impactNormal != Vector3.zero)
        {   // Make the particle system's Z-axis face the impact normal
            rotation = Quaternion.LookRotation(impactNormal);
        }
        else
        {
            // Fallback to default rotation if normal is not available
            rotation = Quaternion.identity;
        }

        // Instantiate the particle system with the correct rotation
        Instantiate(particleExplosion, hitPoint, rotation);

        // Destroy the bullet on impact
        Destroy(gameObject, 0.1f);
    }
}