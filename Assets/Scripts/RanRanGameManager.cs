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
    private Dictionary<int, bool> playerStatus; // true jika mati atau selesai


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            playerStatus = new Dictionary<int, bool>();
            foreach (var player in PhotonNetwork.PlayerList)
            {
                playerStatus[player.ActorNumber] = false; // Semua pemain awalnya hidup
            }
            SpawnPlayer();
            SpawnItems();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CurrentRoom.PlayerCount);
        base.OnPlayerEnteredRoom(newPlayer);
        playerStatus[newPlayer.ActorNumber] = false; // Tambahkan pemain baru sebagai hidup
    }

    public void PlayerDied(int actorNumber)
    {
        if (playerStatus.ContainsKey(actorNumber))
        {
            playerStatus[actorNumber] = true;
            Debug.Log("Player " + actorNumber + " has died.");
            CheckGameOver();
        }
    }

    private void CheckGameOver()
    {
        if (AreAllPlayersDeadOrCompleted())
        {
            Debug.Log("All players are dead or have completed the game.");
            // Tambahkan logika untuk mengakhiri permainan jika semua pemain mati
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
                GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[spawnIndex].transform.position, Quaternion.identity);
                player.GetComponent<PhotonView>().ObservedComponents.Add(player.GetComponent<PhotonTransformView>());
            }
            else
            {
                Debug.LogWarning("No available spawn points for player!");
            }
        }
    }

    private void SpawnItems()
    {
        if (!itemsSpawned)
        {
            photonView.RPC("RPC_SpawnItems", RpcTarget.AllBuffered);
            itemsSpawned = true;
        }
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
                    PhotonNetwork.Instantiate(itemPrefab.name, itemSpawnPoints[spawnIndex].transform.position, Quaternion.identity);
                    itemCount++;
                }
                else
                {
                    Debug.LogWarning("No available spawn points for item!");
                    break;
                }
            }

            photonView.RPC("RPC_UpdateItemCountUI", RpcTarget.AllBuffered, itemCount);
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

        // Cari titik spawn yang belum dipakai
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!usedSpawnPoints.Contains(i))
            {
                availableSpawnPoints.Add(i);
            }
        }

        if (availableSpawnPoints.Count > 0)
        {
            // Munculkan Titik spawn random
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

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        itemsSpawned = false;

        NetworkManager.instance.ActivatePanel("InsideRoomPanel");
        SceneManager.LoadScene("LobbyScene");
    }


}
