using UnityEngine;

public class TailProjectile : MonoBehaviour
{
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private float spikeLifetime = 3f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float flightTime;
    private float elapsedTime;
    private bool isMoving = true;

    private PhaseManager phaseManager;

    public void Initialize(Vector3 target, float speed)
    {
        startPosition = transform.position;
        targetPosition = target;
        targetPosition.y = 1.25f;

        flightTime = Vector3.Distance(startPosition, targetPosition) / speed;
        elapsedTime = 0f;

        phaseManager = FindAnyObjectByType<PhaseManager>();
    }

    private void Update()
    {
        if (!isMoving) return;

        elapsedTime += Time.deltaTime;
        float t = elapsedTime / flightTime;

        if (t >= 1f)
        {
            transform.position = targetPosition;
            Impact();
        }
        else
        {
            Vector3 nextPos = Vector3.Lerp(startPosition, targetPosition, t);
            nextPos.y += Mathf.Sin(t * Mathf.PI) * 3.5f;
            transform.position = nextPos;
        }
    }

    private void Impact()
    {
        isMoving = false;

        CameraShake cameraShake = phaseManager.CurrentCamera.GetComponent<CameraShake>();
        if (cameraShake != null)
            cameraShake.SmoothShakeCamera(2f, 0.1f);

        SpawnSpike();
        Destroy(gameObject);
    }

    private void SpawnSpike()
    {
        GameObject spike = Instantiate(spikePrefab, targetPosition, Quaternion.identity);
        Destroy(spike, spikeLifetime);
    }
}