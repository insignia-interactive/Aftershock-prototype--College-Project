using UnityEngine;

public abstract class Gun : Item
{
    [Header("Effects")]
    public ParticleSystem[] muzzleFlash;
    
    
    public abstract override void Use();
}
