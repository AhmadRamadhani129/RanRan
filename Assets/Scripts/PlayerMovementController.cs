using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;


public class PlayerMovementController : MonoBehaviour
{
    public Joystick joystick;
    private RigidbodyFirstPersonController rigidBodyController;
    public FixedTouchField fixedTouchField;

    private Rigidbody rb;
    public float moveSpeed = 9f;
    public float maxInputThreshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rigidBodyController = GetComponent<RigidbodyFirstPersonController>();
    }

    private void Update()
    {
        if (fixedTouchField != null && rigidBodyController != null)
        {
            rigidBodyController.mouseLook.lookInputAxis = fixedTouchField.TouchDist;
        }
    }


    private void FixedUpdate()
    {
        //rigidBodyController.joystickInputAxis.x = joystick.Horizontal;
        //rigidBodyController.joystickInputAxis.y = joystick.Vertical;

        //if (Mathf.Abs(joystick.Horizontal) > 0.9f || Mathf.Abs(joystick.Vertical) > 0.9f)
        //{
        //    rigidBodyController.movementSettings.ForwardSpeed = 16;
        //}
        //else
        //{
        //    rigidBodyController.movementSettings.ForwardSpeed = 8;
        //}
        float horizontalInput = joystick.Horizontal;
        float verticalInput = joystick.Vertical;

        Camera cam = Camera.main;

        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        //biar arahnya ga kebawah
        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * verticalInput + right * horizontalInput);

        if (moveDirection.magnitude > maxInputThreshold)
        {
            moveDirection = moveDirection.normalized * moveSpeed;

            Vector3 velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z);
            rb.velocity = velocity;
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }

    }


}
