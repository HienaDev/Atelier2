using UnityEngine;

public class LegProjectile : MonoBehaviour
{
    private float speed;
    [SerializeField] private float lifeTime = 5f;
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    private void Start()
    {
        transform.position = new Vector3(0f, transform.position.y, transform.position.z);
        
        Vector3 forwardDirection = transform.forward;
        forwardDirection.x = 0f;
        if (forwardDirection.magnitude > 0.01f)
        {
            transform.forward = forwardDirection.normalized;
        }
        else
        {
            transform.forward = Vector3.forward;
        }
        
        Destroy(gameObject, lifeTime);
    }
    
    private void Update()
    {
        Vector3 movement = transform.forward * speed * Time.deltaTime;
        movement.x = 0f;
        
        transform.position += movement;
        
        transform.position = new Vector3(0f, transform.position.y, transform.position.z);
    }
    
    private void LateUpdate()
    {
        if (transform.position.x != 0f)
        {
            transform.position = new Vector3(0f, transform.position.y, transform.position.z);
        }
    }
}