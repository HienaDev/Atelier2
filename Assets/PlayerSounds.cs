using UnityEngine;

public class PlayerSounds : MonoBehaviour
{

    [SerializeField] private AudioSource shootingAudioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerShoot()
    {
        shootingAudioSource.pitch = Random.Range(0.8f, 1.2f);
        shootingAudioSource.Play();
    }
}
