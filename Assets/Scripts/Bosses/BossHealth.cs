using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using static PhaseManager;
using Unity.Cinemachine;

public class BossHealth : MonoBehaviour
{
    [SerializeField] private int lives = 2000;
    private float percentageToChangePhase = 0.1f;
    [SerializeField] private PhaseManager phaseManager;
    [SerializeField] private ClearProjectiles projectiles;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Transform UIParent;
    [SerializeField] private GameObject healthSplit;

    [SerializeField] private GameObject winScreen;

    private int currentLives;
    private int currentPhase = 0;
    private int numberOfPhasesSwapped = 0;

    [SerializeField] private Material bopWaveMaterial;
    [SerializeField] private float duration = 0.5f;
    float size;
    float startValue;
    float endValue;
    private Tween waveTween;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLives = lives;

        percentageToChangePhase = 1f / (float)phaseManager.phases.Length;

        size = bopWaveMaterial.GetFloat("_Size");
        startValue = 0f - size;
        endValue = 1f;

        bopWaveMaterial.SetFloat("_WaveProgression", startValue);
        //GenerateSplits();
    }

    public void SkipPhase()
    {
        projectiles.ClearAllProjectiles();
        if (phaseManager.GetCurrentSubPhase() == SubPhase.Tutorial)
        {
            phaseManager.ChangePhase();
        }
        DealDamage((int)(lives * percentageToChangePhase) + 5); // Skip to the next phase
    }

    public void DealCritDamage()
    {
        // Add an extra 5 to account for division and rounding errors, so that the boss changes after 3 crits

        waveTween?.Kill();

        Vector3 worldPos = phaseManager.CurrentBoss.transform.position;
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);

        bopWaveMaterial.SetVector("_RingSpawnPosition", new Vector4(viewportPos.x, viewportPos.y, 0f, 0f)); // Set the spawn position of the ring
        // Set starting value
        bopWaveMaterial.SetFloat("_WaveProgression", startValue);

        // Tween to target value
        waveTween = DOTween.To(
            () => bopWaveMaterial.GetFloat("_WaveProgression"),
            val => bopWaveMaterial.SetFloat("_WaveProgression", val),
            endValue,
            duration
        );

        DealDamage(((lives / phaseManager.phases.Length) / 4) + 5);

    }



    public void DealDamage(int damage)
    {
        currentLives -= damage;

        Debug.Log(damage + " damage dealt to boss");
        Debug.Log("currentLives = " + currentLives);

        healthBarFill.fillAmount = (float)currentLives / (float)lives;

        if (currentLives <= 0)
        {
            Debug.Log("Boss defeated");
            GameOver();
        }

        // 1800, 2000 * (1f - 0.1667) - (1 * 2000 * 0.1667) = 1500
        if (currentLives < (lives * (1f - percentageToChangePhase)) - (numberOfPhasesSwapped * lives * percentageToChangePhase))
        {
            Debug.Log(currentLives + " < " + (lives * (1f - percentageToChangePhase)) + " - (" + numberOfPhasesSwapped + " * " + lives * percentageToChangePhase + ")");
            projectiles.ClearAllProjectiles();
            numberOfPhasesSwapped++;
            Debug.Log("Phase changed because of HP");
            ChangePhase();
        }
    }

    // -255 - 255
    private void GenerateSplits()
    {
        int totalPhases = Mathf.RoundToInt(1f / percentageToChangePhase); // e.g., 1 / 0.25 = 4 phases

        for (int i = 1; i < totalPhases; i++) // Start from 1 (skip 0)
        {
            float markerX = (495 / 2 * (i * percentageToChangePhase)) - 255; // Position based on % of bar

            // Create a new marker instance
            GameObject marker = Instantiate(healthSplit, UIParent.transform);
            marker.transform.localPosition = new Vector2(markerX - 246, 189); // Set position on bar
        }
    }

    private void GameOver()
    {
        winScreen.SetActive(true);
    }

    public void ChangePhase()
    {
        phaseManager.ChangePhase();
    }
}
