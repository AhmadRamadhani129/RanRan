using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public Camera FPSCamera;
    private PlayerMovementController playerMovementController;
    public GameObject playerUIPrefab;

    // Start is called before the first frame update
    void Start()
    {
        playerMovementController = GetComponent<PlayerMovementController>();

        if (photonView.IsMine)
        {
            GameObject playerUIGameObject = Instantiate(playerUIPrefab);
            playerMovementController.joystick = playerUIGameObject.transform.Find("Fixed Joystick").GetComponent<Joystick>();
            playerMovementController.fixedTouchField = playerUIGameObject.transform.Find("RotationTouchField").GetComponent<FixedTouchField>();
            FPSCamera.enabled = true;
        }
        else
        {
            playerMovementController.enabled = false;
            GetComponent<RigidbodyFirstPersonController>().enabled = false;
            FPSCamera.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
