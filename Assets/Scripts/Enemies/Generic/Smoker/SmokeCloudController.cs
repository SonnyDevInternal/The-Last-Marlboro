using UnityEngine;

[System.Serializable]
public struct SmokeCloudSettings
{
    public float SmokeDecayTime;
    public float TimeTillMaxStackedDamage;
    public float TimeTillDamage;

    public int SmokeBasicDamage;
    public int SmokeMaxStackedDamage;
}

public class SmokeCloudController : MonoBehaviour
{
    private static bool hasDamagedPlayerThisFrame = false;

    private BoxCollider owningCollider = null;

    private SmokeCloudSettings cloudSettings = new SmokeCloudSettings();

    private Vector3 startingPosition = Vector3.zero;
    private Vector3 endPosition = Vector3.zero;

    private float endTime = 0.0f;
    private float currentTime = 0.0f;

    private float TimeTillDamage_Current = 0.0f;
    private float TimeTillSmokeDecay_Current = 0.0f;

    private int cloudCurrentDamage = 0;

    private bool isActivated = false;
    private bool hasReachedEnd = false;

    private void Start()
    {
        owningCollider = GetComponent<BoxCollider>();

#if DEBUG
        if((owningCollider.excludeLayers.value & LayerMask.GetMask("Default")) == 0)
        {
            Debug.LogWarning("Owning Collider doesn't exclude Default Layers. Expect Performance Issues!");
        }

        if((owningCollider.includeLayers.value & EnemyBase.GetPlayerLayerMask()) == 0)
        {
            throw new System.Exception("Owning Collider doesn't expect the Player Layer in the Include Layermask! Add the Player LayerMask in the Include Layer of the Collider!");
        }
#endif
    }

    private void Update()
    {
        if(isActivated)
        {
            if(currentTime < endTime)
            {
                currentTime += Time.deltaTime;

                if (currentTime >= endTime)
                    currentTime = endTime;

                transform.position = Vector3.Lerp(startingPosition, endPosition, (currentTime / endTime));
            }
            else
            {
                hasReachedEnd = true;

                if (TimeTillSmokeDecay_Current >= cloudSettings.SmokeDecayTime)
                {
                    hasReachedEnd = false;
                    isActivated = false;

                    Destroy(gameObject);
                }
                else
                {
                    UpdateDecayEffect(TimeTillSmokeDecay_Current / cloudSettings.SmokeDecayTime);

                    cloudCurrentDamage = Mathf.RoundToInt(Mathf.Lerp(cloudSettings.SmokeBasicDamage, cloudSettings.SmokeMaxStackedDamage, (TimeTillSmokeDecay_Current / cloudSettings.TimeTillMaxStackedDamage)));

                    TimeTillSmokeDecay_Current += Time.deltaTime;
                }
            }


        }
    }

    private void LateUpdate()
    {
        if(hasReachedEnd)
            hasDamagedPlayerThisFrame = false;
    }

    private void UpdateDecayEffect(float t)
    {

    }

    public void Activate(Vector3 endPosition, float InTime, SmokeCloudSettings cloudSettings) //Time in Seconds
    {
        this.cloudSettings = cloudSettings;
        this.endPosition = endPosition;
        this.startingPosition = transform.position;

        cloudCurrentDamage = cloudSettings.SmokeBasicDamage;

        currentTime = 0.0f;

        TimeTillDamage_Current = 0.0f;
        TimeTillSmokeDecay_Current = 0.0f;

        endTime = InTime;

        hasReachedEnd = false;

        isActivated = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if(hasReachedEnd)
        {
            if (TimeTillDamage_Current >= cloudSettings.TimeTillDamage)
            {
                if(!hasDamagedPlayerThisFrame)
                {
                    hasDamagedPlayerThisFrame = true;

                    TimeTillDamage_Current = 0.0f;

                    var player = other.GetComponent<Player>();

                    if (player != null)
                        player.DamagePlayer(cloudCurrentDamage);
                }

            }
            else
                TimeTillDamage_Current += Time.deltaTime;
        }
    }
}