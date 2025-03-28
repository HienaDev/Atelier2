using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    [SerializeField] private int lives = 2000;
    [SerializeField] private float percentageToChangePhase = 0.1f;
    [SerializeField] private PhaseManager phaseManager;
    [SerializeField] private ClearProjectiles projectiles;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Transform UIParent;
    [SerializeField] private GameObject healthSplit;

    private int currentLives;
    private int currentPhase = 0;
    private int numberOfPhasesSwapped = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLives = lives;

        //GenerateSplits();
    }

    public void DealDamage(int damage)
    {
        currentLives -= damage;

        Debug.Log("currentLives = " + currentLives);

        healthBarFill.fillAmount = (float)currentLives / (float)lives;

        if (currentLives <= 0)
        {
            Debug.Log("Boss defeated");
            GameOver();
        }

        if (currentLives < (lives * (1f - percentageToChangePhase)) - (numberOfPhasesSwapped * lives * percentageToChangePhase))
        {
            projectiles.ClearAllProjectiles();
            numberOfPhasesSwapped++;
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

    }

    public void ChangePhase()
    {
        phaseManager.ChangePhase();
    }
}
