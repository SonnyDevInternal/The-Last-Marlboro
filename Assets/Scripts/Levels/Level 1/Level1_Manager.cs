using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;

public class Level1_Manager : Level_Manager
{
    [SerializeField]
    private GameObject pickupIndicator = null;

    [SerializeField]
    private Item normalGun = null;

    [SerializeField]
    private Item heavyGun = null;

    [SerializeField]
    private GameObject playerToEnemyBlocker = null;

    [SerializeField]
    private GameObject playerToTunnelBlocker = null;

    [SerializeField]
    private ColliderHandler exitLevelCollider = null;

    [SerializeField]
    private Quest gunQuest = null;

    [SerializeField]
    private Quest enemyQuest = null;

    [SerializeField]
    private Quest exitQuest = null;

    private List<EnemyBase> levelEnemies = new List<EnemyBase>();

    private int enemiesKilled = 0;

    private bool hasUnlockedExit = false;

    protected override void OnStartLevelManager()
    {
        normalGun.BindOnInteracted(OnInteractEvent);
        heavyGun.BindOnInteracted(OnInteractEvent);

        if(exitLevelCollider)
            exitLevelCollider.BindOnTriggerColliderEvent(OnColliderEnter_Tunnel);

        IntializeEnemiesForQuest();

        gunQuest.SetQuestText("Pick a Character Class");
        exitQuest.SetQuestText("Go through the Tunnel");

        enemyQuest.SetQuestText(GetEnemyQuestText());

        questSystem.AddQuest(gunQuest);
    }

    private void IntializeEnemiesForQuest()
    {
        levelEnemies = new List<EnemyBase>(FindObjectsByType<EnemyBase>(FindObjectsSortMode.None));

        for (int i = 0; i < levelEnemies.Count; i++)
        {
            levelEnemies[i].BindOnDestroyingEnemy(OnEnemyDestroyed);
        }
    }

    private void UninitializeEnemies()
    {
        for (int i = 0; i < levelEnemies.Count; i++)
        {
            if (levelEnemies[i])
            {
                levelEnemies[i].UnbindOnDestroyingEnemy(OnEnemyDestroyed);
            }
        }
    }

    protected override void OnDestroyLevelManager()
    {
        UninitializeEnemies();

        UnbindInteracts();

        if(exitLevelCollider)
            exitLevelCollider.UnbindOnTriggerColliderEvent(OnColliderEnter_Tunnel);
    }

    private void UnbindInteracts()
    {
        if(normalGun)
            normalGun.UnbindOnInteracted(OnInteractEvent);

        if(heavyGun)
            heavyGun.UnbindOnInteracted(OnInteractEvent);
    }

    private void OnEnemyDestroyed(EnemyBase enemy, EDeathSource source)
    {
        enemiesKilled++;

        if (enemiesKilled >= levelEnemies.Count)
        {
            questSystem.RemoveQuest(enemyQuest.GetQuestID());

            questSystem.AddQuest(exitQuest);

            playerToTunnelBlocker.SetActive(false);

            hasUnlockedExit = true;
        }
        else
        {
            enemyQuest.SetQuestText(GetEnemyQuestText());
        }
    }

    private void OnInteractEvent(Interactable _this, Player player)
    {
        questSystem.RemoveQuest(gunQuest.GetQuestID());

        questSystem.AddQuest(enemyQuest);

        playerToEnemyBlocker.SetActive(false);

        Destroy(pickupIndicator);

        UnbindInteracts();

        if (_this == normalGun)
        {
            Destroy(heavyGun.gameObject);

            player.SetCharacterID(PlayerCharacterID.Normal);
        }
        else if(_this == heavyGun)
        {
            Destroy(normalGun.gameObject);

            player.SetCharacterID(PlayerCharacterID.Heavy);
        }

    }

    private void OnColliderEnter_Tunnel(ColliderHandler _this, EColliderEvent Event)
    {
        if (!hasUnlockedExit)
            return;

#if DEBUG
        Debug.Log("Exited Level");
#endif
    }

    private string GetEnemyQuestText()
    {
        return $"Kill all Enemies: {enemiesKilled}/{levelEnemies.Count}";
    }
}
