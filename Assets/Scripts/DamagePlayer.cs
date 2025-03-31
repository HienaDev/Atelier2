using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystemExplosion;
    [SerializeField] private bool disableMesh = true;
    [SerializeField] private bool destroy = true;

    private Collider colliderTrigger;

    void Start()
    {
        colliderTrigger = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        playerHealth.DealDamage(1);

        if(disableMesh)
        {
            if(colliderTrigger != null)
                colliderTrigger.enabled = false;
            GetComponent<Renderer>().enabled = false;

        }

        float lifetime = 0.1f;

        if (particleSystemExplosion != null)
        {
            particleSystemExplosion.Play();
            // Destroy this object after the particle system finishes
            lifetime = particleSystemExplosion.main.duration + particleSystemExplosion.main.startLifetime.constantMax;
        }

        if(destroy)
            Destroy(gameObject, lifetime);
    }
}