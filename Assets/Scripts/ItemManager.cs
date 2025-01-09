using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class ItemManager : MonoBehaviourPunCallbacks
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.IsConnected)
                {
                    GameObject gameManager = GameObject.Find("GameManager");
                    PhotonView photonView = gameManager.GetComponent<PhotonView>();
                    photonView.RPC("RPC_AddItem", RpcTarget.AllBuffered);

                }
            }
            Destroy(gameObject);
        }
    }
}
