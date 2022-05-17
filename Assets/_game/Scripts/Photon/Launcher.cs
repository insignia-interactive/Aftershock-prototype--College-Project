using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using WebSocketSharp;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    // Create a GetPhotonFriends event
    public static Action GetPhotonFriends = delegate {  };

    [Header("Create Room Menu")]
    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private Toggle privateRoomToggle;
    private bool privateRoom = true;
    
    [Header("Error Menu")]
    [SerializeField] private TMP_Text errorText;
    
    [Header("Find Room Menu")]
    [SerializeField] private Transform roomListContent;
    [SerializeField] private GameObject roomListItemPrefab;
    private string roomName;

    [Header("Room Menu")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject playerListItemPrefab;
    [SerializeField] private GameObject startGameButton;

    private void Awake()
    {
        Instance = this;

        // Subscribes the OnRoomInviteAccept event to the HandleRoomInviteAccept function
        UIInvite.OnRoomInviteAccept += HandleRoomInviteAccept;
    }

    private void OnDestroy()
    {
        // Unsubscribes the OnRoomInviteAccept event to the HandleRoomInviteAccept function
        UIInvite.OnRoomInviteAccept -= HandleRoomInviteAccept;
    }

    // Connects to Photon master server using settings defined in "Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings"
    private void Start()
    {
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();
    }

    // Joins a photon lobby
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        PhotonNetwork.NickName = PlayerPrefs.GetString("USERNAME");
        
        MenuManager.Instance.OpenMenu("title");
        
        GetPhotonFriends?.Invoke();

        if (!roomName.IsNullOrEmpty())
        {
            JoinRoomWithName();
        }
    }
    
    public void SetPrivate(bool _privateRoom)
    {
        privateRoom = !_privateRoom;
    }

    // Allows users to create a room
    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomName)) return;
        
        PhotonNetwork.CreateRoom(roomName, new RoomOptions{ IsVisible = privateRoom, MaxPlayers = 6 });
        MenuManager.Instance.OpenMenu("loading");
    }

    // When a player joins a room run the code below
    public override void OnJoinedRoom()
    {
        // Opens the room menu
        MenuManager.Instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name + " - Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/6";

        // Creates a array of players in current room
        Player[] players = PhotonNetwork.PlayerList;

        // Deletes all child objects from playerListContent when joining a room (Fixes bug that shows old players from previous rooms)
        foreach (Transform child in playerListContent) Destroy(child.gameObject);

        // Instantiates the playerListItemPrefab as a child of playerListContent and runs the SetUp function
        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }
        
        // Enables start button game for the host of the room only
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);

        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            startGameButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            startGameButton.GetComponent<Button>().interactable = false;
        }

        roomName = "";
        roomNameInputField.text = "";
        privateRoomToggle.isOn = false;
    }

    // Runs when host of a room leaves
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    // Creates an error message if room creation fails and opens an error page
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu("error");
    }

    // Starts the game
    public void StartGame()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    // Allows the player to leave the room they are in
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }

    // Sets the room name to whatever is in the input box
    public void SetRoomName(string _roomName)
    {
        roomName = _roomName;
    }

    // Joins a room using its name as a string
    public void JoinRoomWithName()
    {
        if (string.IsNullOrEmpty(roomName)) return;
        
        PhotonNetwork.JoinRoom(roomName);
    }

    // Allows the player to join a room
    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("loading");
    }
    
    private void HandleRoomInviteAccept(string _roomName)
    {
        roomName = _roomName;
        
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveLobby();
        }
        else
        {
            if (PhotonNetwork.InLobby)
            {
                JoinRoomWithName();
            }
        }
    }

    // When a player leaves a room open the main menu page
    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");
    }

    // Update room list
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Destroy all children of roomListContent
        foreach (Transform trans in roomListContent) Destroy(trans.gameObject);
        
        // Loops through roomList
        for (int i = 0; i < roomList.Count; i++)
        {
            if(roomList[i].RemovedFromList) continue;
            
            // Instantiates roomListItemPrefab as a child of roomListContent and runs SetUp function
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    // When a player joins a room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Instantiates playerListItemPrefab as a child of playerListContent and runs SetUp function
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
        
        // Sets player count
        roomNameText.text = PhotonNetwork.CurrentRoom.Name + " - Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/6";
        
        // Checks if more than 2 players and if less then set interactable to false
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            startGameButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            startGameButton.GetComponent<Button>().interactable = false;
        }
    }
    
    // When a player leaves a room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Sets player count
        roomNameText.text = PhotonNetwork.CurrentRoom.Name + " - Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/6";
        
        // Checks if more than 2 players and if less then set interactable to false
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            startGameButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            startGameButton.GetComponent<Button>().interactable = false;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}