using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.15f;
    public Vector3 offset = new Vector3(0, 0, -10);

    Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);
    }
}

