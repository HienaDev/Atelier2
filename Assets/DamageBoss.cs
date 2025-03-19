using UnityEngine;

public class DamageBoss : MonoBehaviour
{

    [SerializeField] private BossHealth health;



    public void DealDamage()
    {
        health.DealDamage(1);
    }
}
