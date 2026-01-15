using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private GameObject followTarget;
    public float zOffset = 3.0f;
    public float yOffset = 0.5f;

    protected void Update()
    {
        Vector3 finalPosition = followTarget.transform.position - followTarget.transform.forward * zOffset;
        finalPosition.y += yOffset;
        transform.position = finalPosition;
        transform.forward = followTarget.transform.forward;
        transform.LookAt(followTarget.transform.position, Vector3.up);
    }
}
