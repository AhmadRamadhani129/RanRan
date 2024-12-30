using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorCameraController : MonoBehaviour
{
    private Camera targetCamera; // Kamera di dalam GameObject target
    public Transform target; // Transform target (opsional untuk fallback)
    public float smoothSpeed = 0.125f; // Kecepatan interpolasi untuk posisi

    void LateUpdate()
    {
        if (targetCamera != null)
        {
            // Sinkronkan posisi dan rotasi dengan targetCamera
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetCamera.transform.position, smoothSpeed);
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

        // Coba temukan kamera di dalam GameObject target
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
