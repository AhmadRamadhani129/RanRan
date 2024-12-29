using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] GameObject fpsCamera;
    [SerializeField] float lookSensitivity = 150f;
    private float currentCameraUpAndDownRotation = 0f;
    private Vector3 velocity = Vector3.zero;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // calculate movement velocity a 3D vector
        float _xMovement = Input.GetAxis("Horizontal");
        float _zMovement = Input.GetAxis("Vertical");

        Vector3 _movementHorizontal = transform.right * _xMovement;
        Vector3 _movementVertical = transform.forward * _zMovement;

        // final movement velocity 
        Vector3 _movementVelocity = (_movementHorizontal + _movementVertical).normalized * speed;

        // Apply movement
        Move(_movementVelocity);

        // calculate rotation as 3D vector for turning around
        float _yRotation = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
        Vector3 _rotationVector = new Vector3(0, _yRotation, 0);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(_rotationVector));

        // Calculate look up and down rotation
        float _cameraUpAndDownRotation = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;
        if (fpsCamera != null)
        {
            currentCameraUpAndDownRotation -= _cameraUpAndDownRotation;

            currentCameraUpAndDownRotation = Mathf.Clamp(currentCameraUpAndDownRotation, -85, 85);
            fpsCamera.transform.localEulerAngles = new Vector3(currentCameraUpAndDownRotation, 0, 0);
        }

    }

    private void FixedUpdate()
    {
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }

    void Move(Vector3 movementVelocity)
    {
        velocity = movementVelocity;

    }
}
