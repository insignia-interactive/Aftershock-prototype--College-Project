using System;
using Photon.Chat;
using TMPro;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class UIFriend : MonoBehaviour
{
    [SerializeField] private TMP_Text friendNameText;
    [SerializeField] private string friendName;
    [SerializeField] private bool isOnline;
    [SerializeField] private Image onlineImage;
    [SerializeField] private GameObject inviteButton;
    [SerializeField] private Color onlineColor;
    [SerializeField] private Color offlineColor;

    // Creates OnRemoveFriend event
    public static Action<string> OnRemoveFriend = delegate { };
    // Creates OnInviteFriend event
    public static Action<string> OnInviteFriend = delegate { };
    // Create OnGetCurrectStatus event
    public static Action<string> OnGetCorrectStatus = delegate { };
    // Create OnGetRoomStatus event
    public static Action OnGetRoomStatus = delegate { };

    private void Awake()
    {
        PhotonChatController.OnStatusUpdated += HandleStatusUpdated;
        PhotonChatFriendController.OnStatusUpdated += HandleStatusUpdated;
    }

    private void OnDisable()
    {
        PhotonChatController.OnStatusUpdated -= HandleStatusUpdated;
        PhotonChatFriendController.OnStatusUpdated -= HandleStatusUpdated;
    }

    private void OnEnable()
    {
        if(string.IsNullOrEmpty(friendName)) return;
        OnGetCorrectStatus?.Invoke(friendName);
    }

    // Initializes the player button
    public void Initialize(FriendInfo friend)
    {
        Debug.Log($"{friend.UserId} is online: {friend.IsOnline} ; in room: {friend.IsInRoom} ; room name: {friend.Room}");

        SetupUI();
    }
    
    public void Initialize(string friendName)
    {
        Debug.Log($"{friendName} is added");
        this.friendName = friendName;

        SetupUI();
        OnGetCorrectStatus?.Invoke(friendName);
    }
    
    private void HandleStatusUpdated(PhotonStatus status)
    {
        if (string.Compare(friendName, status.PlayerName) == 0)
        {
            Debug.Log($"Updating status in UI for {status.PlayerName} to status {status.Status}");
            SetStatus(status.Status);
        }
    }

    private void SetupUI()
    {
       friendNameText.SetText(friendName);
       inviteButton.SetActive(false);
    }
    
    private void SetStatus(int status)
    {
        if (status == ChatUserStatus.Online)
        {
            onlineImage.color = onlineColor;
            isOnline = true;
            OnGetRoomStatus?.Invoke();
            inviteButton.SetActive(true);
        }
        else
        {
            onlineImage.color = offlineColor;
            isOnline = false;
            inviteButton.SetActive(false);
        }
    }

    public void RemoveFriend()
    {
        Debug.Log($"Clicked to remove friend {friendName}");
        OnRemoveFriend?.Invoke(friendName);
    }

    public void InviteFriend()
    {
        Debug.Log($"Clicked to invite friend {friendName}");
        OnInviteFriend?.Invoke(friendName);
    }
}