using System;
using UnityEngine;

public class UIAddFriend : MonoBehaviour
{
    [SerializeField] private string displayName;
    
    // Creates an OnAddFriend event
    public static Action<string> OnAddFriend = delegate {  };

    // Sets displayName to whatever the input box is
    public void SetAddFriendName(string name)
    {
        displayName = name;
    }

    // Invokes the OnAddFriend event
    public void AddFriend()
    {
        Debug.Log($"UI Add Friend Clicked: {displayName}");
        
        if(string.IsNullOrEmpty(displayName)) return;
        
        OnAddFriend?.Invoke(displayName);
    }
}
