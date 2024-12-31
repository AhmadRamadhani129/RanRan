using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private const string GAME_SCENE_NAME = "GameScene";

    private bool canStart;
    private bool isLoadingScene = false;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListGameObjects;
    private Dictionary<int, GameObject> playerListGameObjects;
    private string selectedRoomName;


    [Header("Connection Status")]
    public Text connectionStatusText;

    [Header("Login UI Panel")]
    public InputField playerNameInput;
    public GameObject Login_UI_Panel;

    [Header("Game Options UI Panel")]
    public GameObject GameOptions_UI_Panel;

    [Header("Create Room UI Panel")]
    public GameObject CreateRoom_UI_Panel;
    public InputField roomNameInputField;
    public InputField roomPasswordInputField;
    public InputField maxPlayerInputField;


    [Header("Inside Room UI Panel")]
    public GameObject InsideRoom_UI_Panel;
    public Text roomInfoText;
    public GameObject playerListPrefab;
    public GameObject playerListContent;
    public GameObject startGameButton;


    [Header("Room List UI Panel")]
    public GameObject RoomList_UI_Panel;
    public GameObject roomListEntryPrefab;
    public GameObject roomListParentGameobject;

    [Header("Join Random Room UI Panel")]
    public GameObject JoinRandomRoom_UI_Panel;

    [Header("Join Room Password UI Panel")]
    public GameObject JoinRoomPassword_UI_Panel;
    public InputField roomPasswordInputFieldJoin;

    #region UI Callbacks

    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Playername is invalid");
        }
    }


    public void OnCreateRoomButtonClicked()
    {
        string roomName = roomNameInputField.text;
        int maxPlayers;
        string roomPassword = roomPasswordInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();

        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable();

        customRoomProperties.Add("password", roomPassword);
        roomOptions.CustomRoomProperties = customRoomProperties;

        roomOptions.CustomRoomPropertiesForLobby = new string[] { "password" };
        if (int.TryParse(maxPlayerInputField.text, out maxPlayers))
        {
            maxPlayers = Mathf.Clamp(maxPlayers, 2, 10);
            roomOptions.MaxPlayers = (byte)maxPlayers;
        }
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(GameOptions_UI_Panel.name);
    }

    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        ActivatePanel(RoomList_UI_Panel.name);
    }

    public void OnButtonBackClicked()
    {
        ActivatePanel(GameOptions_UI_Panel.name);
    }

    public void OnLeaveGameButtonClicked()
    {
        CleanUpPlayerList();

        PhotonNetwork.RemoveBufferedRPCs(); // Hapus semua RPC
        PhotonNetwork.DestroyAll(); // Hapus semua objek PhotonView

        PhotonNetwork.LeaveRoom();

        Debug.Log("Left the room and cleared all buffers.");

        roomNameInputField.text = "";
        roomPasswordInputField.text = "";
        maxPlayerInputField.text = "";
    }

    public void OnJoinRandomRoomButtonClicked()
    {
        ActivatePanel(JoinRandomRoom_UI_Panel.name);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnJoinRoomButtonClicked(string _roomName)
    {
        selectedRoomName = _roomName;

        ActivatePanel(JoinRoomPassword_UI_Panel.name);
    }

    public void OnConfirmJoinRoomButtonClicked()
    {
        string enteredPassword = roomPasswordInputFieldJoin.text;

        if (cachedRoomList.ContainsKey(selectedRoomName))
        {
            RoomInfo selectedRoomInfo = cachedRoomList[selectedRoomName];
            if (selectedRoomInfo.CustomProperties.ContainsKey("password"))
            {
                string roomPassword = selectedRoomInfo.CustomProperties["password"].ToString();

                if (enteredPassword == roomPassword)
                {
                    PhotonNetwork.JoinRoom(selectedRoomName);
                }
                else
                {
                    Debug.Log("Incorrect password");
                }
            }
        }
    }
    
    #endregion

    #region Unity Methods
    void Start()
    {
        GameObject objectToDestroy = GameObject.Find("GameManager");

        if (objectToDestroy != null)
        {
            Destroy(objectToDestroy);
            Debug.Log(" has been destroyed.");
        }
        else
        {
            Debug.LogWarning(" not found or is not in DontDestroyOnLoad.");
        }

        canStart = true;

        startGameButton.GetComponent<Button>().onClick.AddListener(OnStartGameButtonClicked);

        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameObjects = new Dictionary<string, GameObject>();

        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Player is already in room.");
            OnJoinedRoom();
        }
        else
        {
            Debug.Log("Player is not in a room.");
            ActivatePanel(Login_UI_Panel.name);
        }
    }



    void Update()
    {
        connectionStatusText.text = "Connection status: " + PhotonNetwork.NetworkClientState;
    }
    #endregion

    #region Photon Callbacks
    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is connected to Photon");
        ActivatePanel(GameOptions_UI_Panel.name);
    }

    public override void OnCreatedRoom()
    {
        ScenesManager.canStart = true;
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created");
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.RemoveBufferedRPCs();
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);

        ActivatePanel(InsideRoom_UI_Panel.name);

        canStart = ScenesManager.canStart;
        Debug.Log(canStart);

        if (PhotonNetwork.LocalPlayer.IsMasterClient && canStart)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }

        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
                            "Players/Max.players: " +
                            PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                            PhotonNetwork.CurrentRoom.MaxPlayers;

        // **Pastikan player list sudah siap dan room tersedia**
        // Update player list after ensuring room is ready



        if (PhotonNetwork.CurrentRoom != null)
        {
            UpdatePlayerList();
        }
        else
        {
            Debug.LogError("CurrentRoom is null after joining.");
        }
    }


    private void UpdatePlayerList()
    {
        // Ensure playerListGameObjects is initialized
        if (playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!playerListGameObjects.ContainsKey(player.ActorNumber))
            {
                GameObject playerListGameObject = Instantiate(playerListPrefab);
                playerListGameObject.transform.SetParent(playerListContent.transform);
                playerListGameObject.transform.localScale = Vector3.one; // Prevent scale issues
                playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

                // Show player indicator for local player
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
                playerListGameObjects.Add(player.ActorNumber, playerListGameObject);
            }
        }

        if (roomInfoText != null && PhotonNetwork.CurrentRoom != null)
        {
            roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
                                "Players/Max.players: " +
                                PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                                PhotonNetwork.CurrentRoom.MaxPlayers;
        }
        else
        {
            if (roomInfoText == null)
            {
                Debug.LogError("roomInfoText is not assigned!");
            }

            if (PhotonNetwork.CurrentRoom == null)
            {
                Debug.LogError("CurrentRoom is null.");
            }
        }

    }




    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();
        foreach (RoomInfo room in roomList)
        {
            Debug.Log(room.Name);
            if (!room.IsOpen || !room.IsVisible || room.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList.Remove(room.Name);
                }
            }
            else
            {
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList[room.Name] = room;
                }
                else
                {
                    cachedRoomList.Add(room.Name, room);
                }
            }
        }

        // Instantiate room list
        foreach (RoomInfo room in cachedRoomList.Values)
        {
            GameObject roomListEntryGameObject = Instantiate(roomListEntryPrefab);
            roomListEntryGameObject.transform.SetParent(roomListParentGameobject.transform);
            roomListEntryGameObject.transform.localScale = Vector3.one;
            roomListEntryGameObject.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
            roomListEntryGameObject.transform.Find("RoomPlayersText").GetComponent<Text>().text = room.PlayerCount + " / " + room.MaxPlayers;
            roomListEntryGameObject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinRoomButtonClicked(room.Name));

            roomListGameObjects.Add(room.Name, roomListEntryGameObject);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject playerListGameobject = Instantiate(playerListPrefab);
        playerListGameobject.transform.SetParent(playerListContent.transform);
        playerListGameobject.transform.localScale = Vector3.one;
        playerListGameobject.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;

        playerListGameobject.transform.Find("PlayerIndicator").gameObject.SetActive(newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
        playerListGameObjects.Add(newPlayer.ActorNumber, playerListGameobject);

        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
                            "Players/Max.players: " +
                            PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                            PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playerListGameObjects.ContainsKey(otherPlayer.ActorNumber))
        {
            Destroy(playerListGameObjects[otherPlayer.ActorNumber]);
            playerListGameObjects.Remove(otherPlayer.ActorNumber);
        }
        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
                            "Players/Max.players: " +
                            "Players/Max.players: " +
                            PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                            PhotonNetwork.CurrentRoom.MaxPlayers;
    }
    public override void OnLeftRoom()
    {
        Debug.Log("Left the room successfully.");

        if (RanRanGameManager.instance != null)
        {
            Destroy(RanRanGameManager.instance.gameObject);
            RanRanGameManager.instance = null;
            Debug.Log("RanRanGameManager instance destroyed.");
        }

        SceneManager.LoadScene("LobbyScene");
    }


    #endregion

    #region Private Methods

    private void ClearRoomListView()
    {
        foreach (var roomListGameObject in roomListGameObjects.Values)
        {
            Destroy(roomListGameObject);
        }
        roomListGameObjects.Clear();
    }

    public void ActivatePanel(string panelToBeActivated)
    {
        Login_UI_Panel.SetActive(panelToBeActivated.Equals(Login_UI_Panel.name));
        GameOptions_UI_Panel.SetActive(panelToBeActivated.Equals(GameOptions_UI_Panel.name));
        CreateRoom_UI_Panel.SetActive(panelToBeActivated.Equals(CreateRoom_UI_Panel.name));
        InsideRoom_UI_Panel.SetActive(panelToBeActivated.Equals(InsideRoom_UI_Panel.name));
        RoomList_UI_Panel.SetActive(panelToBeActivated.Equals(RoomList_UI_Panel.name));
        JoinRandomRoom_UI_Panel.SetActive(panelToBeActivated.Equals(JoinRandomRoom_UI_Panel.name));
        JoinRoomPassword_UI_Panel.SetActive(panelToBeActivated.Equals(JoinRoomPassword_UI_Panel.name));
    }

    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient && !isLoadingScene)
        {
            isLoadingScene = true;

            // Pastikan buffer bersih
            PhotonNetwork.RemoveBufferedRPCs();
            PhotonNetwork.DestroyAll();

            // Mulai ulang level
            PhotonNetwork.LoadLevel(1);
        }
    }


    private void CleanUpPlayerList()
    {
        if (playerListGameObjects != null)
        {
            foreach (GameObject playerGameObject in playerListGameObjects.Values)
            {
                Destroy(playerGameObject);
            }
            playerListGameObjects.Clear();
        }
    }
    #endregion
}
