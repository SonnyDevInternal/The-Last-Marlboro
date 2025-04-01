using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

public enum EEnemyState
{
    Idiling,
    Patrolling,
    TargetingPlayer,
    AttackPlayer
}

[System.Serializable]
public struct EnemySettings
{
    public float minAttackDistance;
    public float minFollowDistance;
    public float maxFollowDistance;

    [Tooltip("In radians per Secs")]
    public float rotateToPlayerSpeed;

   // public float idleTime;
    public float findPlayerTime;

    public float xSightDegree;
    public float zSightDegree;
    public float ySightDegree;

    public int enemyMaxHealth;

    public bool canAttackPlayer;
    public bool canBeAngered;
    public bool canPatrol;

    public bool shouldLookAtPlayer;
    public bool useBasicHitboxes;
}

[System.Serializable]
public struct EnemyPatrolable
{
    public Transform transform;
    public bool isReachable;
}

public enum EDeathSource
{
    Scene,
    Player
}

public abstract class EnemyBase : MonoBehaviour
{
    static protected Player TargetingPlayer = null;

    static protected int playerLayerMask = 0;
    static protected int obstacleLayerMask = 0;

    static private bool hasFoundLayerMasks = false;

    //An Enemy already searched for Player this frame so skip
    static private bool triedFindingPlayer = false;

    static protected bool hasTargetingPlayer = false;

    protected NavMeshAgent agent = null;
    protected Animator animator = null;

    [SerializeField]
    protected Material mainHitMaterial = null;

    [SerializeField]
    protected EnemySettings enemySettings = new EnemySettings();

    [SerializeField]
    protected List<EnemyPatrolable> patrollingTargets = new List<EnemyPatrolable>();

    [SerializeField]
    protected Transform head = null;

    private EnemyPatrolable? currentPatrollable = null;

    private EEnemyState enemyState = EEnemyState.Idiling;
    private float playerDistance = 0.0f;

    private float findPlayerTime_Current = 0.0f;
    private int patrollingPoint = 0;

    private int currentHealth = 0;

    private int ANIMATOR_characterSpeed_ID = 0;
    private int ANIMATOR_idilingTime_ID = 0;

    private bool isSeeingPlayer = false;
    private bool isDead = false;
    private bool isIdling = false;

    public delegate void OnDestroyingEnemy(EnemyBase enemy, EDeathSource deathSource);

    protected OnDestroyingEnemy onDestroyingEnemy;

    public void BindOnDestroyingEnemy(OnDestroyingEnemy enemy) { onDestroyingEnemy += enemy; }

    public void UnbindOnDestroyingEnemy(OnDestroyingEnemy enemy) { onDestroyingEnemy -= enemy; }

    void Start()
    {
        if(!hasFoundLayerMasks)
        {
            hasFoundLayerMasks = true;

            playerLayerMask = LayerMask.GetMask("Player");
            obstacleLayerMask = LayerMask.GetMask("Obstacle");
        }

#if DEBUG
        if(!head)
        {
            throw new NullReferenceException("Enemy is Missing Head GameObject Reference!");
        }
#endif

        currentHealth = enemySettings.enemyMaxHealth;

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if(enemySettings.useBasicHitboxes)
        {
            var hitboxes = GetComponentsInChildren<Hitbox>();

            for (int i = 0; i < hitboxes.Length; i++)
            {
                if(mainHitMaterial)
                    hitboxes[i].hitMaterial = mainHitMaterial;

                hitboxes[i].BindEnemyToHitbox(this);
            }
        }

        ANIMATOR_characterSpeed_ID = Animator.StringToHash("characterSpeed");
        ANIMATOR_idilingTime_ID = Animator.StringToHash("idilingTime");

        SetIdiling(true);

        OnStartAgent();
    }

    private void OnDestroy()
    {
        var deathSource = EDeathSource.Scene;

        OnEnemyDeath(deathSource);

        if (onDestroyingEnemy != null)
            onDestroyingEnemy(this, deathSource);
    }

#if DEBUG
    private void OnDrawGizmosSelected()
    {
        if (!head)
        {
            Debug.LogWarning("Can't Draw Gizmos, Missing Head GameObject!");

            return;
        }

        if(enemySettings.xSightDegree > 0.0f && enemySettings.zSightDegree > 0.0f)
        {
            var forward = head.forward;
            var pos = head.position;

            var forwardEnd = (forward * enemySettings.zSightDegree);

            var pos1 = forwardEnd + (-head.right * enemySettings.xSightDegree);
            var pos2 = forwardEnd + (head.right * enemySettings.xSightDegree);

            Debug.DrawRay(pos, pos1, Color.green);
            Debug.DrawRay(pos, pos2, Color.red);

            
            if (enemySettings.ySightDegree > 0.0f)
            {
                pos1 = forwardEnd + (-head.up * enemySettings.ySightDegree);
                pos2 = forwardEnd + (head.up * enemySettings.ySightDegree);

                Debug.DrawRay(pos, pos1, Color.white);
                Debug.DrawRay(pos, pos2, Color.yellow);
            }
        }
    }
#endif

