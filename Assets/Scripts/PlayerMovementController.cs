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

    // Start is called before the first frame update
    void Start()
    {
        rigidBodyController = GetComponent<RigidbodyFirstPersonController>();
    }

    private void Update()
    {
        try
        {
            rigidBodyController.mouseLook.lookInputAxis = fixedTouchField.TouchDist;
        }
        catch (NullReferenceException ex)
        {
            Debug.LogError("A null reference occurred: " + ex.Message);
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

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput);

        transform.Translate(moveDirection * 6 * Time.deltaTime);

    }


}
