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
    public float reloadTime = 0f;
    public float range = 0f;
    public int magSize = 0;

    [Header("Audio")]
    public AudioClip shot;
    public AudioClip reload;
}
