using System;
using System.Linq;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class UIDisplayFriends : MonoBehaviour
{
    [SerializeField] private Transform friendContainer;
    [SerializeField] private UIFriend UIFriendPrefab;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private Vector2 orginalSize;
    [SerializeField] private Vector2 increaseSize;
    
    private void Awake()
    {
        contentRect = friendContainer.GetComponent<RectTransform>();
        orginalSize = contentRect.sizeDelta;
        increaseSize = new Vector2(0, UIFriendPrefab.GetComponent<RectTransform>().sizeDelta.y);
        
        // Subscribes to the OnDisplayFriends event with the HandleDisplayFriends function
        PhotonFriendController.OnDisplayFriends += HandleDisplayFriends;
        // Subscribes to the OnDisplayFriends event with the HandleDisplayChatFriends function
        PhotonChatFriendController.OnDisplayFriends += HandleDisplayChatFriends;
    }

    private void OnDestroy()
    {
        // Unsubscribes to the OnDisplayFriends event
        PhotonFriendController.OnDisplayFriends -= HandleDisplayFriends;
        // Unsubscribes to the OnDisplayFriends event
        PhotonChatFriendController.OnDisplayFriends -= HandleDisplayChatFriends;
    }

    private void HandleDisplayFriends(List<FriendInfo> friends)
    {
        Debug.Log("UI remove prior friends displayed");
        foreach (Transform child in friendContainer)
        {
            Destroy(child.gameObject);
        }
        
        var sortedFriends = friends.OrderByDescending(o => o.IsOnline ? 1 : 0).ThenBy(u => u.UserId);

        // Loops through all friends and Instantiates a new UIFriendPrefab with the info of the friend
        foreach (FriendInfo friend in sortedFriends)
        {
            UIFriend uifriend = Instantiate(UIFriendPrefab, friendContainer);
            uifriend.Initialize(friend);
            contentRect.sizeDelta += increaseSize;
        }
    }
    
    private void HandleDisplayChatFriends(List<string> friends)
    {
        Debug.Log("UI remove prior friends displayed");
        foreach (Transform child in friendContainer)
        {
            Destroy(child.gameObject);
        }

        Debug.Log($"UI instantiate friends display {friends.Count}");
        contentRect.sizeDelta = orginalSize;

        foreach (string friend in friends)
        {
            UIFriend uifriend = Instantiate(UIFriendPrefab, friendContainer);
            uifriend.Initialize(friend);
            contentRect.sizeDelta += increaseSize;
        }
    }
}