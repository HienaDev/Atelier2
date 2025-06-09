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
                //Debug.Log("ClearAllProjectiles: Attempted to clear a null projectile.");
                continue; // Skip if the projectile is already destroyed
            }


            WeakPoint weakPoint = projectile.GetComponent<WeakPoint>();
            if (weakPoint != null)
            {
                //Debug.Log("Clearing SpikeShot: " + projectile.name);
                weakPoint.BlowUp();
                continue;
            }

            SpikeShot spikeShot = projectile.GetComponent<SpikeShot>();
            if (spikeShot != null)
            {
                //Debug.Log("Clearing SpikeShot: " + projectile.name);
                spikeShot.BreakApart();
                continue;
            }

            Destructable destructable = projectile.GetComponent<Destructable>();
            if (destructable != null)
            {
                //Debug.Log("Clearing Destructable: " + projectile.name);
                destructable.BlowUp();
                continue;
            }

            Destroyable destroyable = projectile.GetComponent<Destroyable>();
            if (destroyable != null)
            {
                //Debug.Log("Clearing Destroyable: " + projectile.name);
                destroyable.BlowUp();
                continue;
            }

            DamagePlayer damagePlayer = projectile.GetComponent<DamagePlayer>();
            if (damagePlayer != null)
            {
                //Debug.Log("Clearing DamagePlayer: " + projectile.name);
                damagePlayer.BlowUp();
                continue;
            }

            // If no specific component is found, just destroy the projectile
            //Debug.Log("Clearing projectile: " + projectile.name);
            Destroy(projectile);
        }

        projectiles.Clear();
    }
}
