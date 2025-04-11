using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject mainQuestUI = null;

    [SerializeField]
    private float yQuestUIPadding = 15.0f; // in Px

    private float currentUIYPos = 0.0f;

    private List<QuestSaveData> cachedSaveDatas = new List<QuestSaveData>();

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
        var systemUITransform = mainQuestUI.GetComponent<RectTransform>();

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

            if (!quest.IsUIActive() || !quest.CanUiBeDisplayed())
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

        if (!quest.HasBeenIntialized())
            quest.IntializeQuest();

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

    public void FinishQuest(int questID)
    {
        if (activeQuests.TryGetValue(questID, out Quest quest))
        {
            quest.EndQuest();

            RemoveQuest(questID);
        }
    }

    public Quest GetQuest(int questID)
    {
        if(activeQuests.TryGetValue(questID, out Quest quest))
            return quest;

        return null;
    }

    public QuestSaveData[] GetQuestSaveDatas()
    {
        var allQuests = Quest.AllQuests();

        List< QuestSaveData > questsSave = new List< QuestSaveData >();

        for (int i = 0; i < allQuests.Length; i++)
        {
            var saveData = allQuests[i].SaveQuest();

            if (saveData.HasValue)
                questsSave.Add(saveData.Value);
        }

        for (int i = 0; i < cachedSaveDatas.Count; i++)
        {
            questsSave.Add(cachedSaveDatas[i]);
        }

        return questsSave.ToArray();
    }

    public void LoadSaveDatas(QuestSaveData[] questSaveDatas)
    {
        var sceneIDCur = SceneManager.GetActiveScene().buildIndex;

        for (int i = 0; i < questSaveDatas.Length; i++)
        {
            if (questSaveDatas[i].sceneID != sceneIDCur)
            {
                cachedSaveDatas.Add(questSaveDatas[i]);

                continue;
            }

            var quest = Quest.FindQuest(questSaveDatas[i].questID);

            if(quest != null )
            {
                quest.LoadSaveData(questSaveDatas[i]);
            }
            else
            {
                cachedSaveDatas.Add(questSaveDatas[i]);
            }
        }
    }

    public QuestSaveData? FindQuestSaveData(int sceneID, int questID)
    {
        var quests = GetQuestSaveDatas();

        for (int i = 0; i < quests.Length; i++)
        {
            if (quests[i].questID == questID && quests[i].sceneID == sceneID)
                return quests[i];
        }

        return null;
    }
}