using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorCameraController : MonoBehaviour
{
    private Camera targetCamera;
    public Transform target;
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (targetCamera != null)
        {
            Vector3 forwardDirection = targetCamera.transform.forward;
            Vector3 desiredPosition = targetCamera.transform.position + forwardDirection * 1f;


            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, targetCamera.transform.rotation, smoothSpeed);
            transform.rotation = smoothedRotation;
        }
        else if (target != null)
        {
            // Fallback: Kamera mengikuti transform target jika targetCamera tidak tersedia
            Vector3 desiredPosition = target.position;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.LookAt(target);
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
