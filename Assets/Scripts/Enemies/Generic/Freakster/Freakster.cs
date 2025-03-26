using System;
using UnityEngine;


[System.Serializable]
public struct FreaksterSettings
{
    public float patrollingSpeed;
    public float followingSpeed;

    public int TongueAttackDamage;
}

public class Freakster : EnemyBase
{
    [SerializeField]
    private FreaksterSettings freaksterSettings = new FreaksterSettings();

    private FreaksterTongueManager freaksterTongueManager = null;

    private FreaksterAnimationHandler freaksterHandler = null;

    [SerializeField]
    private UI_Bar3D healthBar = null;

    private bool hasFreaksterHealthbar = false;


    protected override void OnStartAgent()
    {
        freaksterHandler = GetComponentInChildren<FreaksterAnimationHandler>();

        if (freaksterHandler)
            freaksterHandler.BindOnAnimationHandlerCalled(OnFreaksterAnimEvent);

        freaksterTongueManager = GetComponentInChildren<FreaksterTongueManager>();

        hasFreaksterHealthbar = healthBar != null;

        if (hasFreaksterHealthbar)
            OnEnemyHealthChanged(0);

        if (freaksterTongueManager != null )
            freaksterTongueManager.ActivateTongueRagdoll(true);
#if DEBUG
        else
            throw new NullReferenceException("Freakster Tongue manager was null! Tongue is missing from the Freakster");
#endif
    }

    protected override void OnUpdateAgent()
    {
        if (hasFreaksterHealthbar && hasTargetingPlayer)
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
                agent.speed = freaksterSettings.patrollingSpeed;
                break;
            case EEnemyState.TargetingPlayer:
                agent.speed = freaksterSettings.followingSpeed;
                break;
            case EEnemyState.AttackPlayer:
                break;
            default:
                break;
        }
    }

    protected override void OnAttack()
    {
        if (!freaksterTongueManager)
        {
#if DEBUG
            Debug.LogError("Freakster can't Attack since the TongueManager is Missing!");
#endif
            return;
        }

        freaksterTongueManager.SetTongueDamage(freaksterSettings.TongueAttackDamage);

        freaksterTongueManager.SlashAttack(TargetingPlayer.transform.position);
    }

    protected override bool CanAttack()
    {
        return base.CanAttack() && !freaksterTongueManager.IsAttacking();
    }

    protected override void OnEnemyDeath(EDeathSource source)
    {
        base.OnEnemyDeath(source);

        if (source == EDeathSource.Scene && freaksterHandler != null)
        {
            freaksterHandler.UnbindOnAnimationHandlerCalled(OnFreaksterAnimEvent);

            hasFreaksterHealthbar = false;
        }
        else
            animator.SetTrigger("Death");
    }

    protected override void OnEnemyHealthChanged(int valueChangedBy)
    {
        if(hasFreaksterHealthbar)
            healthBar.SetPercentage((float)GetHealth() / (float)enemySettings.enemyMaxHealth);
    }

    private void OnFreaksterAnimEvent(AnimationHandler<EFreaksterAnimationEvent> _this, EFreaksterAnimationEvent Event)
    {
        switch(Event)
        {
            case EFreaksterAnimationEvent.Death:
                if(freaksterHandler != null)
                    freaksterHandler.UnbindOnAnimationHandlerCalled(OnFreaksterAnimEvent);

                Destroy(gameObject);
                break;

            default:
                break;
        }
    }
}
