using UnityEngine;
using Photon.Realtime;
using TMPro;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    public RoomInfo info;
    
    // Sets text on RoomMenu to match the room the player is currently in
    public void SetUp(RoomInfo _info)
    {
        info = _info;
        text.text = info.Name;
    }

    // Runs the JoinRoom function and passes the rooms info
    public void OnClick()
    {
        Launcher.Instance.JoinRoom(info);
    }
}