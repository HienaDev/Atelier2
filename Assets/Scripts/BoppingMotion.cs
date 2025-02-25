using UnityEngine;

public class BobbingMotion : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.5f; // Max height change
    [SerializeField] private float speed = 2f; // How fast it bobs

    private float startY;

    void Start()
    {
        startY = transform.position.y; // Store original Y position
    }

    void Update()
    {
        float newY = startY + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
