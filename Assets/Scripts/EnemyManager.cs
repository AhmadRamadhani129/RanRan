using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviourPunCallbacks
{
    public GameObject enemyPrefab;  
    public Transform spawnPoint;   

    private GameObject enemyInstance;

    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    SpawnEnemy();
                }
            }
        }
    }

    // Fungsi untuk spawn musuh
    void SpawnEnemy()
    {
        if (enemyPrefab != null && spawnPoint != null)
        {
            // Gunakan InstantiateRoomObject agar musuh tidak terikat dengan player
            enemyInstance = PhotonNetwork.InstantiateRoomObject(enemyPrefab.name, spawnPoint.position, spawnPoint.rotation);
        }
    }


    public override void OnJoinedRoom()
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            base.OnJoinedRoom();

            if (enemyInstance == null && PhotonNetwork.IsMasterClient)
            {
                SpawnEnemy();
            }
        }
    }
}
