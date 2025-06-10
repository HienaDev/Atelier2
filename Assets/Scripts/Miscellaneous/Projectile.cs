using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float defaultSpeed = 5f;
    [SerializeField] private float defaultLifetime = 5f;

    private float speed;
    private float lifetime;
    private float timer;

    private List<Renderer> renderers = new List<Renderer>();
    private List<Collider> colliders = new List<Collider>();
    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    private bool isBreaking = false;

    private void Awake()
    {
        // Cache all renderers, colliders, and particle systems in self + children
        renderers.AddRange(GetComponentsInChildren<Renderer>());
        colliders.AddRange(GetComponentsInChildren<Collider>());
        particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>());
    }

    public void Initialize(float speed, float maxLifetime)
    {
        this.speed = speed;
        this.lifetime = maxLifetime;
        this.timer = 0f;

        // Stop all particles to ensure clean start
        foreach (var ps in particleSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }



    private void Update()
    {
        if (isBreaking) return;

        // Move forward in local space (Z+)
        transform.position += transform.forward * speed * Time.deltaTime;

        // Timer countdown
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            BreakApart();
        }
    }

    private void BreakApart()
    {
        if (isBreaking) return;
        isBreaking = true;

        // Disable renderers
        foreach (var r in renderers)
        {
            r.enabled = false;
        }

        // Disable colliders
        foreach (var c in colliders)
        {
            c.enabled = false;
        }

        // Play all particle systems
        float maxDuration = 0f;
        foreach (var ps in particleSystems)
        {
            ps.Play();
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            if (duration > maxDuration)
                maxDuration = duration;
        }

        // Destroy after longest particle effect finishes
        Destroy(gameObject, maxDuration > 0f ? maxDuration : 0.5f);
    }
}
