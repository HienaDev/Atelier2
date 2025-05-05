using UnityEngine;

public class MoveBackAndForth : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.right; // Direction of movement
    public float moveDistance = 5f; // How far it moves
    public float speed = 2f; // Movement speed

    private Vector3 startPosition;
    private bool movingForward = true;
    private float t = 0f;

    void Start()
    {
        startPosition = transform.position; // Store the starting position
    }

    void Update()
    {
        t += Time.deltaTime * speed * (movingForward ? 1 : -1);
        float moveFactor = Mathf.PingPong(t, 1); // Create a smooth back-and-forth motion

        // Calculate new position based on starting position and movement direction
        transform.position = startPosition + moveDirection.normalized * moveFactor * moveDistance;

        // Switch direction when reaching the limits
        if (t >= 1f) movingForward = false;
        if (t <= 0f) movingForward = true;
    }
}
