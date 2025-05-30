using UnityEngine;

public class OvalPath : MonoBehaviour
{
    [SerializeField] private float width = 5f;
    [SerializeField] private float height = 3f;

    public Vector3 GetPointOnPath(float t)
    {
        float angle = t * Mathf.PI * 2f;
        float y = Mathf.Sin(angle) * height * 0.5f;
        float z = Mathf.Cos(angle) * width * 0.5f;
        return transform.position + new Vector3(0f, y, z);
    }

    public float GetApproximateLength()
    {
        float length = 0f;
        Vector3 prev = GetPointOnPath(0f);
        int steps = 50;

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 next = GetPointOnPath(t);
            length += Vector3.Distance(prev, next);
            prev = next;
        }

        return length;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 prevPoint = GetPointOnPath(0f);

        for (int i = 1; i <= 100; i++)
        {
            float t = i / 100f;
            Vector3 nextPoint = GetPointOnPath(t);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }

    public float GetMinZ()
    {
        float minZ = float.MaxValue;
        for (int i = 0; i <= 100; i++)
        {
            float t = i / 100f;
            Vector3 point = GetPointOnPath(t);
            if (point.z < minZ)
                minZ = point.z;
        }
        return minZ;
    }
}