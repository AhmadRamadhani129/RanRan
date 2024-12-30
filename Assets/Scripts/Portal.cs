using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Portal : MonoBehaviourPunCallbacks
{
    private List<int> playersInCollider = new List<int>(); // Melacak pemain berdasarkan ActorNumber

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView otherPhotonView = other.GetComponent<PhotonView>();

            if (otherPhotonView != null && !playersInCollider.Contains(otherPhotonView.Owner.ActorNumber))
            {
                playersInCollider.Add(otherPhotonView.Owner.ActorNumber); // Tambahkan pemain ke daftar

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
                playersInCollider.Remove(otherPhotonView.Owner.ActorNumber); // Hapus pemain dari daftar jika keluar
            }
        }
    }

    private void CheckAllPlayersInPortal()
    {
        // Dapatkan GameManager untuk mengakses status pemain
        RanRanGameManager gameManager = RanRanGameManager.instance;

        if (gameManager == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        foreach (Player photonPlayer in PhotonNetwork.PlayerList)
        {
            // Cek apakah pemain hidup dan berada di portal
            if (gameManager.playerStatus.ContainsKey(photonPlayer.ActorNumber) &&
                !gameManager.playerStatus[photonPlayer.ActorNumber]) // Pemain hidup (bukan spectate)
            {
                if (!playersInCollider.Contains(photonPlayer.ActorNumber))
                {
                    return; // Ada pemain hidup yang belum di portal
                }
            }
        }

        // Semua pemain hidup berada di portal
        Debug.Log("All alive players are in the portal. Ending level.");
        photonView.RPC("HandleAllPlayersInPortal", RpcTarget.All);
    }

    [PunRPC]
    private void HandleAllPlayersInPortal()
    {
        // Semua pemain keluar dari level
        RanRanGameManager.instance.OnLeaveLevel();
    }
}
