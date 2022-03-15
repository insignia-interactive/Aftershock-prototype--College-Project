using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabFriendController : MonoBehaviour
{
    // Creates a OnFriendListUpdate event
    public static Action<List<FriendInfo>> OnFriendListUpdated = delegate {  };
    private List<FriendInfo> friends;

    private void Awake()
    {
        friends = new List<FriendInfo>();

        // Subscribes GetPhotonFriends to the HandleGetFriends function
        Launcher.GetPhotonFriends += HandleGetFriends;
        // Subscribes OnAddFriend to the HandleAddPlayfabFriend function
        UIAddFriend.OnAddFriend += HandleAddPlayfabFriend;
        // Subscribes OnRemoveFriend to the HandleRemoveFriend function
        UIFriend.OnRemoveFriend += HandleRemoveFriend;
    }

    private void OnDestroy()
    {
        // Unsubscribes from GetPhotonFriends
        Launcher.GetPhotonFriends -= HandleGetFriends;
        // Unsubscribes from OnAddFriend
        UIAddFriend.OnAddFriend -= HandleAddPlayfabFriend;
        // Unsubscribes from OnRemoveFriend
        UIFriend.OnRemoveFriend -= HandleRemoveFriend;
    }

    // Handles Add Friend Requests
    private void HandleAddPlayfabFriend(string name)
    {
        Debug.Log($"Playfab add friend request for {name}");
        var request = new AddFriendRequest { FriendTitleDisplayName = name };
        PlayFabClientAPI.AddFriend(request, OnFriendAddedSuccess, OnError);
    }

    // Handles Remove Friend Requests
    private void HandleRemoveFriend(string name)
    {
        string id = friends.FirstOrDefault(f => f.TitleDisplayName == name).FriendPlayFabId;
        Debug.Log($"Playfab remove friend {name} with id {id}");
        var request = new RemoveFriendRequest { FriendPlayFabId = id };
        PlayFabClientAPI.RemoveFriend(request, OnFriendRemoveSuccess, OnError);
    }
    
    // Calls GetPlayfabFriends Function
    private void HandleGetFriends()
    {
        GetPlayfabFriends();
    }

    // Handles the Get Friends List Requests
    private void GetPlayfabFriends()
    {
        Debug.Log("Playfab get friend list request");
        var request = new GetFriendsListRequest { IncludeSteamFriends = false, IncludeFacebookFriends = false, XboxToken = null };
        PlayFabClientAPI.GetFriendsList(request, OnFriendsListSuccess, OnError);
    }
    
    // Called when friend added successfully
    private void OnFriendAddedSuccess(AddFriendResult result)
    {
        Debug.Log("Playfab add friend success getting updated friend list");
        GetPlayfabFriends();
    }

    // Called when friends list received successfully 
    private void OnFriendsListSuccess(GetFriendsListResult result)
    {
        Debug.Log($"Playfab get friend list success: {result.Friends.Count}");
        
        friends = result.Friends;
        
        OnFriendListUpdated.Invoke(result.Friends);
    }
    
    // Called when friend removed successfully 
    private void OnFriendRemoveSuccess(RemoveFriendResult result)
    {
        Debug.Log("Removed friend");
        GetPlayfabFriends();
    }

    // Called on error
    private void OnError(PlayFabError error)
    {
        Debug.Log($"Error: {error.GenerateErrorReport()}");
    }
}
