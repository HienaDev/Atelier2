using UnityEngine;
using DG.Tweening;

public class InstantiateCubes : MonoBehaviour
{
    private AudioVisualizer audioVisualizer;

    [SerializeField] private GameObject cubePrefab;

    [SerializeField] private float maxScale = 10f;
    [SerializeField] private float startingHeight = 2f;
    GameObject[] cubes;

    [SerializeField] private float spacingBetweenCubes = 1.5f;
    [SerializeField] private Ease easingType = Ease.OutExpo;
    [SerializeField] private float spacingDuration = 2f;

    private ClearProjectiles projectiles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioVisualizer = FindAnyObjectByType<AudioVisualizer>();
        projectiles = FindAnyObjectByType<ClearProjectiles>();
        cubes = new GameObject[audioVisualizer.numberOfFrequencies];

        Vector3 startPosition = transform.position;

        for (int i = 0; i < audioVisualizer.numberOfFrequencies; i++)
        {
            // Instantiate all at the same X
            cubes[i] = Instantiate(cubePrefab, startPosition, transform.rotation);
            cubes[i].transform.parent = transform;
            cubes[i].name = "Cube " + i;

            projectiles.AddProjectile(cubes[i]);

            // Get the cube's X scale to space correctly (assuming scale.x = 1 by default)
            float cubeWidth = cubePrefab.transform.localScale.x;

            float totalWidth = (audioVisualizer.numberOfFrequencies - 1) * (cubeWidth + spacingBetweenCubes);
            float targetX = -totalWidth / 2f + i * (cubeWidth + spacingBetweenCubes);


            // Animate X offset using DOTween
            cubes[i].transform.DOLocalMoveX(targetX, spacingDuration).SetEase(easingType);
        }
    }

    void Update()
    {
        var buffer = audioVisualizer.BandBuffer;
        int totalBands = audioVisualizer.numberOfFrequencies;
        int offset = audioVisualizer.barOffset;

        for (int i = 0; i < totalBands; i++)
        {
            // Apply offset with wrap-around
            int shiftedIndex = (i + offset) % totalBands;

            float height = Mathf.Clamp(buffer[shiftedIndex] * maxScale, 0.1f, maxScale);
            if (cubes[i] != null)
                cubes[i].transform.localScale = new Vector3(1, height + startingHeight, 1);
        }
    }
}
