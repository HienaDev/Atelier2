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
            Destroy(projectile);
        }

        projectiles.Clear();
    }
}
