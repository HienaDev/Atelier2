using UnityEngine;

public class TailProjectile : MonoBehaviour
{
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private float spikeLifetime = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            SpawnSpike();
            CameraShake cameraShake = FindFirstObjectByType<CameraShake>();
            cameraShake.ShakeCamera(2f, 0.1f);
            Destroy(gameObject); // Destroy projectile upon impact
        }
    }

    private void SpawnSpike()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = 0.1f; // Ensure spike is slightly above the ground

        GameObject spike = Instantiate(spikePrefab, spawnPosition, Quaternion.identity);

        // Ensure the spike is destroyed after spikeLifetime
        Destroy(spike, spikeLifetime);
    }
}