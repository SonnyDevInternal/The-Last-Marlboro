using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject mainQuestUI = null;

    [SerializeField]
    private float yQuestUIPadding = 15.0f; // in Px

    private float currentUIYPos = 0.0f;

    private Dictionary<int, Quest> activeQuests = new Dictionary<int, Quest>();

    public delegate void OnQuestsEvent(int questID, EQuestEvent Event);

    private OnQuestsEvent onQuestsEvent = delegate { };

    public void BindOnQuestsEvent(OnQuestsEvent onQuestsEvent) { this.onQuestsEvent += onQuestsEvent; }
    public void UnbindOnQuestsEvent(OnQuestsEvent onQuestsEvent) { this.onQuestsEvent -= onQuestsEvent; }

    private void TransmitQuestEvent(int questID, EQuestEvent questEvent)
    {
        this.onQuestsEvent.Invoke(questID, questEvent);
    }

    private void OnQuestEvent(Quest _this, EQuestEvent Event)
    {
        if(Event == EQuestEvent.OnQuestDestroyed)
        {
            activeQuests.Remove(_this.GetQuestID());

            _this.UnbindOnQuestEvent(OnQuestEvent);
        }

        TransmitQuestEvent(_this.GetQuestID(), Event);
    }

    private void AddQuestToUI(Quest quest)
    {
        var systemUITransform = mainQuestUI.transform;

        quest.SetUIActive(true);

        var uiTransform = quest.GetUITransform();

        uiTransform.SetParent(systemUITransform, false);

        uiTransform.localPosition = new Vector3(0, currentUIYPos);

        currentUIYPos += (uiTransform.rect.height + yQuestUIPadding);
    }

    private void RecalculateQuestUIs()
    {
        currentUIYPos = 0.0f;

        foreach (var pair in activeQuests)
        {
            var quest = pair.Value;

            if (!quest.IsUIActive())
                continue;

            var uiTransform = quest.GetUITransform();

            uiTransform.localPosition = new Vector3(0, currentUIYPos);

            currentUIYPos += (uiTransform.rect.height + yQuestUIPadding);
        }
    }

    private void RemoveQuestToUI(Quest quest)
    {
        quest.transform.parent = null;

        quest.SetUIActive(false);

        RecalculateQuestUIs();
    }

    public void AddQuest(Quest quest)
    {
        if (!quest)
        {
#if DEBUG
            Debug.LogError("Couldn't Add Quest!: Quest is Nullptr!");
#endif
            return;
        }

        quest.BindOnQuestEvent(OnQuestEvent);

        activeQuests[quest.GetQuestID()] = quest;

        AddQuestToUI(quest);
    }

    public void RemoveQuest(int questID)
    {
        if(activeQuests.TryGetValue(questID, out Quest quest))
        {
            activeQuests.Remove(questID);

            if(quest)
            {
                quest.UnbindOnQuestEvent(OnQuestEvent);

                RemoveQuestToUI(quest);
            }
        }
    }

    public Quest GetQuest(int questID)
    {
        if(activeQuests.TryGetValue(questID, out Quest quest))
            return quest;

        return null;
    }
}
