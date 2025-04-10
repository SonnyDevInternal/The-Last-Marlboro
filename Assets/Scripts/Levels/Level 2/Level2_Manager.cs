using UnityEngine;

public class Level2_Manager : Level_Manager
{
    [SerializeField]
    private TalkInteractable smokerTalkNpc = null;

    protected override void OnStartLevelManager()
    {
        if(smokerTalkNpc != null)
        {
            smokerTalkNpc.BindOnTalkEvent(OnTalkEvent_SmokerNpc);
        }
    }
    protected override void OnDestroyLevelManager()
    {
        if(smokerTalkNpc != null)
        {
            smokerTalkNpc.UnbindOnTalkEvent(OnTalkEvent_SmokerNpc);
        }
    }

    private void OnTalkEvent_SmokerNpc(TalkInteractable _this, ETalkEvent Event)
    {

    }
}
