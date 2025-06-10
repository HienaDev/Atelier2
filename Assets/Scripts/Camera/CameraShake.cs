using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class CameraShake : MonoBehaviour
{

    [Header("Tilt Settings")]
    public float maxTiltAngle = 5f;
    public float tiltSpeed = 5f;
    private Quaternion _originalRotation;

    private CinemachineCamera cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    [SerializeField] private float defaultShakeIntensity = 1f;
    [SerializeField] private float defaultShakeDuration = 1f;
    [SerializeField] private float defaultFrequencyGain = 1.2f;

    private readonly List<ShakeInstance> activeShakes = new();
    private Coroutine shakeUpdater;
    private Coroutine smoothShakeCoroutine;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        noise = cinemachineCamera?.GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            Debug.LogError("CinemachineBasicMultiChannelPerlin not found.");
            return;
        }

        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;

        _originalRotation = transform.localRotation;
    }

    void Update()
    {
        ApplyTilt();
    }

    public void ShakeCamera(float intensity, float duration)
    {
        ShakeCamera(intensity, defaultFrequencyGain, duration);
    }

    public void ShakeCamera(float intensity, float frequency, float duration)
    {
        if (noise == null) return;

        activeShakes.Add(new ShakeInstance
        {
            intensity = intensity,
            frequency = frequency,
            timeLeft = duration
        });

        if (shakeUpdater == null)
            shakeUpdater = StartCoroutine(UpdateShake());
    }

    public void SmoothShakeCamera(float intensity, float duration)
    {
        if (noise == null) return;

        if (smoothShakeCoroutine != null)
            StopCoroutine(smoothShakeCoroutine);

        smoothShakeCoroutine = StartCoroutine(SmoothShake(intensity, duration));
    }

    private IEnumerator UpdateShake()
    {
        while (activeShakes.Count > 0)
        {
            float maxIntensity = 0f;
            float correspondingFrequency = 0f;

            for (int i = activeShakes.Count - 1; i >= 0; i--)
            {
                var shake = activeShakes[i];
                shake.timeLeft -= Time.deltaTime;

                if (shake.timeLeft <= 0f)
                {
                    activeShakes.RemoveAt(i);
                    continue;
                }

                if (shake.intensity > maxIntensity)
                {
                    maxIntensity = shake.intensity;
                    correspondingFrequency = shake.frequency;
                }
            }

            noise.AmplitudeGain = maxIntensity;
            noise.FrequencyGain = correspondingFrequency;

            yield return null;
        }

        if (smoothShakeCoroutine == null)
        {
            noise.AmplitudeGain = 0f;
            noise.FrequencyGain = 0f;
        }

        shakeUpdater = null;
    }

    private IEnumerator SmoothShake(float intensity, float duration)
    {
        noise.FrequencyGain = defaultFrequencyGain;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            noise.AmplitudeGain = Mathf.Lerp(intensity, 0f, t);

            elapsed += Time.deltaTime;
            Debug.Log("camera shaking");
            yield return null;
        }

        noise.AmplitudeGain = 0f;

        if (activeShakes.Count == 0)
            noise.FrequencyGain = 0f;

        smoothShakeCoroutine = null;
    }

    private class ShakeInstance
    {
        public float intensity;
        public float frequency;
        public float timeLeft;
    }

    private void ApplyTilt()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Invert values for camera-tilt style motion
        float tiltX = -vertical * maxTiltAngle;
        float tiltZ = horizontal * maxTiltAngle;

        Quaternion targetTilt = _originalRotation * Quaternion.Euler(tiltX, tiltZ, 0f);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetTilt, Time.deltaTime * tiltSpeed);
    }
}