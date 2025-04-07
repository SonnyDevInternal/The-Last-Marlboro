using UnityEngine;

[System.Serializable]
public struct GunSettings
{
    public float gunRayDistance;
    public float gunShootTimer;
}

public class BaseGun : Item
{
    [SerializeField]
    private GameObject hitCubesPrefab = null;

    [SerializeField]
    private GameObject lightObj = null;

    [SerializeField]
    protected GunSettings gunSettings = new GunSettings();

    protected int gunDamage = 4;
    protected int gunAmmo = 7;

    protected float lastGunShootTimer = 0;

    protected virtual void OnGunHitParticle(Material hitMaterial, Vector3 position, Vector3 direction)
    {
        var instatiated = Instantiate(hitCubesPrefab, position, Quaternion.Euler(Vector3.zero));

        instatiated.GetComponent<BaseGunParticles>().StartParticle(direction, hitMaterial, 2.0f);
    }

    private void OnGunHit(Hitbox hitbox)
    {
        if(hitbox)
            hitbox.DamageEnemy(gunDamage);
    }

    protected override void OnUseItem()
    {
        if (Time.unscaledTime - lastGunShootTimer < gunSettings.gunShootTimer)
            return;

        lastGunShootTimer = Time.unscaledTime + gunSettings.gunShootTimer;

        var owningPlayer = GetOwningPlayer();

        if(owningPlayer )
        {
            var localPlayerCam = owningPlayer.GetComponent<LocalPlayer>().GetPlayerCamera();

            if(localPlayerCam != null)
            {
                var ray = localPlayerCam.ScreenPointToRay(new Vector3(0.5f, 0.5f));
                var lcTransform = localPlayerCam.transform;
#if DEBUG
                Debug.DrawRay(lcTransform.position, lcTransform.forward * gunSettings.gunRayDistance, Color.green, 10.0f);
#endif

                if (Physics.Raycast(lcTransform.position, lcTransform.forward, out RaycastHit hit, gunSettings.gunRayDistance, ~(EnemyBase.GetPlayerLayerMask() | (1 << IgnoreRaycastLayer))))
                {
                    var currentLayer = hit.transform.gameObject.layer;

                    if (1 << currentLayer == EnemyBase.GetPlayerLayerMask())
                        return;

                    if (currentLayer == hitboxLayer)
                    {
                        Debug.Log("Hit Enemy!");

                        var hitBox = hit.transform.GetComponent<Hitbox>();

                        OnGunHitParticle(hitBox.GetHitMaterial(), hit.point, hit.point - lcTransform.position);
                        OnGunHit(hitBox);
                    }
                    else
                    {
                        var meshRenderer = hit.transform.GetComponent<MeshRenderer>();

                        if(meshRenderer != null && meshRenderer.material)
                            OnGunHitParticle(meshRenderer.material, hit.point, hit.point - lcTransform.position);
                    }
                }
            }
        }
    }

    protected override void OnItemChangedParent(Transform newParent, bool worldPosStays)
    {
        if(!worldPosStays)
        {
            transform.parent.SetParent(newParent, false);
            transform.parent.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));

            transform.localRotation = Quaternion.Euler(-90.0f, 0.0f, -90.0f);
        }
    }

    protected override void OnPickUpItem_Implementation(GameObject storingLocation)
    {
        if(lightObj)
            lightObj.SetActive(false);
    }

    protected override void OnDropItem_Implementation(Vector3 dropPosition)
    {
        if (lightObj)
            lightObj.SetActive(true);
    }
}
