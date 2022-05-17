using System;
using System.Linq;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PhotonFriendInfo = Photon.Realtime.FriendInfo;
using PlayfabFriendInfo = PlayFab.ClientModels.FriendInfo;

public class PhotonFriendController : MonoBehaviourPunCallbacks
{
    [SerializeField] private float refreshCooldown;
    [SerializeField] private float refreshCountdown;
    [SerializeField] private List<PlayfabFriendInfo> friendList;
    
    // Creates the OnDisplayFriends event
    public static Action<List<PhotonFriendInfo>> OnDisplayFriends = delegate {  };

    private void Awake()
    {
        friendList = new List<PlayfabFriendInfo>();
        PlayfabFriendController.OnFriendListUpdated += HandleFriendsUpdated;
    }

    private void OnDestroy()
    {
        // Unsubscribes the OnFriendList Update to the HandleFriendsUpdated function
        PlayfabFriendController.OnFriendListUpdated -= HandleFriendsUpdated;
    }
    
    private void Update()
    {
        if (refreshCountdown > 0)
        {
            refreshCountdown -= Time.deltaTime;
        }
        else
        {
            refreshCountdown = refreshCooldown;
            if (PhotonNetwork.InRoom) return;
            FindPhotonFriends(friendList);
        }
    }
    
    // Updates friends list
    private void HandleFriendsUpdated(List<PlayfabFriendInfo> friends)
    {
        friendList = friends;
        FindPhotonFriends(friendList);
    }
    
    private static void FindPhotonFriends(List<PlayfabFriendInfo> friends)
    {
        if (friends.Count != 0)
        {
            string[] friendDisplayNames = friends.Select(f => f.TitleDisplayName).ToArray();
            PhotonNetwork.FindFriends(friendDisplayNames);
        }
        else
        {
            List<PhotonFriendInfo> friendList = new List<PhotonFriendInfo>();
            OnDisplayFriends?.Invoke(friendList);
        }
    }

    // When friend list updated Invoke the OnDisplayFriends event
    public override void OnFriendListUpdate(List<PhotonFriendInfo> friendList)
    {
        OnDisplayFriends?.Invoke(friendList);
    }
}