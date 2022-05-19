using UnityEngine;

public abstract class Gun : Item
{
    [Header("Effects")]
    public ParticleSystem[] muzzleFlash;
    public GameObject dropMag;

    [Header("IK")]
    public Transform ref_right_hand_grip;
    public Transform ref_left_hand_grip;
    [Range(0, 1)]
    public float rightWeight, leftWeight; 

    public abstract override void Use(bool isShooting);
    public abstract override void UpdateWeapon();
    public abstract void Reload();
    public void DropMag()
    {
        Destroy(Instantiate(dropMag), 5f);
    }
}