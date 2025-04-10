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
    private Quest mainQuest1 = null;

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

           switch( mainQuest1.GetProgression())
            {
                case 1:
                    if (smokerTalkNpc != null)
                        smokerTalkNpc.UnbindOnTalkEvent(OnTalkEvent_SmokerNpc);

                    Destroy(smokerTalkNpc.gameObject);

                    smokerTalkAlt.gameObject.SetActive(true);
                    break;
            }

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
                mainQuest1.SetQuestText("Enter the Tobacco Shop");
                break;

            case 2:
                mainQuest1.SetQuestText("Go to the Construction Site");
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
                    _this.UnbindOnTriggerColliderEvent(OnEnterShopColliderTriggered);

                    loader.TransitionToNextLevel(EGameLevel.Level2_TobaccoShop);
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

        if (smokerTalkNpc != null)
            smokerTalkNpc.UnbindOnTalkEvent(OnTalkEvent_SmokerNpc);

        Destroy(smokerTalkNpc.gameObject);

        smokerTalkAlt.gameObject.SetActive(true);
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