    void Update()
    {
        if (isDead)
        {
            OnEnemyDiedUpdate();
            return;
        }

        if (!triedFindingPlayer && !hasTargetingPlayer)
        {
            triedFindingPlayer = true;

            if (findPlayerTime_Current >= enemySettings.findPlayerTime)
            {
                TargetingPlayer = FindPlayer();

                findPlayerTime_Current = 0.0f;

                if (TargetingPlayer)
                    hasTargetingPlayer = true;
            }
            else
                findPlayerTime_Current += Time.deltaTime;
        }

        if(hasTargetingPlayer)
        {
            if (!triedFindingPlayer)
            {
                triedFindingPlayer = true;

                hasTargetingPlayer = (TargetingPlayer != null);
            }
        }

        if (hasTargetingPlayer)
        {

            if(enemySettings.shouldLookAtPlayer)
            {
                var playerPos = TargetingPlayer.transform.position;

                var currentVelocity = agent.velocity.magnitude;

                if (currentVelocity <= 0.1f)
                    RotateToPlayer(playerPos);
            }
        }

        playerDistance = GetPlayerDistanceInternal();
        isSeeingPlayer = IsPlayerVisible();

        UpdateAnimations();


#if DEBUG
        /*
        if (isSeeingPlayer)
            Debug.Log("Seeing Player!");
        */
#endif

        OnUpdateAgent();

        switch (enemyState)
        {
            case EEnemyState.Idiling:
                if (isSeeingPlayer && enemySettings.canBeAngered)
                {
                    SetIdiling(false);

                    SwitchState(EEnemyState.TargetingPlayer);
                }
                else
                    if(enemySettings.canPatrol)
                {
                    SetIdiling(false);

                    SwitchState(EEnemyState.Patrolling);
                }
                else
                    OnIdle();
                break;
            case EEnemyState.Patrolling:
                if(isSeeingPlayer && enemySettings.canBeAngered)
                    SwitchState(EEnemyState.TargetingPlayer);
                else
                {
                    if (enemySettings.canPatrol)
                    {
                        if(currentPatrollable == null)
                            currentPatrollable = GetNextPatrollingPoint();
                        else
                        {
                            var patrolable = currentPatrollable.Value;

                            if (patrolable.isReachable && Vector3.Distance(transform.position, patrolable.transform.position) > 1.0f)
                            {
                                OnMoveAgent(patrolable.transform.position);
                            }
                            else
                                currentPatrollable = GetNextPatrollingPoint();
                        }
                    }
                    else
                        SwitchState(EEnemyState.Idiling);
                }

                break;
            case EEnemyState.TargetingPlayer:
                if (CanAttack())
                {
                    SwitchState(EEnemyState.AttackPlayer);
                }
                else
                {
                    if(hasTargetingPlayer && isSeeingPlayer || playerDistance <= enemySettings.minFollowDistance)
                    {
                        var playerPos = TargetingPlayer.transform.position;
                        var playerDir = (playerPos - transform.position).normalized;

                        if (playerDistance > enemySettings.maxFollowDistance)
                            OnMoveAgent(playerPos - (playerDir) * enemySettings.maxFollowDistance);
                    }
                    else
                    {
                        if (enemySettings.canPatrol)
                            SwitchState(EEnemyState.Patrolling);
                        else
                            SwitchState(EEnemyState.Idiling);
                    }
                        
                }

                break;
            case EEnemyState.AttackPlayer:

                if (hasTargetingPlayer && CanAttack())
                {
                    OnAttack();
                }
                else
                {
                    SwitchState(EEnemyState.TargetingPlayer);
                }
                break;

            default:
                break;
        }
    }

    private void LateUpdate()
    {
        triedFindingPlayer = false;
    }

    private void SetIdiling(bool idle)
    {
        if (isIdling == idle)
            return;

        isIdling = idle;

        if(idle)
            SwitchState(EEnemyState.Idiling);
    }

    private void SwitchState(EEnemyState state)
    {
        enemyState = state;

        OnSwitchedEnemyState(state);
    }

    private void OnPlayerDestroyed(Player player, bool byScene)
    {
        TargetingPlayer = null;
        hasTargetingPlayer = false;
    }

    private void RotateToPlayer(Vector3 playerPos)
    {
        var playerDir = (playerPos - transform.position).normalized;

        var nextRotation = Quaternion.LookRotation(playerDir);

        transform.rotation = Quaternion.Slerp(transform.rotation, nextRotation, enemySettings.rotateToPlayerSpeed * Time.deltaTime);
    }

