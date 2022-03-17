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
        // Subscribes OnStatusUpdated event to HandleStatusUpdated
        PhotonChatController.OnStatusUpdated += HandleStatusUpdated;
        // Subscribes OnStatusUpdated event to HandleStatusUpdated
        PhotonChatFriendController.OnStatusUpdated += HandleStatusUpdated;
    }

    private void OnDisable()
    {
        // Unsubscribes OnStatusUpdated event from HandleStatusUpdated
        PhotonChatController.OnStatusUpdated -= HandleStatusUpdated;
        // Unsubscribes OnStatusUpdated event from HandleStatusUpdated
        PhotonChatFriendController.OnStatusUpdated -= HandleStatusUpdated;
    }

    private void OnEnable()
    {
        if(string.IsNullOrEmpty(friendName)) return;
        // Invokes OnGetCorrectStatus event
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
        // Invokes OnGetCorrectStatus event
        OnGetCorrectStatus?.Invoke(friendName);
    }
    
    private void HandleStatusUpdated(PhotonStatus status)
    {
        if (string.Compare(friendName, status.PlayerName) == 0)
        {
            Debug.Log($"Updating status in UI for {status.PlayerName} to status {status.Status}");
            // Sets online status
            SetStatus(status.Status);
        }
    }

    private void SetupUI()
    {
        // Sets friends username text
       friendNameText.SetText(friendName);
       // Sets invite button active
       inviteButton.SetActive(false);
    }
    
    private void SetStatus(int status)
    {
        // If friend is online
        if (status == ChatUserStatus.Online)
        {
            // Sets status image to online color
            onlineImage.color = onlineColor;
            isOnline = true;
            // Invokes OnGetRoomStatus event
            OnGetRoomStatus?.Invoke();
            // Sets inviteButton to active
            inviteButton.SetActive(true);
        }
        else
        {
            // Sets status image to offline color
            onlineImage.color = offlineColor;
            isOnline = false;
            // Sets inviteButton to inactive
            inviteButton.SetActive(false);
        }
    }

    public void RemoveFriend()
    {
        Debug.Log($"Clicked to remove friend {friendName}");
        // Invokes OnRemoveFriend event
        OnRemoveFriend?.Invoke(friendName);
    }

    public void InviteFriend()
    {
        Debug.Log($"Clicked to invite friend {friendName}");
        // Invokes OnInviteFriend event
        OnInviteFriend?.Invoke(friendName);
    }
}