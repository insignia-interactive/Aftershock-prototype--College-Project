using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDisplayInvites : MonoBehaviour
{
    [SerializeField] private Transform inviteContainer;
    [SerializeField] private UIInvite UIInvitePrefab;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private Vector2 originalSize;
    [SerializeField] private Vector2 increaseSize;

    private List<UIInvite> invites;

    private void Awake()
    {
        invites = new List<UIInvite>();
        
        contentRect = inviteContainer.GetComponent<RectTransform>();
        originalSize = contentRect.sizeDelta;
        increaseSize = new Vector2(0, UIInvitePrefab.GetComponent<RectTransform>().sizeDelta.y);
        
        // Subscribes the OnRoomInvite event to the HandleRoomInvite function
        PhotonChatController.OnRoomInvite += HandleRoomInvite;
        // Subscribes the OnInviteAccept event to the HandleInviteAccept function
        UIInvite.OnInviteAccept += HandleInviteAccept;
        // Subscribes the OnInviteDecline event to the HandleInviteDecline function
        UIInvite.OnInviteDecline += HandleInviteDecline;
    }
    
    private void OnDestroy()
    {
        // Unsubscribes the OnRoomInvite event to the HandleRoomInvite function
        PhotonChatController.OnRoomInvite -= HandleRoomInvite;
        // Unsubscribes the OnInviteAccept event to the HandleInviteAccept function
        UIInvite.OnInviteAccept -= HandleInviteAccept;
        // Unsubscribes the OnInviteDecline event to the HandleInviteDecline function
        UIInvite.OnInviteDecline -= HandleInviteDecline;
    }

    private void HandleRoomInvite(string friend, string room)
    {
        Debug.Log($"Room invite for {friend} to room: {room}");
        UIInvite uiInvite = Instantiate(UIInvitePrefab, inviteContainer);
        uiInvite.Initialize(friend, room);
        contentRect.sizeDelta += increaseSize;
        
        invites.Add(uiInvite);
    }
    
    private void HandleInviteAccept(UIInvite invite)
    {
        if (invites.Contains(invite))
        {
            invites.Remove(invite);
            Destroy(invite.gameObject);
        }
    }
    
    private void HandleInviteDecline(UIInvite invite)
    {
        if (invites.Contains(invite))
        {
            invites.Remove(invite);
            Destroy(invite.gameObject);
        }
    }
}
