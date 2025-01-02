using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorCameraController : MonoBehaviour
{
    private Camera targetCamera;
    public Transform target;
    public float smoothSpeed = 0.1f;

    void LateUpdate()
    {
        if (targetCamera != null)
        {
            Vector3 desiredPosition = targetCamera.transform.position + targetCamera.transform.forward;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            Quaternion desiredRotation = targetCamera.transform.rotation;
            Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, desiredRotation, smoothSpeed);
            transform.rotation = smoothedRotation;

        }
        else if (target != null)
        {
            Vector3 desiredPosition = target.position + target.forward * 2f;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            Quaternion desiredRotation = target.rotation;
            Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, desiredRotation, smoothSpeed);
            transform.rotation = smoothedRotation;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (newTarget != null)
        {
            Camera foundCamera = newTarget.GetComponentInChildren<Camera>();
            if (foundCamera != null)
            {
                targetCamera = foundCamera;
                Debug.Log("Target camera set to " + foundCamera.name);
            }
            else
            {
                Debug.LogWarning("No camera found in target. Falling back to position tracking.");
                targetCamera = null;
            }
        }
    }
}
