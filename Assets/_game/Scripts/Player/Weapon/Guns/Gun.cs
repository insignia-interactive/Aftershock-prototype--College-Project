using UnityEngine;

public abstract class Gun : Item
{
    [Header("Effects")]
    public ParticleSystem[] muzzleFlash;

    [Header("IK")]
    public Transform ref_right_hand_grip, ref_left_hand_grip;
    [Range(0, 1)]
    public float rightWeight, leftWeight; 

    public abstract override void Use();
    public abstract override void UpdateWeapon();
    public abstract void Reload();
}