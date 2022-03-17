using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using PlayfabFriendInfo = PlayFab.ClientModels.FriendInfo;

public class PhotonChatFriendController : MonoBehaviour
{        
    [SerializeField] private bool initialized;
    [SerializeField] private List<string> friendList;
    private ChatClient chatClient;
    public static Dictionary<string, PhotonStatus> friendStatuses;
    
    // Creates an OnDisplayFriends event
    public static Action<List<string>> OnDisplayFriends = delegate { };
    // Creates an OnStatusUpdated event
    public static Action<PhotonStatus> OnStatusUpdated = delegate { };

    private void Awake()
    {
        friendList = new List<string>();
        friendStatuses = new Dictionary<string, PhotonStatus>();

        // Subscribes the OnFriendListUpdated event to the HandleFriendsUpdated function
        PlayfabFriendController.OnFriendListUpdated += HandleFriendsUpdated;
        // Subscribes the OnChatConnected event to the HandleChatConnected function
        PhotonChatController.OnChatConnected += HandleChatConnected;
        // Subscribes the OnStatusUpdated event to the HandleStatusUpdated function
        PhotonChatController.OnStatusUpdated += HandleStatusUpdated;
        // Subscribes the OnGetCorrectStatus event to the HandleGetCorrentStatus function
        UIFriend.OnGetCorrectStatus += HandleGetCorrentStatus;
    }

    private void OnDestroy()
    {
        // Unsubscribes the OnFriendListUpdated event from the HandleFriendsUpdated function
        PlayfabFriendController.OnFriendListUpdated -= HandleFriendsUpdated;
        // Unsubscribes the OnChatConnected event from the HandleChatConnected function
        PhotonChatController.OnChatConnected -= HandleChatConnected;
        // Unsubscribes the OnStatusUpdated event from the HandleStatusUpdated function
        PhotonChatController.OnStatusUpdated -= HandleStatusUpdated;
        // Unsubscribes the OnGetCorrectStatus event from the HandleGetCorrentStatus function
        UIFriend.OnGetCorrectStatus -= HandleGetCorrentStatus;
    }

    private void HandleFriendsUpdated(List<PlayfabFriendInfo> friends)
    {
        // Saves friends into a list
        friendList = friends.Select(f => f.TitleDisplayName).ToList();
        // RemovePhotonFriends function
        RemovePhotonFriends();
        // FindPhotonFriends function
        FindPhotonFriends();
    }
    
    private void HandleChatConnected(ChatClient client)
    {
        chatClient = client;
        // RemovePhotonFriends function
        RemovePhotonFriends();
        // FindPhotonFriends function
        FindPhotonFriends();
    }

    private void HandleStatusUpdated(PhotonStatus status)
    {
        // Gets friends status
        if(friendStatuses.ContainsKey(status.PlayerName))
        {
            friendStatuses[status.PlayerName] = status;
        }
        else
        {
            friendStatuses.Add(status.PlayerName, status);
        }
    }

    private void HandleGetCorrentStatus(string name)
    {
        PhotonStatus status;
        if (friendStatuses.ContainsKey(name))
        {
            status = friendStatuses[name];
        }
        else
        {
            status = new PhotonStatus(name, 0, "");
        }
        // Invokes OnStatusUpdated event
        OnStatusUpdated?.Invoke(status);
    }

    private void RemovePhotonFriends()
    {
        if(friendList.Count > 0 && initialized)
        {
            string[] friendDisplayNames = friendList.ToArray();
            chatClient.RemoveFriends(friendDisplayNames);
        }
    }

    private void FindPhotonFriends()
    {
        if (chatClient == null) return;
        if (friendList.Count != 0)
        {
            initialized = true;
            string[] friendDisplayNames = friendList.ToArray();                
            chatClient.AddFriends(friendDisplayNames);                
        }
        OnDisplayFriends?.Invoke(friendList);
    }
}