    private void UpdateAnimations()
    {
        var currentVelocity = agent.velocity.magnitude;

        animator.SetFloat(ANIMATOR_characterSpeed_ID, currentVelocity);

        if (currentVelocity <= 0.1f)
        {
            animator.SetFloat(ANIMATOR_idilingTime_ID, animator.GetFloat(ANIMATOR_idilingTime_ID) + Time.deltaTime);
        }
        else
            animator.SetFloat(ANIMATOR_idilingTime_ID, 0.0f);

        OnUpdateAnimations();
    }

    private EnemyPatrolable? GetNextPatrollingPoint()
    {
        if (patrollingTargets.Count > 0 && patrollingPoint < patrollingTargets.Count)
        {
            return patrollingTargets[patrollingPoint++];
        }
        else
            patrollingPoint = 0;

        return null;
    }

    private Player FindPlayer()
    {
        return FindFirstObjectByType<Player>();
    }

    private float GetPlayerDistanceInternal()
    {
        if (TargetingPlayer)
        {
            return Vector3.Distance(transform.position, TargetingPlayer.transform.position);
        }

        return Mathf.Infinity;
    }

    private void EnemyDied()
    {
        agent.isStopped = true;

        isDead = true;

        OnEnemyDeath(EDeathSource.Player);
    }

    protected float GetPlayerDistance()
    {
        return playerDistance;
    }

    protected bool IsTargetingPlayer()
    {
        return enemyState == EEnemyState.TargetingPlayer;
    }

    protected virtual bool CanAttack()
    {
        return playerDistance <= enemySettings.minAttackDistance && enemySettings.canAttackPlayer;
    }

    protected virtual bool IsPlayerVisible()
    {
        if (TargetingPlayer == null || head == null) 
            return false;

        Vector3 directionToPlayer = (TargetingPlayer.transform.position - head.position).normalized;

        float distanceToPlayer = Vector3.Distance(head.position, TargetingPlayer.transform.position);

        if (distanceToPlayer > enemySettings.zSightDegree)
            return false;

        float angleToPlayer = Vector3.Angle(head.forward, directionToPlayer);

        if (angleToPlayer > enemySettings.xSightDegree / 2)
            return false;

        if (Physics.Raycast(head.position, directionToPlayer, out RaycastHit hitInfo, distanceToPlayer + 1.0f, playerLayerMask | obstacleLayerMask))
        {
            return hitInfo.transform == TargetingPlayer.transform;
        }

        return false;
    }

    protected virtual void OnSwitchedEnemyState(EEnemyState state)
    {

    }

    protected virtual void OnEnemyDeath(EDeathSource source)
    {
        if (enemySettings.useBasicHitboxes)
        {
            var hitboxes = GetComponentsInChildren<Hitbox>();

            for (int i = 0; i < hitboxes.Length; i++)
            {
                hitboxes[i].UnbindEnemy();
            }
        }

        animator.SetFloat(ANIMATOR_characterSpeed_ID, 0.0f);

        animator.SetFloat(ANIMATOR_idilingTime_ID, 0.0f);

    }

    protected virtual void OnEnemyDiedUpdate()
    {

    }

    protected virtual void OnEnemyHealthChanged(int valueChangedBy)
    {

    }

    protected virtual void OnMoveAgent(Vector3 position)
    {
        if(agent.pathEndPosition != position)
        {
            agent.SetDestination(position);

            if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                //Invalid Path, Position cant be reached
                SwitchState(EEnemyState.Patrolling);
            }
        }
    }

    protected virtual void OnAttack()
    {

    }

    protected virtual void OnUpdateAnimations()
    {

    }

    protected virtual void OnIdle()
    {

    }

    protected virtual void OnUpdateAgent()
    {

    }

    protected virtual void OnStartAgent()
    {

    }

    protected bool IsSeeingPlayer()
    {
        return isSeeingPlayer;
    }

    protected float DistanceToPlayer()
    {
        return playerDistance;
    }

    protected int GetHealth()
    {
        return currentHealth;
    }

    public void DamageHealth(int value)
    {
        currentHealth -= value;

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            Debug.Log("Enemy Death");

            EnemyDied();
        }

        OnEnemyHealthChanged(-value);
    }

    public void HealHealth(int value, bool healOverMax = false)
    {
        currentHealth += value;

        if (!healOverMax && currentHealth > enemySettings.enemyMaxHealth)
        {
            currentHealth = enemySettings.enemyMaxHealth;
        }

        OnEnemyHealthChanged(value);
    }

    public static int GetPlayerLayerMask()
    {
        return playerLayerMask;
    }

    public static int GetObstacleLayerMask()
    {
        return obstacleLayerMask;
    }
}
