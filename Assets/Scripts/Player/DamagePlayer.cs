using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystemExplosion;
    [SerializeField] private bool disableMesh = true;
    [SerializeField] private bool destroy = true;

    private Collider colliderTrigger;

    public bool dealsDamage = true;

    private bool blownUp = false;

    void Start()
    {
        colliderTrigger = GetComponent<Collider>();

        if(colliderTrigger == null)
        {
            colliderTrigger = GetComponentInChildren<Collider>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if(dealsDamage)
            playerHealth.DealDamage(1);

        if(disableMesh)
        {
            if(colliderTrigger != null)
                colliderTrigger.enabled = false;

            MeshRenderer rend = GetComponent<MeshRenderer>();

            if(rend != null)
            {
                rend.enabled = false;
                Debug.Log(rend.name);
                Debug.Log("AUDIO CUBE RENDERER DISABLED");
            }
            else
            {
                rend = GetComponentInChildren<MeshRenderer>();
                if (rend != null)
                {
                    rend.enabled = false;
                    Debug.Log("AUDIO CUBE RENDERER DISABLED");
                }
                    
            }
 
        }

        float lifetime = 0.1f;
        
        if (particleSystemExplosion != null)
        {
            particleSystemExplosion.Play();
            // Destroy this object after the particle system finishes
            lifetime = particleSystemExplosion.main.duration + particleSystemExplosion.main.startLifetime.constantMax;
        }

        WeakPoint weakPoint = GetComponent<WeakPoint>();

        if (weakPoint != null)
        {
            Debug.Log("WeakPoint found, calling BlowUp");   
            weakPoint.BlowUp(false);
        }
        else if (destroy)
        {
            Debug.Log("Destroying gameObject after " + lifetime + " seconds");
            Destroy(gameObject, lifetime);
        }
            
    }

    public void BlowUp()
    {
        if (blownUp)
            return;
        blownUp = true;

        if (particleSystemExplosion == null)
        {
            Debug.LogWarning("Particle system explosion is not set, cannot blow up.");
            return;
        }

        if (disableMesh)
        {
            if (colliderTrigger != null)
                colliderTrigger.enabled = false;

            MeshRenderer rend = GetComponent<MeshRenderer>();

            if (rend != null)
            {
                rend.enabled = false;
                Debug.Log(rend.name);
                Debug.Log("AUDIO CUBE RENDERER DISABLED");
            }
            else
            {
                rend = GetComponentInChildren<MeshRenderer>();
                if (rend != null)
                {
                    rend.enabled = false;
                    Debug.Log("AUDIO CUBE RENDERER DISABLED");
                }

            }

        }

        float lifetime = 0.1f;

        if (particleSystemExplosion != null)
        {
            particleSystemExplosion.Play();
            // Destroy this object after the particle system finishes
            lifetime = particleSystemExplosion.main.duration + particleSystemExplosion.main.startLifetime.constantMax;
        }

        Debug.Log("Destroying gameObject after " + lifetime + " seconds");
        Destroy(gameObject, lifetime);
    }
}