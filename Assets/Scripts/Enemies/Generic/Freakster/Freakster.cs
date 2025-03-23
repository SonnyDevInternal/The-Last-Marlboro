using System;
using UnityEngine;
using UnityEngine.AI;

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

    protected override void OnStartAgent()
    {
        freaksterTongueManager = GetComponentInChildren<FreaksterTongueManager>();

        if(freaksterTongueManager != null )
            freaksterTongueManager.ActivateTongueRagdoll(true);
#if DEBUG
        else
            throw new NullReferenceException("Freakster Tongue manager was null! Tongue is missing from the Freakster");
#endif
    }

    protected override void OnUpdateAgent()
    {

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
}
