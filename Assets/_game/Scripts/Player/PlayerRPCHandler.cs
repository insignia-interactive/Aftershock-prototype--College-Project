using Photon.Pun;
using UnityEngine;

public class PlayerRPCHandler : MonoBehaviourPunCallbacks
{
    [SerializeField] private PlayerController _playerController;

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        _playerController.RPC_TakeDamageEvent(damage);
    }
}
