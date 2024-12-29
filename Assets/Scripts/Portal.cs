using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon; // Untuk menggunakan Hashtable Photon

public class Portal : MonoBehaviourPunCallbacks
{
    private List<int> playersInCollider = new List<int>(); // Melacak pemain berdasarkan ActorNumber

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (photonView.IsMine)
            {
                PhotonView otherPhotonView = other.GetComponent<PhotonView>();

                if (otherPhotonView != null && !playersInCollider.Contains(otherPhotonView.Owner.ActorNumber))
                {
                    playersInCollider.Add(otherPhotonView.Owner.ActorNumber); // Tambahkan pemain ke daftar

                    // Cek apakah jumlah pemain dalam collider sama dengan jumlah pemain aktif di room
                    if (playersInCollider.Count == PhotonNetwork.CurrentRoom.PlayerCount)
                    {
                        PhotonNetwork.LoadLevel("LobbyScene"); // Kembali ke RoomScene
                    }
                }
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
}
