using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Portal : MonoBehaviourPunCallbacks
{
    private List<int> playersInCollider = new List<int>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView otherPhotonView = other.GetComponent<PhotonView>();

            if (otherPhotonView != null && !playersInCollider.Contains(otherPhotonView.Owner.ActorNumber))
            {
                playersInCollider.Add(otherPhotonView.Owner.ActorNumber);
                CheckAllPlayersInPortal();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView otherPhotonView = other.GetComponent<PhotonView>();

            if (otherPhotonView != null && playersInCollider.Contains(otherPhotonView.Owner.ActorNumber))
            {
                playersInCollider.Remove(otherPhotonView.Owner.ActorNumber); 
            }
        }
    }

    private void CheckAllPlayersInPortal()
    {
        RanRanGameManager gameManager = RanRanGameManager.instance;

        if (gameManager == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        foreach (Player photonPlayer in PhotonNetwork.PlayerList)
        {
            if (gameManager.playerStatus.ContainsKey(photonPlayer.ActorNumber) &&
                !gameManager.playerStatus[photonPlayer.ActorNumber])
            {
                if (!playersInCollider.Contains(photonPlayer.ActorNumber))
                {
                    return;
                }
            }
        }

        Debug.Log("All alive players are in the portal. Ending level.");
        photonView.RPC("HandleAllPlayersInPortal", RpcTarget.All);
    }

    [PunRPC]
    private void HandleAllPlayersInPortal()
    {
        RanRanGameManager.instance.OnLeaveLevel();
    }
}
