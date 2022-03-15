using System;
using TMPro;
using UnityEngine;

public class UIInvite : MonoBehaviour
{
    [SerializeField] private string friendName;
    [SerializeField] private string roomName;
    [SerializeField] private TMP_Text friendNameText;
    
    public static Action<UIInvite> OnInviteAccept = delegate {  };
    public static Action<string> OnRoomInviteAccept = delegate {  };
    public static Action<UIInvite> OnInviteDecline = delegate {  };
    
    public void Initialize(string _friendName, string _roomName)
    {
        friendName = _friendName;
        roomName = _roomName;
        
        friendNameText.SetText(friendName);
    }

    public void AcceptInvite()
    {
        OnInviteAccept?.Invoke(this);
        OnRoomInviteAccept?.Invoke(roomName);
    }

    public void DeclineInvite()
    {
        OnInviteDecline?.Invoke(this);
    }
}
