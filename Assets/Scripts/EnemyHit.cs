using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyHit : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();

            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                int actorNumber = playerPhotonView.Owner.ActorNumber;

                RanRanGameManager.instance.PlayerDied(actorNumber);

                Debug.Log("Player hit by enemy and marked as dead: " + playerPhotonView.gameObject.name);

                PhotonView.Get(RanRanGameManager.instance).RPC("RPC_DestroyPlayer", RpcTarget.AllBuffered, playerPhotonView.ViewID);

                GameObject nextAlivePlayer = RanRanGameManager.instance.GetNextAlivePlayer();
                if (nextAlivePlayer != null)
                {
                    RanRanGameManager.instance.SetPlayerToSpectator(nextAlivePlayer);
                }
                else
                {
                    Debug.LogWarning("No players alive to spectate.");
                }
            }
        }
    }





}
