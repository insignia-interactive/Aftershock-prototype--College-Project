using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerRPCHandler : MonoBehaviourPunCallbacks
{
    [SerializeField] private PlayerController _playerController;

    [PunRPC]
    void RPC_TakeDamage(float damage, Player killer, Sprite weaponIcon)
    {
        _playerController.RPC_TakeDamageEvent(damage, killer, weaponIcon);
    }
}