using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerRPCHandler : MonoBehaviourPunCallbacks
{
    [SerializeField] private PlayerController _playerController;

    [SerializeField] private Sprite[] weaponIcons;

    [PunRPC]
    void RPC_TakeDamage(float damage, Player killer, string _weaponIcon)
    {
        Sprite weaponIcon = weaponIcons[0];

        foreach (Sprite item in weaponIcons)
        {
            if(item.name == _weaponIcon)
            {
                weaponIcon = item;
            }
        }

        _playerController.RPC_TakeDamageEvent(damage, killer, weaponIcon);
    }
}
