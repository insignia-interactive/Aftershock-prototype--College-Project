using UnityEngine;

public class SingleshotGun : Gun
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask invisLayers;
    
    public override void Use()
    {
        Shoot();
    }

    void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;

        LayerMask layer =  ~(1 << invisLayers);

        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, ((GunInfo) itemInfo).range, layer, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            
            if (hit.collider.gameObject.name == "Head")
            {
                Debug.Log("Headshot");
                hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).head);
                break;
            } else if (hit.collider.gameObject.name == "LowerSpine")
            {
                Debug.Log("LowerTorso Shot");
                hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).lowerTorso);
                break;
            } else if (hit.collider.gameObject.name == "UpperSpine")
            {
                Debug.Log("UpperTorso Shot");
                hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).upperTorso);
                break;
            } else if (hit.collider.gameObject.name == "RightArm" || hit.collider.gameObject.name == "RightForeArm" || hit.collider.gameObject.name == "RightHand" || hit.collider.gameObject.name == "LeftArm" || hit.collider.gameObject.name == "LeftForeArm" || hit.collider.gameObject.name == "LeftHand" || hit.collider.gameObject.name == "RightUpperLeg" || hit.collider.gameObject.name == "RightLowerLeg" || hit.collider.gameObject.name == "RightFoot" || hit.collider.gameObject.name == "LeftUpperLeg" || hit.collider.gameObject.name == "LeftLowerLeg" || hit.collider.gameObject.name == "LeftFoot")
            {
                Debug.Log("Limb Shot");
                hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).limb);
                break;
            }
        }
    }
}
