using UnityEngine;

public class CreateChain : MonoBehaviour
{
    [SerializeField] private int numberOfChains = 10;
    [SerializeField] private GameObject chainPrefab;

    [SerializeField] private float moveSpeed = 5f; // Movement speed
    [SerializeField] private float radius = 1.3f; // Radius of the chain

    private Transform previousChain;
    private Vector3 anchorPoint;

    void Start()
    {
        previousChain = transform;

        InstantiateChains();
    }

    public void InstantiateChains()
    {
        for (int i = 0; i < numberOfChains; i++)
        {
            GameObject chain = Instantiate(chainPrefab);

            chain.transform.position = transform.position;

            chain.GetComponent<ProceduralChainMovement>().Initialize(moveSpeed, radius, previousChain);
            previousChain = chain.transform;
        }
    }
}
