using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Customisation/New Gun")]
public class GunInfo : ItemInfo
{
    [Header("Damage")]
    public float head = 0f;
    public float upperTorso = 0f;
    public float lowerTorso = 0f;
    public float limb = 0f;

    [Header("Stats")]
    public float fireRate = 0f;
    public float bulletSpeed = 1000f;
    public float bulletDrop = 0f;
    public float bulletLife = 10f;
    public float reloadTime = 0f;
    public int magSize = 0;
    public int pocketMags = 0;

    [Header("Audio")]
    public AudioClip shot;
    public AudioClip reload;
}
