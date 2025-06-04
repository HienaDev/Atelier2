using UnityEngine;
using System.Collections.Generic;

public class ClearProjectiles : MonoBehaviour
{
    private List<GameObject> projectiles;

    void Start()
    {
        projectiles = new List<GameObject> ();
    }

    public void AddProjectile(GameObject projectile)
    {
        projectiles.Add (projectile);
    }

    public void ClearAllProjectiles()
    {
        foreach (GameObject projectile in projectiles)
        {

            if(projectile == null)
            {
                continue; // Skip if the projectile is already destroyed
            }

            SpikeShot spikeShot = projectile.GetComponent<SpikeShot>();
            if (spikeShot != null)
            {
                spikeShot.BreakApart();
                continue;
            }

            Destructable destructable = projectile.GetComponent<Destructable>();
            if (destructable != null)
            {
                destructable.BlowUp();
                continue;
            }

            Destroyable destroyable = projectile.GetComponent<Destroyable>();
            if (destroyable != null)
            {
                destroyable.BlowUp();
                continue;
            }

            DamagePlayer damagePlayer = projectile.GetComponent<DamagePlayer>();
            if (damagePlayer != null)
            {
                damagePlayer.BlowUp();
                continue;
            }

            Destroy(projectile);
        }

        projectiles.Clear();
    }
}
