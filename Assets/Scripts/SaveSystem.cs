using System.IO;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct SaveFileData
{
    public PlayerSaveData playerSaveData;
    public InventorySaveData inventorySaveData;
    public QuestSaveData[] questSaveDatas;

    public string GetSaveData()
    {
        return JsonUtility.ToJson(this);
    }

    public bool SetSaveData(string saveData)
    {
        try
        {
            var save = JsonUtility.FromJson<SaveFileData>(saveData);

            playerSaveData = save.playerSaveData;
            questSaveDatas = save.questSaveDatas;

            return true;
        }
        catch (System.Exception)
        {
            Debug.LogError("Failed to set Save Data (Corrupted?)");
        }
        return false;
    }
}

public class SaveSystem : MonoBehaviour
{
    //Overwrite with Data
    public static SaveFileData saveFileData = new SaveFileData();
    public static string currentFileName = "File1";

    public static bool hasTargetFile = true;

    private const string fileType = "tlm";

    [SerializeField]
    private string currentPath = "";

    public string[] GetSaveFileNames(string path)
    {
        if (!path.Contains(':'))
            path = Application.persistentDataPath + "/" + path;

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return Directory.GetFiles(path)
            .Where(file => file.EndsWith($".{fileType}"))
            .Select(Path.GetFileName)
            .ToArray();
    }


    public SaveFileData? GetSaveFileData(string fileName, string path)
    {
        if (!path.Contains(':'))
            path = Application.persistentDataPath + "/" + path;

        hasTargetFile = false;

        try
        {
            string fullPath = fileName.Contains(fileType) ? path + fileName : path + fileName + $".{fileType}";

            if (!File.Exists(fullPath)) return null;

            string data = File.ReadAllText(fullPath);
            SaveFileData saveFileOut = JsonUtility.FromJson<SaveFileData>(data);

#if DEBUG
            Debug.Log("Loaded from File: " + fullPath);
#endif

            saveFileData = saveFileOut;

            hasTargetFile = true;
            currentFileName = fileName;

            return saveFileOut;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load save: {ex.Message}");
        }
        return null;
    }


    public bool SetSaveFileData(string fileName, string path)
    {
        if (!path.Contains(':'))
            path = Application.persistentDataPath + "/" + path;

        hasTargetFile = false;

        try
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fullPath = fileName.Contains(fileType) ? path + fileName : path + fileName + $".{fileType}";

            File.WriteAllText(fullPath, saveFileData.GetSaveData());

#if DEBUG
            Debug.Log("Saved to File: " + fullPath);
#endif

            hasTargetFile = true; 
            currentFileName = fileName;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save: {ex.Message}");
        }
        return false;
    }

    public string[] GetSaveFileNames()
    {
         return GetSaveFileNames(currentPath);
    }

    public SaveFileData? GetSaveFileData(string fileName)
    {
        return GetSaveFileData(fileName, currentPath);
    }

    public bool SetSaveFileData(string fileName)
    {
        return SetSaveFileData(fileName, currentPath);
    }

    public static void SetTargetFileName(string fileName)
    {
        hasTargetFile = true;
        currentFileName = fileName;
    }

    public SaveFileData? GetSaveFileData()
    {
        if(hasTargetFile)
            return GetSaveFileData(currentFileName, currentPath);

        return null;
    }

    public bool SetSaveFileData()
    {
        if (hasTargetFile)
            return SetSaveFileData(currentFileName);

        return false;
    }

    public SaveFileData GetLoadedSaveFileData()
    {
        return saveFileData;
    }
}
