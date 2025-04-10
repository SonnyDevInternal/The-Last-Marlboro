using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum EGameLevel
{
    MainMenu,
    Level1,
    Level2,
    Level2_TobaccoShop
}

[System.Serializable]
internal struct LevelLoaderSettings
{
    public float timeToExitLevel;

    public bool transitionExit;
}

public class LevelLoader : MonoBehaviour
{
    [SerializeField]
    private LevelLoaderSettings levelLoaderSettings = new LevelLoaderSettings();

    [SerializeField]
    private Image transitionImage = null;

    private string sceneName = "";

    private float timeToExitLevel_Current = 0.0f;

    private bool isActive = false;

    private void CallNextLevel()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void ResetLoader()
    {
        timeToExitLevel_Current = 0.0f;
        isActive = false;
    }

    // 0 - 1
    private void SetImageTransparancy(float percantage)
    {
        if(transitionImage != null)
        {
            var color = transitionImage.color;

            color.a = percantage;

            transitionImage.color = color;
        }
    }

    private void Update()
    {
        if (isActive)
        {
            if(timeToExitLevel_Current >= levelLoaderSettings.timeToExitLevel)
            {
                SetImageTransparancy(1.0f);

                ResetLoader();

                CallNextLevel();
            }
            else
            {

                timeToExitLevel_Current += Time.deltaTime;

                if (timeToExitLevel_Current < levelLoaderSettings.timeToExitLevel)
                {
                    SetImageTransparancy(timeToExitLevel_Current / levelLoaderSettings.timeToExitLevel);
                }
            }
        }
    }

    private string GetLevelName(EGameLevel level)
    {
        switch (level)
        {
            case EGameLevel.MainMenu:
                return "Scenes/MainMenu";

            case EGameLevel.Level1:
                return "Scenes/Level1";

            case EGameLevel.Level2:
                return "Scenes/Level2";

            case EGameLevel.Level2_TobaccoShop:
                return "Scenes/Level2_SubLevels/TobaccoShop";

            default:
                return "NONE";
        }
    }

    public void TransitionToNextLevel(EGameLevel level, bool loadLevelInstant = false)
    {
        sceneName = GetLevelName(level);

        if (sceneName == "NONE")
            return;

        ResetLoader();
        SetImageTransparancy(0.0f);

        if (levelLoaderSettings.transitionExit && !loadLevelInstant)
            isActive = true;
        else
            CallNextLevel();
    }
}
