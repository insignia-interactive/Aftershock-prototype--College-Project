using Photon.Pun;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public ItemInfo itemInfo;
    public GameObject itemGameObject;
    public PhotonView PV;

    public abstract void Use();
    public abstract void UpdateWeapon();
}