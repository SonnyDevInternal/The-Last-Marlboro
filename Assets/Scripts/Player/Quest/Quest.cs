using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using System.Linq;

public enum EQuestEvent
{
    OnStartQuest,
    OnProgressQuest,
    OnEndQuest,

    OnQuestDestroyed
}

[System.Serializable]
public struct QuestSettings
{
    public bool canBeSaved;
    public bool canBeDisplayed;
}

[System.Serializable]
public struct QuestSaveData
{
    public int sceneID;

    public int questID;

    public int progress;

    public QuestSettings settings;
}


public class Quest : MonoBehaviour
{
    static private Dictionary<int, Quest> allQuests = new Dictionary<int, Quest>();

    [SerializeField]
    private TextMeshProUGUI questTextUI = null;

    private RectTransform currentUITransform = null;

    [SerializeField]
    private CanvasRenderer canvasRenderer = null;

    [SerializeField]
    private string questName = "";

    private int sceneID = 0;

    [SerializeField]
    private int questID = 0;

    private int progress = 0;

    private bool isActive = false;

    private bool isUiActive = false;

    private bool hasBeenIntialized = false;

    [SerializeField]
    protected QuestSettings questSettings = new QuestSettings();

    public delegate void OnQuestEvent(Quest _this, EQuestEvent Event);

    protected OnQuestEvent onQuestEvent = delegate { };

    public void BindOnQuestEvent(OnQuestEvent onQuestEvent) { this.onQuestEvent += onQuestEvent; }
    public void UnbindOnQuestEvent(OnQuestEvent onQuestEvent) { this.onQuestEvent -= onQuestEvent; }

    private void Start()
    {
        IntializeQuest();
    }

    private void SendQuestEvent(EQuestEvent Event)
    {
        onQuestEvent.Invoke(this, Event);
    }

    private void OnDestroy()
    {
        SendQuestEvent(EQuestEvent.OnQuestDestroyed);

        allQuests.Remove(questID);
    }

    public string GetQuestName()
    {
        return questName;
    }

    public void SetQuestActive()
    {
        isActive = true;

        progress = 0;

        SendQuestEvent(EQuestEvent.OnStartQuest);
    }

    public void IntializeQuest()
    {
        if (hasBeenIntialized)
            return;

        hasBeenIntialized = true;

        currentUITransform = GetComponent<RectTransform>();

        sceneID = SceneManager.GetActiveScene().buildIndex;

        canvasRenderer.cull = true;

        allQuests.Add(questID, this);
    }

    public void EndQuest()
    {
        SendQuestEvent(EQuestEvent.OnEndQuest);
    }

    public void SetQuestText(string text)
    {
        questTextUI.text = text;
    }

    // 0 - 100
    public void SetProgression(int progress)
    {
        this.progress = progress;

        SendQuestEvent(EQuestEvent.OnProgressQuest);
    }

    public void SetUIActive(bool active)
    {
        if (active && !questSettings.canBeDisplayed)
            return;

        this.isUiActive = active;

        canvasRenderer.cull = !active;
    }

    public void LoadSaveData(QuestSaveData data)
    {
        if (data.sceneID != sceneID || data.questID != questID)
            return;

        progress = data.progress;
        questSettings = data.settings;
    }

    public bool HasBeenIntialized()
    {
        return hasBeenIntialized;
    }

    public QuestSaveData? SaveQuest()
    {
        if (questSettings.canBeSaved)
        {
            var saveData = new QuestSaveData();

            saveData.progress = progress;
            saveData.settings = questSettings;
            saveData.sceneID = sceneID;
            saveData.questID = questID;

            return saveData;
        }

        return null;
    }

    public int GetProgression()
    {
        return progress;
    }

    public int GetQuestID()
    {
        return sceneID;
    }

    public bool IsUIActive()
    {
        return isUiActive;
    }

    public bool CanUiBeDisplayed()
    {
        return questSettings.canBeDisplayed;
    }

    public RectTransform GetUITransform()
    {
        return currentUITransform;
    }

    static public Quest FindQuest(int questID)
    {
        if (allQuests.TryGetValue(questID, out Quest quest))
            return quest;

        return null;
    }

    static public Quest[] AllQuests()
    {
        return allQuests.Values.ToArray();
    }
}