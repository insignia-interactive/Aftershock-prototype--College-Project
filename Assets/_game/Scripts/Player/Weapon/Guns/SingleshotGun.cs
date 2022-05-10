using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SingleshotGun : Gun
{
    private UpdateAmmoDisplay _updateAmmoDisplay;

    private int magSize;
    private int pocketMags;
    
    public int currentMagAmount;
    public int currentPocketAmount;
    
    private bool CanShoot = true;
    private bool IsReloading = false;
    
    class Bullet
    {
        public float time;
        public Vector3 initialPosition;
        public Vector3 initialVelocity;
    }
    
    [Header("Raycasting")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private LayerMask invisLayers;
    private Ray ray;

    private List<Bullet> bullets = new List<Bullet>();

    private void Start()
    {
        cam = Camera.main;

        magSize = ((GunInfo) itemInfo).magSize;
        pocketMags = ((GunInfo) itemInfo).pocketMags;

        currentMagAmount = magSize;
        currentPocketAmount = pocketMags * magSize;

        _updateAmmoDisplay = GetComponentInParent<UpdateAmmoDisplay>();
        _updateAmmoDisplay.magSize.text = currentMagAmount.ToString();
        _updateAmmoDisplay.pocketAmmo.text = currentPocketAmount.ToString();
        _updateAmmoDisplay.weaponIcon.sprite = ((GunInfo) itemInfo).weaponIcon;

        if (transform.parent.name == "Secondary")
        {
            gameObject.SetActive(false);
        }
    }

    Vector3 GetPosition(Bullet bullet)
    {
        Vector3 gravity = Vector3.down * ((GunInfo) itemInfo).bulletDrop;
        return (bullet.initialPosition) + (bullet.initialVelocity * bullet.time) + (0.5f * gravity * bullet.time * bullet.time);
    }

    Bullet CreateBullet(Vector3 pos, Vector3 vel)
    {
        Bullet bullet = new Bullet();
        bullet.initialPosition = pos;
        bullet.initialVelocity = vel;
        bullet.time = 0.0f;
        return bullet;
    }

    private void LateUpdate()
    {
        UpdateBullets(Time.deltaTime);
    }

    public override void Use()
    {
        Shoot();
    }

    public void UpdateBullets(float deltaTime)
    {
        SimulateBullets(deltaTime);
        DestroyBullets();
    }

    void SimulateBullets(float deltaTime)
    {
        bullets.ForEach(bullet =>
        {
            Vector3 p0 = GetPosition(bullet);
            bullet.time += deltaTime;
            Vector3 p1 = GetPosition(bullet);
            RaycastSegment(p0, p1, bullet);
        });
    }

    void DestroyBullets()
    {
        bullets.RemoveAll(bullet => bullet.time > ((GunInfo) itemInfo).bulletLife);
    }

    void RaycastSegment(Vector3 start, Vector3 end, Bullet bullet)
    {
        Vector3 direction = end - start;
        float distance = (direction).magnitude;
        ray.origin = start;
        ray.direction = direction;
        
        LayerMask layer =  ~(1 << invisLayers);
        
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, distance, layer, QueryTriggerInteraction.Ignore);
        
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (hit.collider.gameObject.name == "Head")
            {
                Debug.Log("Headshot");
                hit.collider.gameObject.transform.root.GetComponentInChildren<PlayerController>()?.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).head, PV.Owner, ((GunInfo)itemInfo).weaponIcon, true);
                break;
            } else if (hit.collider.gameObject.name == "LowerSpine")
            {
                Debug.Log("LowerTorso Shot");
                hit.collider.gameObject.transform.root.GetComponentInChildren<PlayerController>()?.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).lowerTorso, PV.Owner, ((GunInfo)itemInfo).weaponIcon, false);
                break;
            } else if (hit.collider.gameObject.name == "UpperSpine")
            {
                Debug.Log("UpperTorso Shot");
                hit.collider.gameObject.transform.root.GetComponentInChildren<PlayerController>()?.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).upperTorso, PV.Owner, ((GunInfo)itemInfo).weaponIcon, false);
                break;
            } else if (hit.collider.gameObject.name == "RightArm" || hit.collider.gameObject.name == "RightForeArm" || hit.collider.gameObject.name == "RightHand" || hit.collider.gameObject.name == "LeftArm" || hit.collider.gameObject.name == "LeftForeArm" || hit.collider.gameObject.name == "LeftHand" || hit.collider.gameObject.name == "RightUpperLeg" || hit.collider.gameObject.name == "RightLowerLeg" || hit.collider.gameObject.name == "RightFoot" || hit.collider.gameObject.name == "LeftUpperLeg" || hit.collider.gameObject.name == "LeftLowerLeg" || hit.collider.gameObject.name == "LeftFoot")
            {
                Debug.Log("Limb Shot");
                hit.collider.gameObject.transform.root.GetComponentInChildren<PlayerController>()?.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).limb, PV.Owner, ((GunInfo)itemInfo).weaponIcon, false);
                break;
            }
            else
            {
                bullet.time = ((GunInfo) itemInfo).bulletLife;
            }
        }
    }
    
    void Shoot()
    {
        if (CanShoot && currentMagAmount > 0 && !IsReloading)
        {
            CanShoot = false;

            currentMagAmount--;
            _updateAmmoDisplay.magSize.text = currentMagAmount.ToString();
            
            Debug.Log("Shot");
        
            foreach (ParticleSystem particle in muzzleFlash)
            {
                particle.Emit(1);
            }
        
            ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            ray.origin = raycastOrigin.position;

            Vector3 velocity = (ray.direction).normalized * ((GunInfo) itemInfo).bulletSpeed;
            var bullet = CreateBullet(raycastOrigin.position, velocity);
            bullets.Add(bullet);
            
            Invoke(nameof(ResetShoot), ((GunInfo)itemInfo).fireRate);
        }
    }

    void ResetShoot()
    {
        CanShoot = true;
    }

    private void OnEnable()
    {
        _updateAmmoDisplay.holder.transform.SetAsFirstSibling();
    }

    private void OnDisable()
    {
        _updateAmmoDisplay.holder.transform.SetAsLastSibling();
    }
}
