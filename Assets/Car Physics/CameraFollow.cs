using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private GameObject followTarget;
    public float zOffset = 4.56f;
    public float yOffset = 1.88f;
    public float mfollowResponsiveness = 5.0f;
    public float mlookAtResponsiveness = 5.0f;
    

    protected void LateUpdate()
    {
        //Rotation ratio depends on difference between camera forward and target forward. Higher the difference, higher the ratio.
        transform.rotation = Quaternion.Slerp(transform.rotation, followTarget.transform.rotation, mlookAtResponsiveness * Time.deltaTime);
        
        //Update follow position for camera
        Vector3 followPosition = followTarget.transform.position - followTarget.transform.forward * zOffset;
        followPosition.y += yOffset;
        transform.position = Vector3.Lerp(transform.position, followPosition, mfollowResponsiveness * Time.deltaTime);
    }
}
