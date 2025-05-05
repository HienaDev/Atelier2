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

    public void SmoothShakeCamera(float intensity, float duration)
    {
        if (noise == null) return;

        StopAllCoroutines();
        StartCoroutine(SmoothShake(intensity, duration));
    }

    private IEnumerator Shake(float intensity, float duration)
    {
        if (noise == null) yield break;

        noise.AmplitudeGain = intensity;
        yield return new WaitForSeconds(duration);
        noise.AmplitudeGain = 0f; // Reset shake
    }

    private IEnumerator SmoothShake(float intensity, float duration)
    {
        if (noise == null) yield break;

        float elapsed = 0f;
        float smoothIntensity = 0f;

        while (elapsed < duration)
        {
            smoothIntensity = Mathf.Lerp(intensity, 0f, elapsed / duration);
            noise.AmplitudeGain = smoothIntensity;

            elapsed += Time.deltaTime;
            yield return null;
        }

        noise.AmplitudeGain = 0f; // Reset shake
    }
}