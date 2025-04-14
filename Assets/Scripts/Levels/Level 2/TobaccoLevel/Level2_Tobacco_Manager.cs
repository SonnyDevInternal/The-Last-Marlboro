using UnityEngine;

public class Level2_Tobacco_Manager : Level_Manager 
{
    [SerializeField]
    private Interactable exitToBackside = null;

    [SerializeField]
    private Interactable exitToShop = null;

    [SerializeField]
    private Interactable exitToCity = null;

    [SerializeField]
    private TalkInteractable manualJacksonNpc = null;

    [SerializeField]
    private TalkInteractable manualJacksonNext = null;

    [SerializeField]
    private TalkInteractable manualJacksonGeneric = null;

    [SerializeField]
    private TalkInteractable workerNpc = null;

    [SerializeField]
    private Quest mainQuest_ = null;

    [SerializeField]
    private Transform outsidePoint = null;

    [SerializeField]
    private Transform shopPoint = null;

    [SerializeField]
    private EnemyBase[] friendlyWorkers = new EnemyBase[0];

    private int enemiesDead = 0;

    private bool hasTalkedToManual = false;
    private bool hasMadeEnemiesMad = false;

    protected override void OnStartLevelManager()
    {
        manualJacksonNpc.BindOnTalkEvent(OnTalkEvent_ManualJackson);
        manualJacksonNext.BindOnTalkEvent(OnTalkEvent_ManualJackson);
        workerNpc.BindOnTalkEvent(OnTalkEvent_WorkerNpc);

        MakeEnemiesAngry(false);

        for (int i = 0; i < friendlyWorkers.Length; i++)
        {
            friendlyWorkers[i].BindOnDestroyingEnemy(OnEnemyDestroyed);
            friendlyWorkers[i].BindOnEnemyEvent(OnWorkerEnemyEvent);
        }

        friendlyWorkers[1].SetIdlingMode(1);

        exitToBackside.BindOnInteracted(OnInteracted_exitToBackside);
        exitToCity.BindOnInteracted(OnInteracted_exitToCity);
        exitToShop.BindOnInteracted(OnInteracted_exitToShop);

        questSystem.AddQuest(mainQuest_);

        UpdateQuestState();
    }

    protected override void OnDestroyLevelManager()
    {
        UnbindManualJackson();
        UnbindWorkerNpc();

        if(exitToBackside != null)
            exitToBackside.UnbindOnInteracted(OnInteracted_exitToBackside);

        if (exitToCity != null)
            exitToCity.UnbindOnInteracted(OnInteracted_exitToCity);
    }

    private void UpdateQuestState()
    {
        switch (mainQuest_.GetProgression())
        {
            case 0:
                mainQuest_.SetQuestText("Talk to Manual Jackson");
                break;

            case 1:
                hasTalkedToManual = true;

                manualJacksonNpc.gameObject.SetActive(false);
                manualJacksonGeneric.gameObject.SetActive(true);

                exitToBackside.SetIsInteractable(true);

                mainQuest_.SetQuestText("Go to the Backside and fight the Goons");
                break;

            case 2:
                manualJacksonGeneric.gameObject.SetActive(false);
                manualJacksonNext.gameObject.SetActive(true);

                mainQuest_.SetQuestText("Talk to Manual Jackson again");
                break;

            case 3:
                var quest_ = questSystem.FindQuestSaveData(2, 0);

                if (quest_.HasValue)
                {
                   var questSaveData = quest_.Value;

                    questSaveData.progress = 2;

                    questSystem.FindQuestAndSetSaveData(2, 0, questSaveData);
                }

                exitToShop.SetIsInteractable(true);
                exitToCity.SetIsInteractable(true);

                mainQuest_.SetQuestText("Go back to the City");
                break;

            default:
                break;
        }
    }

