using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum EMainMenuState
{
    none,
    Credits,
    Options,
    StartGame,
    LoadGame
}

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject savePrefab = null;

    [SerializeField]
    private Button goBackButton = null;

    [SerializeField]
    private Button startGameBtn = null;

    [SerializeField]
    private Button optionsBtn = null;

    [SerializeField]
    private Button creditsBtn = null;

    [SerializeField]
    private Button quitBtn = null;

    [SerializeField]
    private GameObject otherUI = null;

    [SerializeField]
    private GameObject fileCanvas = null;

    [SerializeField]
    private GameObject contentHolder = null;

    [SerializeField]
    private Button loadGameBtn = null;

    [SerializeField]
    private Button newGameBtn = null;

    private SaveSystem saveSystem = null;

    private string[] gatheredSaveFiles = null;
    private List<SaveInstance> saveInstances = new List<SaveInstance>();

    private EMainMenuState currentMenuState = EMainMenuState.none;

    private bool hasCleanedUp = false;

    private void Cleanup()
    {
        if (hasCleanedUp)
            return;

        hasCleanedUp = true;

        saveInstances = null;
        gatheredSaveFiles = null;

        startGameBtn.onClick.RemoveAllListeners();
        optionsBtn.onClick.RemoveAllListeners();
        creditsBtn.onClick.RemoveAllListeners();
        goBackButton.onClick.RemoveAllListeners();
        goBackButton.onClick.RemoveAllListeners();
        newGameBtn.onClick.RemoveAllListeners();
    }

    private void GatherSaveFiles()
    {
        gatheredSaveFiles = saveSystem.GetSaveFileNames();

        for (int i = 0; i < gatheredSaveFiles.Length; i++)
        {
            var obj = Instantiate(savePrefab, contentHolder.transform, false);

            var saveInstance = obj.GetComponent<SaveInstance>();

            saveInstance.SetSaveIndex(i);

            saveInstance.BindOnLoadSave(OnPressedSaveInstanceButton);

            obj.SetActive(false);

            saveInstances.Add(saveInstance);
        }
    }

    private void HideSaveFileInstances(bool value)
    {
        bool value_ = !value;

        for (int i = 0; i < saveInstances.Count; i++)
        {
            saveInstances[i].gameObject.SetActive(value_);
        }
    }

    private void HideLoadButtons(bool value)
    {
        bool value_ = !value;

        fileCanvas.SetActive(value_);
        otherUI.SetActive(value_);
        contentHolder.SetActive(value_);
    }

    private void HideMainButtons(bool value)
    {
        bool value_ = !value;

        startGameBtn.gameObject.SetActive(value_);
        optionsBtn.gameObject.SetActive(value_);
        creditsBtn.gameObject.SetActive(value_);
        quitBtn.gameObject.SetActive(value_);
    }

    private void HideStartButtons(bool value)
    {
        bool value_ = !value;

        otherUI.gameObject.SetActive(value_);
        loadGameBtn.gameObject.SetActive(value_);
        newGameBtn.gameObject.SetActive(value_);
    }

    private void HideGoBackButton(bool value)
    {
        bool value_ = !value;

        goBackButton.gameObject.SetActive(value_); 
    }

    private void UpdateMenuState(EMainMenuState nextState)
    {
        switch (nextState)
        {
            case EMainMenuState.none:
                HideStartButtons(true);
                HideMainButtons(false);

                HideGoBackButton(true);
                break;

            case EMainMenuState.Credits:
                HideMainButtons(true);

                HideGoBackButton(false);
                break;
            case EMainMenuState.Options:
                HideMainButtons(true);

                HideGoBackButton(false);
                break;
            case EMainMenuState.StartGame:
                if(currentMenuState == EMainMenuState.LoadGame)
                {
                    HideLoadButtons(true);
                    HideSaveFileInstances(true);
                }

                HideMainButtons(true);
                HideStartButtons(false);

                HideGoBackButton(false);
                break;
            case EMainMenuState.LoadGame:
                HideStartButtons(true);

                HideLoadButtons(false);
                HideSaveFileInstances(false);
                break;

            default:
                break;
        }

        currentMenuState = nextState;
    }

    private void OnPressedSaveInstanceButton(int index)
    {
        SaveSystem.SetTargetFileName(gatheredSaveFiles[index]);

        var saveData = saveSystem.GetSaveFileData();

        if (saveData.HasValue)
            SceneManager.LoadScene(saveData.Value.playerSaveData.sceneID, LoadSceneMode.Single);
        else
        {
            saveInstances.Remove(saveInstances[index]);

            OnPressedGoBack();

            UpdateMenuState(EMainMenuState.none);
        }
    }

    void OnPressedGoBack()
    {
        switch (currentMenuState)
        {
            case EMainMenuState.Credits:
                UpdateMenuState(EMainMenuState.none);
                break;
            case EMainMenuState.Options:
                UpdateMenuState(EMainMenuState.none);
                break;
            case EMainMenuState.StartGame:
                UpdateMenuState(EMainMenuState.none);
                break;
            case EMainMenuState.LoadGame:
                UpdateMenuState(EMainMenuState.StartGame);
                break;

            default:
                break;
        }
    }

    void OnPressedStartGame()
    {
        UpdateMenuState(EMainMenuState.StartGame);
    }

    void OnPressedOptions()
    {
        UpdateMenuState(EMainMenuState.Options);
    }

    void OnPressedCredits()
    {
        UpdateMenuState(EMainMenuState.Credits);
    }

    void OnPressedQuit()
    {
        Cleanup();

        Application.Quit();
    }

    void OnPressedNewGame()
    {
        SaveSystem.SetTargetFileName(Environment.TickCount.ToString());

        SceneManager.LoadScene("Level1", LoadSceneMode.Single);
    }

    void OnPressedLoadGame()
    {
        UpdateMenuState(EMainMenuState.LoadGame);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        saveSystem = GetComponent<SaveSystem>();

        goBackButton.onClick.AddListener(OnPressedGoBack);

        startGameBtn.onClick.AddListener(OnPressedStartGame);
        optionsBtn.onClick.AddListener(OnPressedOptions);
        creditsBtn.onClick.AddListener(OnPressedCredits);

        newGameBtn.onClick.AddListener(OnPressedNewGame);
        loadGameBtn.onClick.AddListener(OnPressedLoadGame);

        GatherSaveFiles();
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}
