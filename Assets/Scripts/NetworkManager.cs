using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private const string GAME_SCENE_NAME = "GameScene";

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

    //Method Login
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
        
        //Ketika nama room kosong, akan dibuat angka random
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();

        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable();

        //Tambah password ke custom properties
        customRoomProperties.Add("password", roomPassword);
        roomOptions.CustomRoomProperties = customRoomProperties;
        //Buat string untuk password
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "password" };

        //Method untuk menampilkan max player
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
        PhotonNetwork.LeaveRoom();

        //Membersihkan semua text flied saat create room
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
        //Menyimpan nama room
        selectedRoomName = _roomName;

        ActivatePanel(JoinRoomPassword_UI_Panel.name);
    }

    public void OnConfirmJoinRoomButtonClicked()
    {
        string enteredPassword = roomPasswordInputFieldJoin.text;

        //Membaca custom properties
        if (cachedRoomList.ContainsKey(selectedRoomName))
        {
            RoomInfo selectedRoomInfo = cachedRoomList[selectedRoomName];
            if (selectedRoomInfo.CustomProperties.ContainsKey("password"))
            {
                string roomPassword = selectedRoomInfo.CustomProperties["password"].ToString();

                if (enteredPassword == roomPassword)
                {
                    PhotonNetwork.JoinRoom(selectedRoomName); // Join the room if the password matches
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
        ActivatePanel(Login_UI_Panel.name);
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameObjects = new Dictionary<string, GameObject>();
        startGameButton.GetComponent<Button>().onClick.AddListener(OnStartGameButtonClicked);

        //Menghubungkan Master dan client
        PhotonNetwork.AutomaticallySyncScene = true;
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
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(InsideRoom_UI_Panel.name);
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
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

        if (playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        // Instantiate GameObject Player
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerListGameobject = Instantiate(playerListPrefab);
            playerListGameobject.transform.SetParent(playerListContent.transform);
            playerListGameobject.transform.localScale = Vector3.one; // to prevent scale issues
            playerListGameobject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

            // Mengecek jika ada key yang double
            if (!playerListGameObjects.ContainsKey(player.ActorNumber))
            {
                if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    playerListGameobject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
                }
                else
                {
                    playerListGameobject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
                }

                playerListGameObjects.Add(player.ActorNumber, playerListGameobject);
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
                            PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                            PhotonNetwork.CurrentRoom.MaxPlayers;
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

    private void ActivatePanel(string panelToBeActivated)
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
            isLoadingScene = true;  // Prevent another scene load attempt
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