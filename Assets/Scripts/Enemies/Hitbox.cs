using UnityEngine;

[System.Serializable]
public struct HitboxSettings
{
    [Range(0.0f, 10.0f), Tooltip("How much Damage gets through")]
    public float hitInflictorPercentage;
}

public class Hitbox : MonoBehaviour
{
    private EnemyBase owningEnemy = null;

    public Material hitMaterial = null;

    [SerializeField]
    private HitboxSettings hitboxSettings = new HitboxSettings();

    private bool hasOwningEnemy = false;

    private void OnEnemyDestroyed(EnemyBase enemy, EDeathSource deathSource)
    {
        hasOwningEnemy = false;
        enemy.UnbindOnDestroyingEnemy(OnEnemyDestroyed);
    }

    private void OnDestroy()
    {
        if(hasOwningEnemy)
        {
            hasOwningEnemy = false;
            owningEnemy.UnbindOnDestroyingEnemy(OnEnemyDestroyed);
        }
    }

    public void BindEnemyToHitbox(EnemyBase enemy)
    {
        Debug.Log("Bounded Collider");

        hasOwningEnemy = true;
        owningEnemy = enemy;

        enemy.BindOnDestroyingEnemy(OnEnemyDestroyed);
    }

    public void UnbindEnemy()
    {
        owningEnemy.UnbindOnDestroyingEnemy(OnEnemyDestroyed);

        owningEnemy = null;

        hasOwningEnemy = false;
    }

    public void DamageEnemy(int damage)
    {
        if (hasOwningEnemy)
            owningEnemy.DamageHealth(Mathf.RoundToInt(damage * hitboxSettings.hitInflictorPercentage));
    }

    public Material GetHitMaterial()
    {
        return hitMaterial;
    }
}
