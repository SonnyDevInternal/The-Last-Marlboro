using UnityEngine;

public struct SaveFileData
{
    public PlayerSaveData playerSaveData;
    public QuestSaveData[] questSaveDatas;
}

public class SaveSystem : MonoBehaviour
{
    static string currentPath = "";
    static string currentFileName = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
