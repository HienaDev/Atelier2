using UnityEngine;

public class SpeakerEffects : MonoBehaviour
{
    private ParticleSystem particleSystemShoot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        particleSystemShoot = GetComponentInChildren<ParticleSystem>();
    }

    public void ShootEffect()
    {
        if (particleSystemShoot != null)
        {
            particleSystemShoot.Play();
        }
    }
}
