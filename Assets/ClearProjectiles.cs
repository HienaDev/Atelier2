
using UnityEngine;
using System.Collections.Generic;

public class ClearProjectiles : MonoBehaviour
{
    private List<GameObject> projectiles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        projectiles = new List<GameObject> ();
    }

    // Update is called once per frame
    void Update()
    {
        
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
