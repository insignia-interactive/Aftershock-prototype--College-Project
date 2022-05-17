using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text text;
    private Player player;
    
    // Sets itself to the NickName of the player
    public void SetUp(Player _player)
    {
        player = _player;
        text.text = player.NickName;
    }

    // If a player leaves with the same player data as this item then destroy this object
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (player == otherPlayer) Destroy(gameObject);
    }

    // When the player leaves the room destroy this object
    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}