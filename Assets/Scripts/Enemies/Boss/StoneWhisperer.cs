using System.Collections.Generic;
using UnityEngine;


internal enum EStoneWhispererState
{
    None = 0,
    StartAnimation,
    Phase0, // Drop Stones Phase
    Phase1, // Lawine Phase
    Phase2, // Bubble Phase
    DyingAnimation,
}

[System.Serializable]
public struct StoneWhispererSettings
{
    public float ProtectionBubbleTimer;
    public float breakdanceTimer;
    public float stoneThrowingStrength;
}

public class StoneWhisperer : EnemyBase
{
    [SerializeField]
    private GameObject stonePrefab = null;

    [SerializeField]
    private GameObject stoneLargePrefab = null;

    [SerializeField]
    private GameObject bossUI = null;

    [SerializeField]
    private GameObject hand = null;

    private StoneController stoneController = null;

    [SerializeField]
    private Hitbox protectionBubble = null;

    [SerializeField]
    private UI_Bar bossHpBar = null;

    private List<Vector3> randomStoneDropPositions = new List<Vector3>();

    private EStoneWhispererState currentState = EStoneWhispererState.None;

    [SerializeField]
    private StoneWhispererSettings whispererSettings = new StoneWhispererSettings();

    private float protectionBubbleTimerCurrent = .0f;
    private float breakdanceTimerCurrent = 0.0f;

    private bool hasProtectionBubble = true;

    private bool isAttacking = false;

    protected override void OnStartAgent()
    {
        gameObject.SetActive(false);

        bossUI.SetActive(false);

        GenerateRandomPositions();
    }

    protected override void OnUpdateAgent()
    {
        switch (currentState)
        {
            case EStoneWhispererState.Phase0:
                if (breakdanceTimerCurrent >= whispererSettings.breakdanceTimer)
                {
                    breakdanceTimerCurrent = 0.0f;
                    animator.SetBool("isAttacking", false);
                }
                else
                    breakdanceTimerCurrent += Time.deltaTime;
                    break;
            case EStoneWhispererState.Phase1:
                break;

            case EStoneWhispererState.Phase2:

                if(hasProtectionBubble)
                {
                    if(protectionBubbleTimerCurrent >= whispererSettings.ProtectionBubbleTimer)
                    {
                        protectionBubbleTimerCurrent = 0.0f;

                        SwitchWhisperer(EStoneWhispererState.Phase1);
                    }
                    else
                        protectionBubbleTimerCurrent += Time.deltaTime;
                }
                break;

            default:
                break;
        }
    }

    protected override void OnEnemyHealthChanged(int valueChangedBy)
    {
        var percentage = (float)GetHealth() / (float)enemySettings.enemyMaxHealth;

        if (percentage <= 0.75f)
        {
            if (percentage <= 0.45f)
                if (percentage <= 0.25f)
                    SwitchWhisperer(EStoneWhispererState.Phase2);
                else
                    SwitchWhisperer(EStoneWhispererState.Phase1);
            else
                SwitchWhisperer(EStoneWhispererState.Phase0);
        }
        else
            SwitchWhisperer(EStoneWhispererState.None);

        bossHpBar.SetPercentage(percentage);
    }

    protected override void OnEnemyDeath(EDeathSource source)
    {
        base.OnEnemyDeath(source);

        if(source != EDeathSource.Scene)
        {
            currentState = EStoneWhispererState.DyingAnimation;

            animator.SetBool("isAttacking", false);

            animator.SetTrigger("Death");
        }
    }

    protected override bool CanAttack()
    {
        return base.CanAttack() && !isAttacking;
    }

    protected override void OnAttack()
    {
        animator.SetBool("isAttacking", true);

        switch (currentState)
        {
            case EStoneWhispererState.Phase0:
                isAttacking = true;

                animator.SetInteger("AttackID", 1);

                OnExecutePhase0();
                break;
            case EStoneWhispererState.Phase1:
                OnExecutePhase1();
                break;
            case EStoneWhispererState.Phase2:
                OnExecutePhase2();
                break;

            default:
                OnAttackGeneric();
                break;
        }
    }
    private void GenerateRandomPositions()
    {
        randomStoneDropPositions.Clear();

        for (int i = 0; i < 50; i++)
        {
            float x = Random.Range(-20f, 20f);
            float z = Random.Range(-20f, 20f);

            float y = 4f;

            Vector3 randomPos = new Vector3(x, y, z);

            randomStoneDropPositions.Add(randomPos);
        }
    }

    private void OnExecutePhase0()
    {

    }

    private void OnExecutePhase1()
    {
        OnExecutePhase0();

        OnAttackGeneric();
    }

    private void OnExecutePhase2()
    {
        OnExecutePhase1();
    }

    private void SwitchWhisperer(EStoneWhispererState state)
    {
        if (currentState == state)
            return;

        switch (state)
        {
            case EStoneWhispererState.None:
                break;
            case EStoneWhispererState.StartAnimation:
                break;
            case EStoneWhispererState.Phase0:
                break;
            case EStoneWhispererState.Phase1:
                protectionBubble.gameObject.SetActive(true);
                break;
            case EStoneWhispererState.Phase2:
                protectionBubble.gameObject.SetActive(false);
                break;
            case EStoneWhispererState.DyingAnimation:
                break;
            default:
                break;
        }

        currentState = state;
    }

    private void OnAttackGeneric()
    {
        isAttacking = true;

        animator.SetInteger("AttackID", 0);
    }

    private void OnSpawnAirBalls()
    {
        var randNumb = UnityEngine.Random.Range(0, randomStoneDropPositions.Count - 1);

        if(randNumb > 0)
        {
            var randomPos = transform.position + randomStoneDropPositions[randNumb];

            var instatiated = Instantiate(stoneLargePrefab, randomPos, Quaternion.Euler(Vector3.zero));

            var stone_ = instatiated.GetComponent<StoneController>();

            stone_.Activate();

            var stoneRigid = stone_.GetComponent<Rigidbody>();

            stoneRigid.isKinematic = false;

            stoneRigid.linearVelocity = Vector3.down * whispererSettings.stoneThrowingStrength;
        }
    }

    private void OnSpawnStoneInHand()
    {
        var instatiated = Instantiate(stonePrefab, hand.transform);

        stoneController = instatiated.GetComponent<StoneController>();
    }

    private void OnReleaseStone()
    {
        if (!stoneController)
            return;

        var stoneRigid = stoneController.GetComponent<Rigidbody>();

        stoneController.Activate();

        stoneRigid.linearVelocity = (((TargetingPlayer.transform.position - stoneRigid.position).normalized * whispererSettings.stoneThrowingStrength));

        stoneController = null;
    }

    private void OnFinishAttack()
    {
        animator.SetBool("isAttacking", false);

        isAttacking = false;
    }

    private void OnDeathAnimFinished()
    {
        Destroy(gameObject);
    }

    public void OnStartAnimationFinished()
    {
        animator.SetBool("StartAnim", false);

        enemySettings.canBeAngered = true;
    }

    public void Activate()
    {
        animator.SetBool("StartAnim", true);

        bossUI.SetActive(true);
    }
}
