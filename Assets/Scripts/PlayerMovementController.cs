using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using Photon.Pun;


public class PlayerMovementController : MonoBehaviour
{
    public Joystick joystick;
    private RigidbodyFirstPersonController rigidBodyController;
    public FixedTouchField fixedTouchField;

    public Animator anim;

    private Rigidbody rb;
    private float moveSpeed = 8f;
    public float maxInputThreshold = 0.1f;

    private PhotonView photonView;
    public GameObject playerModel;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rigidBodyController = GetComponent<RigidbodyFirstPersonController>();
        photonView = playerModel.GetComponent<PhotonView>();

        if (photonView.IsMine)
        {
            if (playerModel != null)
            {
                playerModel.SetActive(false);
            }

            if (anim != null)
            {
                anim.enabled = false;
            }
        }
        else
        {
            if (playerModel != null)
            {
                playerModel.SetActive(true);
            }

            if (anim != null)
            {
                anim.enabled = true;
            }
        }
    }

    private void Update()
    {
        if (fixedTouchField != null && rigidBodyController != null && photonView.IsMine)
        {
            rigidBodyController.mouseLook.lookInputAxis = fixedTouchField.TouchDist;
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        float horizontalInput = joystick.Horizontal;
        float verticalInput = joystick.Vertical;

        Camera cam = Camera.main;

        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

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

            photonView.RPC("RPC_AnimWalk", RpcTarget.Others, true);
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);

            photonView.RPC("RPC_AnimWalk", RpcTarget.Others, false);
        }
    }

    [PunRPC]
    void RPC_AnimWalk(bool isWalking)
    {
        if (anim != null)
        {
            anim.SetBool("Walk", isWalking);
        }
    }


}
