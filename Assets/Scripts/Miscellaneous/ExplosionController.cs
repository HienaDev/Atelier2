using System.Collections;
using UnityEngine;
using DG.Tweening; // For DOTween version

public class ExplosionController : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionForce = 500f;
    public float explosionRadius = 10f;
    public float upwardModifier = 3f;
    public float resetDelay = 3f;

    [Header("Visual Effects")]
    public ParticleSystem explosionParticles;
    public AudioSource explosionSound;
    public float screenShakeIntensity = 0.5f;
    public float screenShakeDuration = 0.3f;

    [Header("Glitch Settings")]
    public float glitchDuration = 2f;
    public float glitchIntensity = 2f;
    public int glitchSteps = 10;

    [Header("Grid Material Glitch Settings")]
    public float gridGlitchIntensity = 1f;
    public float gridGlitchFrequency = 0.1f; // How often glitches occur (lower = more frequent)
    public bool enableGridColorGlitch = true;
    public bool enableGridGeometryGlitch = true;
    public bool enableGridInverseGlitch = true;
    public bool enableGridGradientGlitch = true;

    [SerializeField] private Transform[] objectsToExplode;
    [SerializeField] private GameObject[] gridObjects;

    [SerializeField] private GameObject winScreen;

    // Store original material values for reset
    private struct GridMaterialState
    {
        public bool inverse;
        public Color color;
        public bool gradient;
        public Color nonGradientColor;
        public float lineWidth;
        public float lineHeight;
        public float colorIntensity;
        public Material material;
    }

    private GridMaterialState[] originalGridStates;

    private void Start()
    {
        // Store original grid material states
        StoreOriginalGridStates();
        ExplodeObjectsGlitchy(objectsToExplode, objectsToExplode[0].position);
    }

    private void StoreOriginalGridStates()
    {
        if (gridObjects != null && gridObjects.Length > 0)
        {
            originalGridStates = new GridMaterialState[gridObjects.Length];

            for (int i = 0; i < gridObjects.Length; i++)
            {
                if (gridObjects[i] != null)
                {
                    Renderer renderer = gridObjects[i].GetComponent<Renderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        Material mat = renderer.material;
                        originalGridStates[i] = new GridMaterialState
                        {
                            inverse = mat.HasProperty("_Inverse") ? mat.GetFloat("_Inverse") == 1f : false,
                            color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white,
                            gradient = mat.HasProperty("_Gradient") ? mat.GetFloat("_Gradient") == 1f : false,
                            //nonGradientColor = mat.HasProperty("_NonGradientColor") ? mat.GetColor("_NonGradientColor") : Color.black,
                            lineWidth = mat.HasProperty("_LineWidth") ? mat.GetFloat("_LineWidth") : 0.9f,
                            lineHeight = mat.HasProperty("_LineHeight") ? mat.GetFloat("_LineHeight") : 0.9f,
                            colorIntensity = mat.HasProperty("_ColorIntensity") ? mat.GetFloat("_ColorIntensity") : 0.01f,
                            material = mat
                        };
                    }
                }
            }
        }
    }

    // VERSION 2: DOTween Glitchy Explosion
    public void ExplodeObjectsGlitchy(Transform[] objects, Vector3 explosionCenter)
    {
        StartCoroutine(ExplodeGlitchyRoutine(objects, explosionCenter));
    }

    private IEnumerator ExplodeGlitchyRoutine(Transform[] objects, Vector3 explosionCenter)
    {
        // Play explosion effects
        PlayExplosionEffects(explosionCenter);

        // Start grid material glitching
        StartCoroutine(GridMaterialGlitchRoutine(glitchDuration + 1f));

        foreach (Transform obj in objects)
        {
            // Store original position and rotation
            Vector3 originalPos = obj.position;
            Quaternion originalRot = obj.rotation;
            Vector3 originalScale = obj.localScale;

            // Disable rigidbody for DOTween control
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            // Calculate final explosion position
            Vector3 explosionDir = (obj.position - explosionCenter).normalized;
            explosionDir.y = Mathf.Max(explosionDir.y, 0.5f); // Bias upward
            Vector3 finalPos = obj.position + explosionDir * Random.Range(5f, 15f);
            finalPos.y += Random.Range(2f, 8f); // Additional upward movement

            // Create glitchy movement sequence
            Sequence glitchSequence = DOTween.Sequence();

            // Phase 1: Glitchy teleportation
            for (int i = 0; i < glitchSteps; i++)
            {
                Vector3 glitchPos = Vector3.Lerp(originalPos, finalPos, (float)i / glitchSteps);
                glitchPos += Random.insideUnitSphere * glitchIntensity * Random.Range(0.1f, 1f);

                float stepDuration = glitchDuration / glitchSteps;

                glitchSequence.Append(obj.DOMove(glitchPos, stepDuration * 0.1f).SetEase(Ease.Flash));
                glitchSequence.AppendInterval(stepDuration * 0.9f);

                // Random scale glitches
                if (Random.value < 0.3f)
                {
                    glitchSequence.Join(obj.DOScale(originalScale * Random.Range(0.8f, 1.3f), stepDuration * 0.1f)
                        .SetEase(Ease.Flash));
                }

                // Random rotation glitches
                if (Random.value < 0.4f)
                {
                    Vector3 glitchRotation = Random.insideUnitSphere * 90f;
                    glitchSequence.Join(obj.DORotate(glitchRotation, stepDuration * 0.1f, RotateMode.LocalAxisAdd)
                        .SetEase(Ease.Flash));
                }
            }

            // Phase 2: Final explosive movement
            glitchSequence.Append(obj.DOMove(finalPos, 0.5f).SetEase(Ease.OutQuart));
            glitchSequence.Join(obj.DORotate(Random.insideUnitSphere * 360f, 0.5f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.OutQuart));

            // Phase 3: Falling with glitchy interruptions
            Vector3 fallPos = new Vector3(finalPos.x, originalPos.y - 5f, finalPos.z);
            Sequence fallSequence = DOTween.Sequence();

            // Add glitchy interruptions during fall
            for (int i = 0; i < 3; i++)
            {
                Vector3 interruptPos = Vector3.Lerp(finalPos, fallPos, (float)i / 3f);
                interruptPos += Random.insideUnitSphere * 0.5f;

                fallSequence.Append(obj.DOMove(interruptPos, 0.2f).SetEase(Ease.InOutQuart));

                // Glitch effect
                fallSequence.Append(obj.DOMove(interruptPos + Random.insideUnitSphere * 0.8f, 0.05f)
                    .SetEase(Ease.Flash));
                fallSequence.Append(obj.DOMove(interruptPos, 0.05f).SetEase(Ease.Flash));
            }

            fallSequence.Append(obj.DOMove(fallPos, 0.3f).SetEase(Ease.InQuart));

            glitchSequence.Append(fallSequence);

            // Add continuous rotation throughout
            obj.DORotate(Random.insideUnitSphere * 720f, glitchDuration + 1f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear);

            // Random color/material glitching (if renderer exists)
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                StartCoroutine(GlitchMaterial(renderer, glitchDuration));
            }

            // Small delay between objects
            yield return new WaitForSeconds(Random.Range(0f, 0.2f));
        }

        // Wait for animations to complete
        yield return new WaitForSeconds(glitchDuration + 1f);

        // Reset all objects
        foreach (Transform obj in objects)
        {
            obj.DOKill(); // Stop all tweens
            // Reset to original state or desired reset state
        }

        yield return new WaitForSeconds(2f);

        winScreen.SetActive(true);

        // Reset grid materials
        ResetGridMaterials();

    }

    private IEnumerator GridMaterialGlitchRoutine(float duration)
    {
        if (gridObjects == null || gridObjects.Length == 0) yield break;

        float elapsed = 0f;
        float nextGlitchTime = 0f;

        while (elapsed < duration)
        {
            if (elapsed >= nextGlitchTime)
            {
                // Apply glitch effects to random grid objects
                int numObjectsToGlitch = Random.Range(1, Mathf.Min(gridObjects.Length + 1, 5));

                for (int i = 0; i < numObjectsToGlitch; i++)
                {
                    int randomIndex = Random.Range(0, gridObjects.Length);
                    if (gridObjects[randomIndex] != null)
                    {
                        StartCoroutine(ApplyGridGlitchEffect(randomIndex));
                    }
                }

                // Set next glitch time
                nextGlitchTime = elapsed + Random.Range(gridGlitchFrequency * 0.5f, gridGlitchFrequency * 2f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ApplyGridGlitchEffect(int gridIndex)
    {
        if (gridIndex >= gridObjects.Length || gridObjects[gridIndex] == null) yield break;

        Renderer renderer = gridObjects[gridIndex].GetComponent<Renderer>();
        if (renderer == null || renderer.material == null) yield break;

        Material mat = renderer.material;
        GridMaterialState originalState = originalGridStates[gridIndex];

        // Duration of this specific glitch
        float glitchEffectDuration = Random.Range(0.05f, 0.3f);
        float elapsed = 0f;

        while (elapsed < glitchEffectDuration)
        {
            // Inverse glitch
            if (enableGridInverseGlitch && mat.HasProperty("_Inverse"))
            {
                if (Random.value < 0.3f)
                {
                    mat.SetFloat("_Inverse", Random.value > 0.5f ? 1f : 0f);
                }
            }

            // Color glitch
            if (enableGridColorGlitch && mat.HasProperty("_Color"))
            {
                if (Random.value < 0.4f)
                {
                    Color glitchColor = new Color(
                        Random.Range(0f, 1f),
                        Random.Range(0f, 1f),
                        Random.Range(0f, 1f),
                        originalState.color.a
                    );
                    mat.SetColor("_Color", glitchColor);
                }
            }

            //// Non-gradient color glitch
            //if (enableGridColorGlitch && mat.HasProperty("_NonGradientColor"))
            //{
            //    if (Random.value < 0.4f)
            //    {
            //        Color glitchNonGradColor = new Color(
            //            Random.Range(0f, 1f),
            //            Random.Range(0f, 1f),
            //            Random.Range(0f, 1f),
            //            originalState.nonGradientColor.a
            //        );
            //        mat.SetColor("_NonGradientColor", glitchNonGradColor);
            //    }
            //}

            // Gradient toggle glitch
            if (enableGridGradientGlitch && mat.HasProperty("_Gradient"))
            {
                if (Random.value < 0.2f)
                {
                    mat.SetFloat("_Gradient", Random.value > 0.5f ? 1f : 0f);
                }
            }

            // Geometry glitches
            if (enableGridGeometryGlitch)
            {
                if (mat.HasProperty("_LineWidth") && Random.value < 0.3f)
                {
                    float glitchWidth = originalState.lineWidth + Random.Range(-0.3f, 0.3f) * gridGlitchIntensity;
                    glitchWidth = Mathf.Clamp01(glitchWidth);
                    mat.SetFloat("_LineWidth", glitchWidth);
                }

                if (mat.HasProperty("_LineHeight") && Random.value < 0.3f)
                {
                    float glitchHeight = originalState.lineHeight + Random.Range(-0.3f, 0.3f) * gridGlitchIntensity;
                    glitchHeight = Mathf.Clamp01(glitchHeight);
                    mat.SetFloat("_LineHeight", glitchHeight);
                }
            }

            // Color intensity glitch
            if (enableGridColorGlitch && mat.HasProperty("_ColorIntensity"))
            {
                if (Random.value < 0.3f)
                {
                    float glitchIntensity = originalState.colorIntensity + Random.Range(-0.02f, 0.1f) * gridGlitchIntensity;
                    glitchIntensity = Mathf.Max(0f, glitchIntensity);
                    mat.SetFloat("_ColorIntensity", glitchIntensity);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Brief return to normal (optional flicker effect)
        if (Random.value < 0.5f)
        {
            ResetSingleGridMaterial(gridIndex);
            yield return new WaitForSeconds(Random.Range(0.02f, 0.1f));
        }
    }

    private void ResetGridMaterials()
    {
        if (originalGridStates == null) return;

        for (int i = 0; i < originalGridStates.Length; i++)
        {
            ResetSingleGridMaterial(i);
        }
    }

    private void ResetSingleGridMaterial(int index)
    {
        if (index >= originalGridStates.Length || index >= gridObjects.Length) return;
        if (gridObjects[index] == null) return;

        Renderer renderer = gridObjects[index].GetComponent<Renderer>();
        if (renderer == null || renderer.material == null) return;

        Material mat = renderer.material;
        GridMaterialState original = originalGridStates[index];

        if (mat.HasProperty("_Inverse"))
            mat.SetFloat("_Inverse", original.inverse ? 1f : 0f);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", original.color);

        if (mat.HasProperty("_Gradient"))
            mat.SetFloat("_Gradient", original.gradient ? 1f : 0f);

        //if (mat.HasProperty("_NonGradientColor"))
        //    mat.SetColor("_NonGradientColor", original.nonGradientColor);

        if (mat.HasProperty("_LineWidth"))
            mat.SetFloat("_LineWidth", original.lineWidth);

        if (mat.HasProperty("_LineHeight"))
            mat.SetFloat("_LineHeight", original.lineHeight);

        if (mat.HasProperty("_ColorIntensity"))
            mat.SetFloat("_ColorIntensity", original.colorIntensity);
    }

    private void PlayExplosionEffects(Vector3 position)
    {
        // Particle effects
        if (explosionParticles != null)
        {
            explosionParticles.transform.position = position;
            explosionParticles.Play();
        }

        // Sound effect
        if (explosionSound != null)
        {
            explosionSound.Play();
        }

        // Screen shake
        StartCoroutine(ScreenShake());
    }

    private IEnumerator ScreenShake()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 originalPos = cam.transform.position;
            float elapsed = 0f;

            while (elapsed < screenShakeDuration)
            {
                float strength = screenShakeIntensity * (1f - elapsed / screenShakeDuration);
                Vector3 randomOffset = Random.insideUnitSphere * strength;
                cam.transform.position = originalPos + randomOffset;

                elapsed += Time.deltaTime;
                yield return null;
            }

            cam.transform.position = originalPos;
        }
    }

    private IEnumerator ScaleImpact(Transform obj)
    {
        Vector3 originalScale = obj.localScale;
        obj.localScale = originalScale * 1.1f;

        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            obj.localScale = Vector3.Lerp(originalScale * 1.1f, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.localScale = originalScale;
    }

    private IEnumerator GlitchMaterial(Renderer renderer, float duration)
    {
        Material originalMaterial = renderer.material;
        Color originalColor = originalMaterial.color;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (Random.value < 0.1f) // 10% chance each frame
            {
                // Random color glitch
                renderer.material.color = new Color(
                    Random.Range(0f, 1f),
                    Random.Range(0f, 1f),
                    Random.Range(0f, 1f),
                    originalColor.a
                );

                yield return new WaitForSeconds(0.05f);
                renderer.material.color = originalColor;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        renderer.material.color = originalColor;
    }
}