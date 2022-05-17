using System;
using UnityEngine;
using Photon.Chat;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PhotonChatController : MonoBehaviour, IChatClientListener
{
    [SerializeField] private string nickName;
    private ChatClient chatClient;
    
    // Creates an OnRoomInite event
    public static Action<string, string> OnRoomInvite = delegate { };
    // Creates an OnChatConnected event
    public static Action<ChatClient> OnChatConnected = delegate { };
    // Creates an OnStatusUpdated event
    public static Action<PhotonStatus> OnStatusUpdated = delegate { };

    private void Awake()
    {
        // Get player name from PlayerPrefs
        nickName = PlayerPrefs.GetString("USERNAME");
        
        // Subscribes OnInviteFriend event to the HandleFriendInvite
        UIFriend.OnInviteFriend += HandleFriendInvite;
    }
    
    private void OnDestroy()
    {
        // Unsubscribes OnInviteFriend event from the HandleFriendInvite
        UIFriend.OnInviteFriend -= HandleFriendInvite;
    }

    private void Start()
    {
        // Saves this script as chatClient
        chatClient = new ChatClient(this);
        // Runs connect to photon chat function
        ConnectToPhotonChat();
    }

    private void Update()
    {
        chatClient.Service();
    }

    private void ConnectToPhotonChat()
    {
        Debug.Log("Connecting to Photon Chat");
        // Sets authentication values
        chatClient.AuthValues = new Photon.Chat.AuthenticationValues(nickName);
        // Sets chat settings
        ChatAppSettings chatSettings = new ChatAppSettings { AppIdChat = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, AppVersion = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion, FixedRegion = PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion };
        // Connects to photon chat using settings and authentication values
        chatClient.ConnectUsingSettings(chatSettings);
    }
    
    // On friend invite send a private message to the server with recipient and current rooms name
    public void HandleFriendInvite(string recipient)
    {
        chatClient.SendPrivateMessage(recipient, PhotonNetwork.CurrentRoom.Name);
    }
    
    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log($"Photon Chat DebugReturn: {message}");
    }

    public void OnDisconnected()
    {
        Debug.Log("Disconnected from the Photon Chat");
    }

    // Ran when PhotonChat is connected
    public void OnConnected()
    {
        Debug.Log("Connected to the Photon Chat");
        // Invokes OnChatConnected event
        OnChatConnected?.Invoke(chatClient);
        // Sets status online
        chatClient.SetOnlineStatus(ChatUserStatus.Online);
    }

    public void OnChatStateChange(ChatState state)
    {
        Debug.Log($"Photon Chat OnChatStateChange: {state.ToString()}");
    }

    // Loops through messages sent to the server
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        Debug.Log($"Photon Chat OnGetMessages {channelName}");
        for (int i = 0; i < senders.Length; i++)
        {
            Debug.Log($"{senders[i]} messaged: {messages[i]}");
        }
    }
    
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        // Checks if message string is empty
        if (!string.IsNullOrEmpty(message.ToString()))
        {
            // Channel Name format [Sender : Recipient]
            string[] splitNames = channelName.Split(new char[] { ':' });
            string senderName = splitNames[0];

            if (!sender.Equals(senderName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"{sender}: {message}");
                // Invokes OnRoomInvite event
                OnRoomInvite?.Invoke(sender, message.ToString());
            }
        }
    }
    
    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log($"Photon Chat OnSubscribed");
        for (int i = 0; i < channels.Length; i++)
        {
            Debug.Log($"{channels[i]}");
        }
    }

    public void OnUnsubscribed(string[] channels)
    {
        Debug.Log($"Photon Chat OnUnsubscribed");
        for (int i = 0; i < channels.Length; i++)
        {
            Debug.Log($"{channels[i]}");
        }
    }

    // Gets status updates
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Debug.Log($"Photon Chat OnStatusUpdate: {user} changed to {status}: {message}");
        // Saves username & status in a variable 
        PhotonStatus newStatus = new PhotonStatus(user, status, (string)message);
        Debug.Log($"Status Update for {user} and its now {status}.");
        // Invokes the OnStatusUpdated event
        OnStatusUpdated?.Invoke(newStatus);
    }

    public void OnUserSubscribed(string channel, string user)
    {
        Debug.Log($"Photon Chat OnUserSubscribed: {channel} {user}");
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        Debug.Log($"Photon Chat OnUserUnsubscribed: {channel} {user}");
    }
}