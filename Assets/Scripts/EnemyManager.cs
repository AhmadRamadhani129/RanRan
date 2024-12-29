using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class EnemyManager : MonoBehaviourPunCallbacks
{
    public GameObject enemyPrefab;  
    public Transform spawnPoint;   

    private GameObject enemyInstance;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SpawnEnemy();
            }
        }
    }

    // Fungsi untuk spawn musuh
    void SpawnEnemy()
    {
        if (enemyPrefab != null && spawnPoint != null)
        {
            enemyInstance = PhotonNetwork.Instantiate(enemyPrefab.name, spawnPoint.position, spawnPoint.rotation);
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        if (enemyInstance == null && PhotonNetwork.IsMasterClient)
        {
            SpawnEnemy();
        }
    }
}
