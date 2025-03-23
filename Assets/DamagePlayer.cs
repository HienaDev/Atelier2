using NUnit.Framework;
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{

    [SerializeField] private ParticleSystem particleSystemExplosion;

    private Collider colliderTrigger;

    [SerializeField] private bool disableMesh = true;
    [SerializeField] private bool destroy = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colliderTrigger = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        playerHealth.DealDamage(1);

        if(disableMesh)
        {
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
