using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Customisation/New Gun")]
public class GunObject : ScriptableObject
{
    [Header("Damage")]
    public float head = 0f;
    public float upperTorso = 0f;
    public float lowerTorso = 0f;
    public float limb = 0f;

    [Header("Stats")]
    public float fireRate = 0f;
    public float range = 0f;
    public bool isAutomatic;

    [Header("Projectile")]
    public bool isProjectile = false;
    public GameObject projectile = null;

    [Header("Audio")]
    public AudioClip shot;
    public AudioClip reload;
}
