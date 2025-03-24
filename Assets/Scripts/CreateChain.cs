using UnityEngine;

public class CreateChain : MonoBehaviour
{

    [SerializeField] private int numberOfChains = 10;
    [SerializeField] private GameObject chainPrefab;
    private Transform previousChain;
    private Vector3 anchorPoint;

    [SerializeField] private float moveSpeed = 5f; // Movement speed
    [SerializeField] private float radius = 1.3f; // Radius of the chain


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previousChain = transform;

        InstantiateChains();
    }

    // Update is called once per frame
    void Update()
    {

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
