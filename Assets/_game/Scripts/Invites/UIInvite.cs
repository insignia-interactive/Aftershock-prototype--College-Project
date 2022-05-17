using System;
using TMPro;
using UnityEngine;

public class UIInvite : MonoBehaviour
{
    [SerializeField] private string friendName;
    [SerializeField] private string roomName;
    [SerializeField] private TMP_Text friendNameText;
    
    // Creates an OnInviteAccept event
    public static Action<UIInvite> OnInviteAccept = delegate {  };
    // Creates an OnRoomInviteAccept event
    public static Action<string> OnRoomInviteAccept = delegate {  };
    // Creates an OnInviteDecline event
    public static Action<UIInvite> OnInviteDecline = delegate {  };
    
    public void Initialize(string _friendName, string _roomName)
    {
        friendName = _friendName;
        roomName = _roomName;
        
        friendNameText.SetText(friendName);
    }

    public void AcceptInvite()
    {
        // Invokes OnInviteAccept event
        OnInviteAccept?.Invoke(this);
        // Invokes OnRoomInviteAccept event
        OnRoomInviteAccept?.Invoke(roomName);
    }

    public void DeclineInvite()
    {
        // Invokes OnInviteDecline event
        OnInviteDecline?.Invoke(this);
    }
}