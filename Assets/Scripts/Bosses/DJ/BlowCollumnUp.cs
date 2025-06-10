using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DestructionAudioClip
{
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(-3f, 3f)]
    public float pitch = 1f;
}

public class BlowCollumnUp : MonoBehaviour
{
    [SerializeField] private Renderer[] partsRenderer;
    private int health;
    private int maxHealth;
    [SerializeField] private GameObject weakpoint;
    [SerializeField] private DJBoss boss;
    private DamageBoss damageBoss;
    [SerializeField] private ClearProjectiles clearProjectiles;

    private Transform[] critPoints;
    public void SetCritPoints(Transform[] points)
    {
        critPoints = points;
    }

    private int index = 0;

    [Header("Fall Settings")]
    public float fallHeight = 5f;
    public float fallDuration = 0.6f;

    [Header("Cartoon Animation Settings")]
    public Vector3 stretchScale = new Vector3(0.7f, 1.5f, 0.7f);      // Stretched tall and thin
    public Vector3 squashScale = new Vector3(1.5f, 0.4f, 1.5f);       // Very flat and wide
    public Vector3 reboundScale = new Vector3(0.85f, 1.3f, 0.85f);    // Slight vertical stretch after squash
    public Vector3 overshootScale = new Vector3(1.1f, 0.9f, 1.1f);    // Slight horizontal stretch

    public float anticipationDuration = 0.2f;                         // Initial "preparation" before fall
    public float stretchDuration = 0.15f;                             // Duration for stretching during fall
    public float squashDuration = 0.15f;                              // Duration for impact squash
    public float reboundDuration = 0.2f;                              // Duration for first rebound
    public float settleDuration = 0.25f;                              // Duration for final settle

    public float bounceHeight = 0.5f;                                 // How high to bounce after squash
    public float wiggleAmount = 5f;                                   // Rotation wiggle intensity

    [Header("Destruction Audio Settings")]
    [SerializeField] private List<DestructionAudioClip> destructionAudioClips = new List<DestructionAudioClip>();
    [SerializeField] private bool playAllClipsOnDestruction = true; // If false, plays random clip
    [SerializeField] private float pitchVariation = 0.1f; // Random pitch variation range

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitSoundVolume = 1f;
    [SerializeField] private float hitSoundPitch = 1f;

    private Gradient healthGradient;

    private bool immuneToDamage = false;

    void Awake()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;

