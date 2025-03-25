using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    static private Dictionary<int, Quest> allQuests = new Dictionary<int, Quest>();

    private string questName = "";

    private int sceneID = 0;
    private int questID = 0;

    private int progress = 0;

    Quest(string questName, int sceneID, int questID)
    {
        this.questName = questName;
        this.sceneID = sceneID;
        this.questID = questID;

        allQuests.Add(questID, this);
    }

    ~Quest()
    {
        allQuests.Remove(questID);
    }

    public string GetQuestName()
    {
        return questName;
    }

    public int GetQuestID()
    { 
        return sceneID; 
    }

    static public Quest FindQuest(int questID)
    {
        if(allQuests.TryGetValue(questID, out Quest quest))
            return quest;

        return null;
    }
}

public class QuestSystem : MonoBehaviour
{
    private GameObject mainQuestUI = null;
    private TMPro.TextMeshPro questTextUI = null;

    private Quest currentQuest = null;

    public void UpdateCurrentQuest(int questID)
    {
        currentQuest = Quest.FindQuest(questID);

        if (currentQuest != null)
        {
            mainQuestUI.SetActive(true);
            questTextUI.text = currentQuest.GetQuestName();
        }
        else
        {
            mainQuestUI.SetActive(false);
            questTextUI.text = "NULL_QUEST";
        }
    }
}
