using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public struct FreaksterSettings
{
    public float patrollingSpeed;
    public float followingSpeed;
}

public class Freakster : EnemyBase
{
    [SerializeField]
    private FreaksterSettings freaksterSettings = new FreaksterSettings();

    private FreaksterTongueManager freaksterTongueManager = null;

    protected override void OnStartAgent()
    {
        freaksterTongueManager = GetComponentInChildren<FreaksterTongueManager>();

        freaksterTongueManager.ActivateTongueRagdoll(true);
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
        freaksterTongueManager.SlashAttack(TargetingPlayer.transform.position);
    }

    protected override bool CanAttack()
    {
        return base.CanAttack() && !freaksterTongueManager.IsAttacking();
    }
}