    private void MakeEnemiesAngry(bool value)
    {
        exitToShop.SetIsInteractable(false);

        for (int i = 0; i < friendlyWorkers.Length; i++)
        {
            var worker = friendlyWorkers[i];

            if(worker)
            {
                var settings = worker.GetEnemySettings();

                settings.canBeAngered = value;

                worker.SetEnemySettings(settings);
            }
        }
    }

    private void OnEnemyDestroyed(EnemyBase _this, EDeathSource _source)
    {
        enemiesDead++;

        _this.UnbindOnDestroyingEnemy(OnEnemyDestroyed);
        _this.UnbindOnEnemyEvent(OnWorkerEnemyEvent);

        if (enemiesDead == friendlyWorkers.Length)
        {
            exitToShop.SetIsInteractable(true);

            mainQuest_.SetProgression(2);

            UpdateQuestState();
        }
    }

    private void OnWorkerEnemyEvent(EnemyBase _this, EEnemyEvent Event)
    {
        switch (Event)
        {
            case EEnemyEvent.OnDamaged:
                if(!hasMadeEnemiesMad)
                {
                    hasMadeEnemiesMad = true;

                    friendlyWorkers[1].SetIdlingMode(0);

                    Destroy(workerNpc.gameObject);

                    MakeEnemiesAngry(true);
                }
                break;
            case EEnemyEvent.OnHealed:
                break;

            default:
                break;
        }
    }

    private void OnTalkEnd_ManualJackson()
    {
        mainQuest_.SetProgression(1);

        UpdateQuestState();

        UnbindManualJackson();
    }

    private void UnbindManualJackson()
    {
        if (manualJacksonNpc != null)
            manualJacksonNpc.UnbindOnTalkEvent(OnTalkEvent_ManualJackson);
    }

    private void OnTalkEnd_WorkerNpc()
    {
        mainQuest_.SetProgression(3);

        UpdateQuestState();

        manualJacksonGeneric.gameObject.SetActive(false);

        workerNpc.gameObject.SetActive(false);

        UnbindWorkerNpc();
    }

    private void UnbindWorkerNpc()
    {
        if (workerNpc != null)
            workerNpc.UnbindOnTalkEvent(OnTalkEvent_WorkerNpc);
    }



    private void OnTalkEvent_ManualJackson(TalkInteractable _this, ETalkEvent Event)
    {
        switch (Event)
        {
            case ETalkEvent.Start:
                break;
            case ETalkEvent.TalkStep:
                break;

            case ETalkEvent.End:
                switch (mainQuest_.GetProgression())
                {
                    case 0:
                        OnTalkEnd_ManualJackson();
                        break;

                    case 2:
                        manualJacksonNext.gameObject.SetActive(false);

                        mainQuest_.SetProgression(3);

                        UpdateQuestState();
                        break;

                    default:
                        break;
                }
                break;

            case ETalkEvent.ScriptDestroyed:
                UnbindManualJackson();
                break;

            default:
                break;
        }
    }

    private void OnTalkEvent_WorkerNpc(TalkInteractable _this, ETalkEvent Event)
    {
        switch (Event)
        {
            case ETalkEvent.Start:
                break;
            case ETalkEvent.TalkStep:
                break;

            case ETalkEvent.End:
                OnTalkEnd_WorkerNpc();
                break;

            case ETalkEvent.ScriptDestroyed:
                UnbindWorkerNpc();
                break;

            default:
                break;
        }
    }

    private void OnInteracted_exitToBackside(Interactable _this, Player player)
    {
        if (!hasTalkedToManual)
            return;

        player.SetPosition(outsidePoint.position);
        player.SetRotation(outsidePoint.rotation);
    }

    private void OnInteracted_exitToShop(Interactable _this, Player player)
    {
        player.SetPosition(shopPoint.position);
        player.SetRotation(shopPoint.rotation);
    }

    private void OnInteracted_exitToCity(Interactable _this, Player player)
    {
        loader.TransitionToNextLevel(EGameLevel.Level2);
    }
}
