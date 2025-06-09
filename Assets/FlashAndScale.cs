using UnityEngine;
using DG.Tweening;
using System.Collections;

public class FlashAndScale : MonoBehaviour
{
    public enum ScaleAxis { X, Y, Z }

    [Header("Indicator Settings")]
    public GameObject indicator;              // Cylinder that flashes
    [ColorUsage(true, true)]
    public Color flashColor = Color.red;      // Color of the flash
    public float flashDuration = 5f;          // Total time to flash before scaling
    public float minFlashInterval = 0.6f;     // Initial flash interval
    public float maxFlashSpeed = 0.05f;       // Minimum interval (max flash speed)

    [Header("Target Settings")]
    public GameObject targetObject;           // Object to activate and scale
    public float scaleMultiplier = 25f;       // Multiplier for selected scale axis
    public ScaleAxis scaleAxis = ScaleAxis.Z; // Select axis to scale

    [SerializeField] private float scaleDuration = 3f; // Duration of scaling animation 

    private Material indicatorMaterial;
    private Color originalColor;

    private Transform playerTransform;

    void Start()
    {
        if (!indicator || !targetObject)
        {
            Debug.LogError("Indicator or TargetObject is not assigned.");
            return;
        }

        // Instantiate a new material instance to avoid changing shared material
        Renderer rend = indicator.GetComponent<Renderer>();
        indicatorMaterial = rend.material;

        originalColor = indicatorMaterial.GetColor("_Color");

        targetObject.SetActive(false);

        // Find player by PlayerHealth script
        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerTransform = playerHealth.transform;
            RotateTowardsPlayer();
        }
        else
        {
            Debug.LogWarning("PlayerHealth component not found in scene.");
        }

        StartCoroutine(FlashCoroutine());
    }

    void RotateTowardsPlayer()
    {
        if (playerTransform == null) return;

        Vector3 direction = transform.position - playerTransform.position;
        direction.y = 0; // only rotate in Y axis

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    IEnumerator FlashCoroutine()
    {
        indicator.SetActive(true);

        float elapsed = 0f;
        float currentInterval = minFlashInterval;

        while (elapsed < flashDuration)
        {
            // Flash on
            indicatorMaterial.SetColor("_Color", flashColor);
            yield return new WaitForSeconds(currentInterval / 2f);

            // Flash off
            indicatorMaterial.SetColor("_Color", originalColor);
            yield return new WaitForSeconds(currentInterval / 2f);

            elapsed += currentInterval;

            // Accelerate flashing
            float progress = Mathf.Clamp01(elapsed / flashDuration);
            currentInterval = Mathf.Lerp(minFlashInterval, maxFlashSpeed, progress);
        }

        indicator.SetActive(false);
        ActivateAndScaleTarget();
    }

    void ActivateAndScaleTarget()
    {
        targetObject.SetActive(true);

        Vector3 originalScale = targetObject.transform.localScale;
        Vector3 targetScale = originalScale;

        switch (scaleAxis)
        {
            case ScaleAxis.X:
                targetScale.x = originalScale.x * scaleMultiplier;
                break;
            case ScaleAxis.Y:
                targetScale.y = originalScale.y * scaleMultiplier;
                break;
            case ScaleAxis.Z:
                targetScale.z = originalScale.z * scaleMultiplier;
                break;
        }

        targetObject.transform.DOScale(targetScale, scaleDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => { targetObject.GetComponent<DamagePlayer>().BlowUp(); });
    }
}
