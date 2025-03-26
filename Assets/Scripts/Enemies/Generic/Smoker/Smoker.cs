using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct SmokerSettings
{
    public float patrollingSpeed;
    public float followingSpeed;

    public float SmokeAttackDelay;
    public float TimeTillSmokeReachPlayer;

    public SmokeCloudSettings cloudSettings;

    public bool hasAttackDelay;
}

public class Smoker : EnemyBase
{
    [SerializeField]
    private SmokerSettings smokerSettings = new SmokerSettings();

    private SmokerAnimationHandler smokerHandler = null;

    [SerializeField]
    private GameObject smokeCloudPrefab = null;

    [SerializeField]
    private UI_Bar3D healthBar = null;

    private float SmokeAttackDelayCurrent = 0.0f;

    private bool hasSmokeAttackDelay = false;
    private bool isAttacking = false;

    private bool hasSmokerHealthbar = false;

    protected override void OnStartAgent()
    {
        smokerHandler = GetComponentInChildren<SmokerAnimationHandler>();

        hasSmokerHealthbar = healthBar != null;

        if (hasSmokerHealthbar)
            OnEnemyHealthChanged(0);

        if ( smokerHandler != null )
            smokerHandler.BindOnAnimationHandlerCalled(OnSmokerAnimationEvent);
    }

    protected override void OnEnemyDeath(EDeathSource source)
    {
        base.OnEnemyDeath(source);

        if(source == EDeathSource.Scene && smokerHandler != null)
        {
            smokerHandler.UnbindOnAnimationHandlerCalled(OnSmokerAnimationEvent);

            hasSmokerHealthbar = false;
        }
        else
            animator.SetTrigger("Death");
    }

    protected override void OnEnemyHealthChanged(int valueChangedBy)
    {
        if (hasSmokerHealthbar)
            healthBar.SetPercentage((float)GetHealth() / (float)enemySettings.enemyMaxHealth);
    }

    protected override void OnUpdateAgent()
    {
        if(hasSmokeAttackDelay)
        {
            if (SmokeAttackDelayCurrent >= smokerSettings.SmokeAttackDelay)
            {
                hasSmokeAttackDelay = false;

                SmokeAttackDelayCurrent = 0.0f;

                ResetSmoker();
            }
            else
                SmokeAttackDelayCurrent += Time.deltaTime;
        }

        if (hasSmokerHealthbar && hasTargetingPlayer)
        {
            var targetRotation = Quaternion.LookRotation(TargetingPlayer.transform.position - transform.position);

            var targetVec = targetRotation.eulerAngles; targetVec.z = 0;

            healthBar.transform.rotation = Quaternion.Euler(targetVec);
        }
    }

    protected override void OnSwitchedEnemyState(EEnemyState state)
    {
        switch (state)
        {
            case EEnemyState.Idiling:
                break;
            case EEnemyState.Patrolling:
                break;
            case EEnemyState.TargetingPlayer:
                break;
            case EEnemyState.AttackPlayer:
                break;
            default:
                break;
        }
    }

    private void ResetSmoker()
    {
        agent.isStopped = false;

        isAttacking = false;

        enemySettings.shouldLookAtPlayer = true;
    }

    private void OnSmokerAnimationEvent(AnimationHandler<ESmokerAnimationEvent> _this, ESmokerAnimationEvent Event)
    {
        switch (Event)
        {
            case ESmokerAnimationEvent.OnSmokeSpawn:
                var spawnedCloud = Instantiate(smokeCloudPrefab, head.position + head.forward * 2, Quaternion.Euler(Vector3.zero));

                if(spawnedCloud)
                {
                    var cloudComponent = spawnedCloud.GetComponent<SmokeCloudController>();

                    if(cloudComponent)
                    {
                        cloudComponent.Activate(TargetingPlayer.transform.position, smokerSettings.TimeTillSmokeReachPlayer, smokerSettings.cloudSettings);
                    }
                }
                break;

            case ESmokerAnimationEvent.OnSmokeEnd:
                hasSmokeAttackDelay = false;

                if (smokerSettings.hasAttackDelay)
                {
                    SmokeAttackDelayCurrent = 0.0f;

                    hasSmokeAttackDelay = true;

                    enemySettings.shouldLookAtPlayer = false;
                }
                else
                {
                    ResetSmoker();
                }

                break;

            case ESmokerAnimationEvent.OnDeath:
                if(smokerHandler != null)
                    smokerHandler.UnbindOnAnimationHandlerCalled(OnSmokerAnimationEvent);

                Destroy(gameObject);

                break;

            default:
                break;
        }
    }


    protected override void OnMoveAgent(Vector3 position)
    {
        if(!isAttacking)
            base.OnMoveAgent(position);
    }

    protected override void OnAttack()
    {
        isAttacking = true;

        agent.isStopped = true;

        animator.SetTrigger("SmokingAttack");
    }

    protected override bool CanAttack()
    {
        return base.CanAttack() && !isAttacking && !hasSmokeAttackDelay;
    }
}
