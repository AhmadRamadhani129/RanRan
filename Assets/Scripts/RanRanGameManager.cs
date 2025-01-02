using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;

public class RanRanGameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject playerPrefab;

    [SerializeField] GameObject[] spawnPoints;

    [SerializeField] public GameObject[] itemSpawnPoints;

    [SerializeField] GameObject itemPrefab;

    [SerializeField] GameObject portalPrefab;

    [SerializeField] GameObject portalPosition;

    public int playerCount;

    private int totalItem;
    private TextMeshProUGUI totalItemText;

    public static RanRanGameManager instance;

    private List<int> usedSpawnPoints = new List<int>();
    private List<int> usedItemSpawnPoints = new List<int>();

    private bool itemsSpawned = false;
    public Dictionary<int, bool> playerStatus; // true jika mati atau selesai

    private int playersAlive; // Variabel untuk melacak jumlah pemain yang masih hidup

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Hapus duplikasi GameManager
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // Pertahankan antara perubahan scene
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            playerStatus = new Dictionary<int, bool>();
            playersAlive = PhotonNetwork.PlayerList.Length; // Semua pemain awalnya hidup

            foreach (var player in PhotonNetwork.PlayerList)
            {
                playerStatus[player.ActorNumber] = false;
            }

            SpawnPlayer();
            SpawnItems();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CurrentRoom.PlayerCount);
        base.OnPlayerEnteredRoom(newPlayer);
        playerStatus[newPlayer.ActorNumber] = false;
        playersAlive++;
    }

    public void PlayerDied(int actorNumber)
    {
        if (playerStatus.ContainsKey(actorNumber))
        {
            playerStatus[actorNumber] = true;
            playersAlive--;

            Debug.Log("Player " + actorNumber + " has died. Players Alive: " + playersAlive);

            PhotonView.Get(this).RPC("RPC_UpdatePlayerStatus", RpcTarget.All, actorNumber, true);

            if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            {
                GameObject localPlayer = PhotonNetwork.LocalPlayer.TagObject as GameObject;
                SetPlayerToSpectator(localPlayer);
            }

            CheckGameOver();
        }
    }



    private void CheckGameOver()
    {
        if (AreAllPlayersDeadOrCompleted())
        {
            Debug.Log("Game Over. All players dead or completed.");
            OnLeaveLevel();
        }
    }


    public bool AreAllPlayersDeadOrCompleted()
    {
        foreach (bool isDead in playerStatus.Values)
        {
            if (!isDead) return false;
        }
        return true;
    }

    private IEnumerator WaitForTotalText(int itemCount)
    {
        yield return new WaitForSeconds(0.5f);

        GameObject playerUI = GameObject.Find("PlayerUI(Clone)");
        if (playerUI != null)
        {
            GameObject totalTextbox = GameObject.Find("TotalTextbox");
            Debug.Log("Masukke playerUI");
            if (totalTextbox != null && totalTextbox.transform.IsChildOf(playerUI.transform))
            {
                Debug.Log("Masukke Textbox");
                totalItemText = totalTextbox.GetComponentInChildren<TextMeshProUGUI>();
                UpdateItemCountUI(itemCount);
                if (totalItemText == null)
                {
                    Debug.LogError("TotalText component not found inside TotalTextbox!");
                }
            }
            else
            {
                Debug.LogError("TotalTextbox not found inside PlayerUI!");
            }
        }
        else
        {
            Debug.LogError("PlayerUI(Clone) not found in the scene!");
        }
    }

    private void SpawnPlayer()
    {
        if (playerPrefab != null && spawnPoints.Length > 0)
        {
            string spawnIndexStr = GetRandomSpawnIndex();
            if (spawnIndexStr != "None")
            {
                int spawnIndex = int.Parse(spawnIndexStr);

                // Spawn player di lokasi spawn point yang dipilih
                GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[spawnIndex].transform.position, Quaternion.identity);

                // Pastikan komponen terkait sinkronisasi ditambahkan (jika ada)
                PhotonView playerView = player.GetComponent<PhotonView>();
                if (playerView != null)
                {
                    playerView.ObservedComponents.Add(player.GetComponent<PhotonTransformView>());
                }

                // Atur TagObject untuk pemain lokal
                PhotonNetwork.LocalPlayer.TagObject = player;
                Debug.Log($"Player spawned and TagObject set for {PhotonNetwork.LocalPlayer.NickName} at spawn point {spawnIndex}");

                // Broadcast ke semua klien untuk menyinkronkan TagObject
                PhotonView.Get(this).RPC("SetTagObjectRPC", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, playerView.ViewID);
            }
            else
            {
                Debug.LogWarning("No available spawn points for player!");
            }
        }
    }



    private void SpawnItems()
    {
        // Hanya host spawner yang bertanggung jawab untuk memulai spawn
        if (!itemsSpawned && IsRoomHost())
        {
            photonView.RPC("RPC_SpawnItems", RpcTarget.All);
            itemsSpawned = true;
        }
    }
    private bool IsRoomHost()
    {
        // Pemain pertama di PlayerList dianggap sebagai host spawner
        return PhotonNetwork.LocalPlayer.ActorNumber == PhotonNetwork.PlayerList[0].ActorNumber;
    }

    [PunRPC]
    private void HandleGameOver()
    {
        Debug.Log("Game over, switching to LobbyScene.");
        itemsSpawned = false;

        // Pastikan semua pemain memuat LobbyScene
        if (PhotonNetwork.IsMasterClient)
        {
            ScenesManager.instance.SetCanStart(true); // Tampilkan tombol start game
        }
        SceneManager.LoadScene("LobbyScene");
    }



    [PunRPC]
    private void RPC_UpdateItemCountUI(int itemCount)
    {
        StartCoroutine(WaitForTotalText(itemCount));
    }

    [PunRPC]
    private void RPC_SpawnItems()
    {
        if (itemsSpawned)
        {
            return;
        }

        itemsSpawned = true;

        if (itemPrefab != null && itemSpawnPoints.Length > 0)
        {
            usedItemSpawnPoints.Clear();

            int itemCount = 0;

            for (int i = 0; i < 6; i++)
            {
                string spawnIndexStr = GetRandomItemSpawnIndex();
                if (spawnIndexStr != "None")
                {
                    int spawnIndex = int.Parse(spawnIndexStr);

                    if (IsRoomHost())
                    {
                        PhotonNetwork.Instantiate(itemPrefab.name, itemSpawnPoints[spawnIndex].transform.position, Quaternion.identity);
                    }

                    itemCount++;
                }
                else
                {
                    Debug.LogWarning("No available spawn points for item!");
                    break;
                }
            }

            if (IsRoomHost())
            {
                photonView.RPC("RPC_UpdateItemCountUI", RpcTarget.All, itemCount);
            }
        }
    }


    [PunRPC]
    private void RPC_UpdatePlayerStatus(int actorNumber, bool isDead)
    {
        if (playerStatus.ContainsKey(actorNumber))
        {
            playerStatus[actorNumber] = isDead;
        }
    }



    [PunRPC]
    private void RPC_AddItem()
    {

        totalItem++;
        UpdateItemCountUI(totalItem);
        if (totalItem == 6)
        {
            PhotonNetwork.Instantiate(portalPrefab.name, portalPosition.transform.position, Quaternion.identity);
        }
        
    }
    [PunRPC]
    private void SetTagObjectRPC(int actorNumber, int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view != null)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (player != null)
            {
                player.TagObject = view.gameObject;
                Debug.Log($"TagObject synchronized for player {actorNumber}: {view.gameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning($"PhotonView with ID {viewID} not found for player {actorNumber}.");
        }
    }

    [PunRPC]
    public void RPC_DestroyPlayer(int viewID)
    {
        PhotonView playerView = PhotonView.Find(viewID);
        if (playerView != null)
        {
            Destroy(playerView.gameObject);
            Debug.Log($"Player with viewID {viewID} destroyed.");
        }
        else
        {
            Debug.LogWarning($"Player with viewID {viewID} not found.");
        }
    }



    private void UpdateItemCountUI(int itemCount)
    {
        if (totalItemText != null)
        {
            totalItemText.text = "Total Item " + totalItem.ToString() + "/6";
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found!");
        }
    }

    private string GetRandomSpawnIndex()
    {
        List<int> availableSpawnPoints = new List<int>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!usedSpawnPoints.Contains(i))
            {
                availableSpawnPoints.Add(i);
            }
        }

        if (availableSpawnPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            int selectedIndex = availableSpawnPoints[randomIndex];
            usedSpawnPoints.Add(selectedIndex);
            return selectedIndex.ToString();
        }

        return "None";
    }

    private string GetRandomItemSpawnIndex()
    {
        List<int> availableItemSpawnPoints = new List<int>();

        for (int i = 0; i < itemSpawnPoints.Length; i++)
        {
            if (!usedItemSpawnPoints.Contains(i))
            {
                availableItemSpawnPoints.Add(i);
            }
        }

        if (availableItemSpawnPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, availableItemSpawnPoints.Count);
            int selectedIndex = availableItemSpawnPoints[randomIndex];
            usedItemSpawnPoints.Add(selectedIndex);
            return selectedIndex.ToString();
        }

        return "None";
    }
    public void SetPlayerToSpectator(GameObject player)
    {
        Debug.Log("Di RANRAN: " + player.name);
        player.GetComponent<PlayerMovementController>().enabled = false;
        EnableSpectatorCamera(player);
    }

    private void EnableSpectatorCamera(GameObject player)
    {
        // Disable player's camera
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            playerCamera.enabled = false;
        }

        // Instantiate the spectator camera
        GameObject spectatorCamera = Instantiate(Resources.Load("SpectatorCamera")) as GameObject;
        SpectatorCameraController followCamera = spectatorCamera.GetComponent<SpectatorCameraController>();

        // Find the next alive player to follow
        GameObject nextAlivePlayer = GetNextAlivePlayer();
        if (nextAlivePlayer != null)
        {
            followCamera.SetTarget(nextAlivePlayer.transform);
        }
        else
        {
            Debug.LogWarning("No alive players to follow.");
        }
    }


    public GameObject GetNextAlivePlayer()
    {
        foreach (Player photonPlayer in PhotonNetwork.PlayerList)
        {
            if (playerStatus.ContainsKey(photonPlayer.ActorNumber) && !playerStatus[photonPlayer.ActorNumber])
            {
                Debug.Log($"Checking player {photonPlayer.ActorNumber} - IsDead: {playerStatus[photonPlayer.ActorNumber]}");

                if (photonPlayer.TagObject != null)
                {
                    GameObject playerGameObject = photonPlayer.TagObject as GameObject;
                    if (playerGameObject != null)
                    {
                        Debug.Log($"Found alive player: {photonPlayer.ActorNumber}");
                        return playerGameObject;
                    }
                    else
                    {
                        Debug.LogWarning($"Player {photonPlayer.ActorNumber} has a null TagObject.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Player {photonPlayer.ActorNumber} has no TagObject.");
                }
            }
        }
        Debug.LogWarning("No alive players found.");
        CheckGameOver();
        return null;
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left the room successfully.");

        itemsSpawned = false;
        playerStatus.Clear();

        PhotonNetwork.RemoveBufferedRPCs();
        PhotonNetwork.DestroyAll();

        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }

        SceneManager.LoadScene("LobbyScene");
    }


    public void LeaveRoom()
    {
        int viewID = photonView.ViewID;
        PhotonNetwork.RemoveBufferedRPCs(viewID);
        PhotonNetwork.DestroyAll();            // Hapus semua objek Photon
        PhotonNetwork.LeaveRoom();             // Tinggalkan room

    }


    //public void OnLeaveLevel()
    //{
    //    itemsSpawned = false;

    //    ScenesManager.instance.SetCanStart(false); // Tombol start game disembunyikan

    //    if (PhotonNetwork.LocalPlayer.IsMasterClient && AreAllPlayersDeadOrCompleted())
    //    {
    //        ScenesManager.instance.SetCanStart(true); // Tampilkan tombol start game
    //        SceneManager.LoadScene("LobbyScene");
    //    }
    //}

    public void OnLeaveLevel()
    {
        itemsSpawned = false;

        ScenesManager.instance.SetCanStart(false); // Tombol start game disembunyikan

        // Pastikan semua pemain keluar
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("HandleGameOver", RpcTarget.All);

        int viewID = photonView.ViewID;
        PhotonNetwork.RemoveBufferedRPCs(viewID);
        PhotonNetwork.DestroyAll();            // Hapus semua objek Photon
    }


}
