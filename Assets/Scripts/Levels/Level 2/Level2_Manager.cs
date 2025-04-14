using Unity.VisualScripting;
using UnityEngine;

public class Level2_Manager : Level_Manager
{
    [SerializeField]
    private TalkInteractable smokerTalkNpc = null;

    [SerializeField]
    private TalkInteractable smokerTalkAlt = null;

    [SerializeField]
    private ColliderHandler enterTobaccoShopCollider = null;

    [SerializeField]
    private ColliderHandler exitShopArea = null;

    [SerializeField]
    private ColliderHandler enterArenaCollider = null;

    [SerializeField]
    private Interactable gateInteractable = null;

    [SerializeField]
    private Quest mainQuest1 = null;

    [SerializeField]
    private Transform shopExitPoint = null;

    [SerializeField]
    private GameObject blockShopPath = null;

    [SerializeField]
    private StoneWhisperer stoneWhisperer = null;

    [SerializeField]
    private ColliderHandler exitLevel = null;

    [SerializeField]
    private GameObject wallBlocker = null;

    [SerializeField]
    private GameObject[] enemies = new GameObject[0];

    [SerializeField]
    private GameObject[] constructionSiteLanterns = new GameObject[0];

    protected override void OnStartLevelManager()
    {
        if(smokerTalkNpc != null)
        {
            smokerTalkNpc.BindOnTalkEvent(OnTalkEvent_SmokerNpc);
        }

        enterTobaccoShopCollider.BindOnTriggerColliderEvent(OnEnterShopColliderTriggered);


        if(!mainQuest1.HasQuestBeenFinished())
        {
            UpdateMainQuest();

            questSystem.AddQuest(mainQuest1);
        }
    }
    protected override void OnDestroyLevelManager()
    {
        if(smokerTalkNpc != null)
        {
            smokerTalkNpc.UnbindOnTalkEvent(OnTalkEvent_SmokerNpc);
        }

        enterTobaccoShopCollider.UnbindOnTriggerColliderEvent(OnEnterShopColliderTriggered);
    }

    private void UpdateMainQuest()
    {
        switch (mainQuest1.GetProgression())
        {
            case 0:
                mainQuest1.SetQuestText("Talk to the Smoking Alien");
                break;

            case 1:
                if (smokerTalkNpc != null)
                {
                    smokerTalkNpc.UnbindOnTalkEvent(OnTalkEvent_SmokerNpc);

                    Destroy(smokerTalkNpc.gameObject);

                    smokerTalkAlt.gameObject.SetActive(true);
                }

                mainQuest1.SetQuestText("Enter the Tobacco Shop");
                break;

            case 2:
                if (smokerTalkNpc != null)
                {
                    smokerTalkNpc.UnbindOnTalkEvent(OnTalkEvent_SmokerNpc);

                    Destroy(smokerTalkNpc.gameObject);
                }

                exitShopArea.gameObject.SetActive(true);

                exitShopArea.BindOnTriggerColliderEvent(OnExitShopAreaColliderTriggered);
                gateInteractable.BindOnInteracted(OnInteracted_ConstructionFenceEnter);

                enterArenaCollider.BindOnTriggerColliderEvent(OnEnterBossArenaCollider);

                gateInteractable.SetIsInteractable(true);

                for (int i = 0; i < enemies.Length; i++)
                {
                    enemies[i].SetActive(true);
                }

                player.SetPosition(shopExitPoint.position);
                player.SetRotation(shopExitPoint.rotation);

                mainQuest1.SetQuestText("Enter the Construction Site");
                break;

            case 3:
                Destroy(enterArenaCollider.gameObject);

                stoneWhisperer.gameObject.SetActive(true);

                gateInteractable.GetComponent<Animator>().SetTrigger("Close");

                stoneWhisperer.BindOnDestroyingEnemy(OnWhispererDead);

                stoneWhisperer.Activate();

                mainQuest1.SetQuestText("Fight the Stonewhisperer");
                break;

            case 4:
                mainQuest1.SetQuestText("Exit the Level");

                wallBlocker.gameObject.SetActive(false);

                exitLevel.gameObject.SetActive(true);

                exitLevel.BindOnTriggerColliderEvent(OnEnterExitLevelCollider);

                break;
        }
    }

    private void OnInteracted_ConstructionFenceEnter(Interactable _this, Player player)
    {
        _this.SetIsInteractable(false);

        _this.UnbindOnInteracted(OnInteracted_ConstructionFenceEnter);

        _this.GetComponent<Animator>().SetTrigger("Open");

        for (int i = 0;i < constructionSiteLanterns.Length;i++)
        {
            constructionSiteLanterns[i].SetActive(true);
        }
    }

    private void OnExitShopAreaColliderTriggered(ColliderHandler _this, EColliderEvent colliderEvent)
    {
        switch (colliderEvent)
        {
            case EColliderEvent.OnColliderEnter:
                if(_this.GetCollidingObject().gameObject == player.gameObject)
                {
                    _this.UnbindOnTriggerColliderEvent(OnExitShopAreaColliderTriggered);

                    blockShopPath.SetActive(true);

                    Destroy(_this.gameObject);
                }
                break;

            default:
                break;
        }
    }

    private void OnEnterShopColliderTriggered(ColliderHandler _this, EColliderEvent colliderEvent)
    {
        switch (colliderEvent)
        {
            case EColliderEvent.OnColliderEnter:
                if (mainQuest1.GetProgression() == 1)
                {
                    mainQuest1.SetProgression(2);

                    _this.UnbindOnTriggerColliderEvent(OnEnterShopColliderTriggered);

                    SwitchLevel(EGameLevel.Level2_TobaccoShop);
                }

                break;
            default:
                break;
        }
    }

    private void OnEnterBossArenaCollider(ColliderHandler _this, EColliderEvent colliderEvent)
    {
        switch (colliderEvent)
        {
            case EColliderEvent.OnColliderEnter:
                if (mainQuest1.GetProgression() == 2 && _this.GetCollidingObject().gameObject == player.gameObject)
                {
                    mainQuest1.SetProgression(3);

                    _this.UnbindOnTriggerColliderEvent(OnEnterBossArenaCollider);

                    UpdateMainQuest();
                }

                break;
            default:
                break;
        }
    }

    private void OnEnterExitLevelCollider(ColliderHandler _this, EColliderEvent colliderEvent)
    {
        switch (colliderEvent)
        {
            case EColliderEvent.OnColliderEnter:
                if (_this.GetCollidingObject().gameObject == player.gameObject)
                {
                    _this.UnbindOnTriggerColliderEvent(OnEnterBossArenaCollider);

                    SwitchLevel(EGameLevel.MainMenu);
                }

                break;
            default:
                break;
        }
    }

    private void OnAlienTalkEnded()
    {
        mainQuest1.SetProgression(1);

        UpdateMainQuest();
    }

    private void OnWhispererDead(EnemyBase enemy, EDeathSource deathSource)
    {
        mainQuest1.SetProgression(4);

        UpdateMainQuest();
    }

    private void OnTalkEvent_SmokerNpc(TalkInteractable _this, ETalkEvent Event)
    {
        switch (Event)
        {
            case ETalkEvent.End:
                OnAlienTalkEnded();
                break;

            case ETalkEvent.ScriptDestroyed:
                _this.UnbindOnTalkEvent(OnTalkEvent_SmokerNpc);
                break;

            default:
                break;
        }
    }
}
