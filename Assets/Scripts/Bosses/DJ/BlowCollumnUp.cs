using DG.Tweening;
using UnityEngine;
public class BlowCollumnUp : MonoBehaviour
{
    [SerializeField] private Renderer[] partsRenderer;
    private int health;
    [SerializeField] private GameObject weakpoint;
    [SerializeField] private DJBoss boss;
    private DamageBoss damageBoss;
    [SerializeField] private ClearProjectiles clearProjectiles;

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

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    void Awake()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
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

    public void Initialize(DamageBoss damageBoss, int index, int health = 50)
    {
        this.health = health;
        this.index = index;
        this.damageBoss = damageBoss;
        TriggerFall();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DealDamage(int damage)
    {
        health -= damage;
        foreach (var renderer in partsRenderer)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(renderer.material.DOFloat(5f, "_ColorBrightness", 0.05f).SetEase(Ease.InOutSine));
            sequence.Join(renderer.material.DOFloat(0.15f + damage * 0.5f, "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));
            sequence.Append(renderer.material.DOFloat(1f, "_ColorBrightness", 0.05f).SetEase(Ease.InOutSine));
            sequence.Join(renderer.material.DOFloat(0f, "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));
        }
        if (health <= 0)
        {
            boss.AddSpeakerToList(index);
            GameObject weakpointClone = Instantiate(weakpoint, transform.position, Quaternion.identity);
            weakpointClone.GetComponent<WeakPoint>().onDeath.AddListener(damageBoss.DealCritDamage);
            weakpointClone.GetComponent<WeakPoint>().onDeath.AddListener(boss.DamageAnimation);

            clearProjectiles.AddProjectile(weakpointClone);

            switch (index)
                {                 
                case 0:
                    weakpointClone.GetComponent<WeakPoint>().onLifetime.AddListener(boss.SpawnCollum0);
                    weakpointClone.GetComponent<WeakPoint>().onDeath.AddListener(boss.SpawnCollum0);
                    break;
                case 1:
                    weakpointClone.GetComponent<WeakPoint>().onLifetime.AddListener(boss.SpawnCollum1);
                    weakpointClone.GetComponent<WeakPoint>().onDeath.AddListener(boss.SpawnCollum1);
                    break;
                case 2:
                    weakpointClone.GetComponent<WeakPoint>().onLifetime.AddListener(boss.SpawnCollum2);
                    weakpointClone.GetComponent<WeakPoint>().onDeath.AddListener(boss.SpawnCollum2);
                    break;
                case 3:
                    weakpointClone.GetComponent<WeakPoint>().onLifetime.AddListener(boss.SpawnCollum3);
                    weakpointClone.GetComponent<WeakPoint>().onDeath.AddListener(boss.SpawnCollum3);
                    break;
                default:
                    break;
            }
                

            weakpointClone.GetComponent<WeakPoint>().SetTarget(damageBoss.gameObject.transform);
            gameObject.SetActive(false);
        }
    }
}