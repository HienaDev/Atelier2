using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private CinemachineCamera cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    [SerializeField] private float defaultShakeIntensity = 1f;
    [SerializeField] private float defaultShakeDuration = 1f;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        noise = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            Debug.LogError("No CinemachineBasicMultiChannelPerlin found.");
        }

        noise.AmplitudeGain = 0f; // Start with no shake
    }

    public void ShakeCamera(float intensity, float duration)
    {
        if (noise == null) return;

        StopAllCoroutines();
        StartCoroutine(Shake(intensity, duration));
    }

    private IEnumerator Shake(float intensity, float duration)
    {
        if (noise == null) yield break;

        noise.AmplitudeGain = intensity;
        yield return new WaitForSeconds(duration);
        noise.AmplitudeGain = 0f; // Reset shake
    }
}