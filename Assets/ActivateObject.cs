using System.Collections;
using UnityEngine;

public class ActivateObject : MonoBehaviour
{
    [SerializeField] private GameObject objectToActivate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);
        objectToActivate.SetActive(true);
    }

}
