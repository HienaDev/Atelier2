using System;
using UnityEngine;
using static PhaseManager;

public class DJBoss : MonoBehaviour, BossInterface
{

    [Serializable]
    public struct Collumn
    {
        public GameObject collumn;
        public int index;
        public Transform firePoint;
    }

    [SerializeField] private int numberOfWeakspointsToDestroy = 2;
    private int tutorialWeakpointsDestroyed = 0;

    [SerializeField] private Collumn[] collumns;

    public void PhaseEnded()
    {
        
    }

    public void StartAttack(PhaseManager.SubPhase subphase)
    {
        Debug.Log(subphase);
        //switch (subphase)
        //{
        //    case PhaseManager.SubPhase.Tutorial:
        //        StartCoroutine(SpawnTutorialWeakpoints());
        //        break;
        //    case PhaseManager.SubPhase.Easy:
        //        fightStarted = true;
        //        break;
        //    case PhaseManager.SubPhase.Normal:
        //        NormalDifficulty();
        //        fightStarted = true;
        //        break;
        //}
    }

    public void NormalDifficulty()
    {

    }

    public void StartBoss(PhaseManager.SubPhase subPhase)
    {
        StartAttack(subPhase);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnCollumn(int pos)
    {

    }
}
