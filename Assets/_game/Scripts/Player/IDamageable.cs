using Photon.Realtime;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, Player killer, Sprite weaponIcon);
}
