using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private AudioSource shootingAudioSource;

    public void PlayerShoot()
    {
        shootingAudioSource.pitch = Random.Range(0.8f, 1.2f);
        shootingAudioSource.Play();
    }
}