        healthGradient = new Gradient();
        healthGradient.SetKeys(
            new GradientColorKey[] {
            new GradientColorKey(new Color(0.15f, 0.02f, 0.02f), 1f),    // Dark red
            new GradientColorKey(new Color(0.3f, 0.1f, 0.05f), 0.8f),    // Reddish brown
            new GradientColorKey(new Color(0.4f, 0.4f, 0.05f), 0.6f),    // Dull olive
            new GradientColorKey(new Color(0.4f, 0.6f, 0.1f), 0.4f),     // Yellow-green
            new GradientColorKey(new Color(0.4f, 0.8f, 0.2f), 0.2f),     // Lime green (muted)
            new GradientColorKey(new Color(0.5f, 1f, 0.25f), 0f)         // Bright lime green
            },
            new GradientAlphaKey[] {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
            }
        );
    }


    private void PlayDestructionAudio()
    {
        if (destructionAudioClips.Count == 0 || AudioManager.Instance == null) return;

        if (playAllClipsOnDestruction)
        {
            // Play all clips simultaneously
            foreach (var destructionClip in destructionAudioClips)
            {
                if (destructionClip.clip != null)
                {
                    // Add random pitch variation within the specified range
                    float randomPitch = destructionClip.pitch + Random.Range(-pitchVariation, pitchVariation);
                    AudioManager.Instance.PlaySound(destructionClip.clip, destructionClip.volume, randomPitch, true, 0f);
                }
            }
        }
        else
        {
            // Play a random clip
            DestructionAudioClip randomClip = destructionAudioClips[Random.Range(0, destructionAudioClips.Count)];
            if (randomClip.clip != null)
            {
                // Add random pitch variation within the specified range
                float randomPitch = randomClip.pitch + Random.Range(-pitchVariation, pitchVariation);
                AudioManager.Instance.PlaySound(randomClip.clip, randomClip.volume, randomPitch, true, 0f);
            }
        }
    }

    public void TriggerFall()
    {
        // Reset position and rotation
        transform.position = originalPosition + Vector3.up * fallHeight;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;

        // Create a more elaborate cartoon sequence
        Sequence cartoonFallSequence = DOTween.Sequence();

        // Optional: Quick anticipation move (slight upward movement before falling)
        cartoonFallSequence.Append(transform.DOMoveY(transform.position.y + 0.2f, anticipationDuration)
            .SetEase(Ease.OutQuad));

        // Fall with stretching effect (objects stretch when falling fast)
        cartoonFallSequence.Append(transform.DOMoveY(originalPosition.y, fallDuration)
            .SetEase(Ease.InQuad));

        // Progressive stretch during fall (thinner and longer as it falls)
        cartoonFallSequence.Join(transform.DOScale(stretchScale, fallDuration * 0.8f)
            .SetEase(Ease.InOutQuad));

        // Extreme squash on impact
        cartoonFallSequence.Append(transform.DOScale(squashScale, squashDuration)
            .SetEase(Ease.OutBounce));

        // Add impact effect - slight horizontal wiggle
        cartoonFallSequence.Join(transform.DOShakeRotation(squashDuration, wiggleAmount, 10, 90, false));

        // Rebound upward with stretching
        cartoonFallSequence.Append(transform.DOScale(reboundScale, reboundDuration)
            .SetEase(Ease.OutQuad));
        cartoonFallSequence.Join(transform.DOMoveY(originalPosition.y + bounceHeight, reboundDuration)
            .SetEase(Ease.OutQuad));

        // Fall back down with slight stretching
        cartoonFallSequence.Append(transform.DOMoveY(originalPosition.y, reboundDuration * 0.7f)
            .SetEase(Ease.InQuad));

        // Final smaller squash
        cartoonFallSequence.Append(transform.DOScale(overshootScale, squashDuration * 0.7f)
            .SetEase(Ease.OutQuad));

        // Settle to normal scale with elastic effect
        cartoonFallSequence.Append(transform.DOScale(originalScale, settleDuration)
            .SetEase(Ease.OutElastic, 1.2f));

        cartoonFallSequence.AppendCallback(() =>
        {
            boss.RemoveSpeakerFromList(index);
        });
    }

    public void Initialize(DamageBoss damageBoss, int index, int health = 50, bool immuneToDamage = false)
    {
        this.health = health;
        maxHealth = health;
        this.index = index;
        this.damageBoss = damageBoss;
        this.immuneToDamage = immuneToDamage;


        if (!immuneToDamage)
            UpdateColorByHealth();
        else
            foreach (var renderer in partsRenderer)
                renderer.material.SetColor("_Color", Color.black);

        TriggerFall();
    }


    public void DealDamage(int damage)
    {
        if (immuneToDamage)
            return;

        health -= damage;

        UpdateColorByHealth();

        foreach (var renderer in partsRenderer)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(renderer.material.DOFloat(1.5f, "_ColorBrightness", 0.05f).SetEase(Ease.InOutSine));
            sequence.Join(renderer.material.DOFloat(0.15f + damage * 0.5f, "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));
            sequence.Append(renderer.material.DOFloat(0.75f, "_ColorBrightness", 0.05f).SetEase(Ease.InOutSine));
            sequence.Join(renderer.material.DOFloat(0f, "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));
            AudioManager.Instance.PlaySound(hitSound, hitSoundVolume, hitSoundPitch, true, 0f);
        }
        if (health <= 0)
        {
            PlayDestructionAudio();

            boss.AddSpeakerToList(index);
            GameObject weakpointClone = Instantiate(weakpoint, transform.position, Quaternion.identity);
            var weakpointComponent = weakpointClone.GetComponent<WeakPoint>();

            weakpointComponent.onDeath.AddListener(() => damageBoss.DealCritDamage(false));
            weakpointComponent.onDeath.AddListener(boss.DamageAnimation);

            DamagePlayer damagePlayer = weakpointClone.GetComponent<DamagePlayer>();
            if (damagePlayer != null)
                damagePlayer.enabled = false;


            clearProjectiles.AddProjectile(weakpointClone);


            weakpointComponent.onLifetime.AddListener(() => boss.SpawnCollumn(index, false));
            weakpointComponent.onDeath.AddListener(() => boss.SpawnCollumn(index, true));
            weakpointComponent.SetCritPositions(critPoints);


            weakpointComponent.SetTarget(damageBoss.transform);
            gameObject.SetActive(false);

        }
    }

    private void UpdateColorByHealth()
    {
        float t = Mathf.Clamp01(health / 50f);
        Color newColor = healthGradient.Evaluate(t);

        foreach (var renderer in partsRenderer)
        {
            Material instancedMaterial = renderer.material;
            instancedMaterial.SetColor("_Color", newColor);
        }
    }

}