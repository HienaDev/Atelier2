using UnityEngine;

public class RezProjectile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject); // Destroy projectile on player hit
        }
    }
}
