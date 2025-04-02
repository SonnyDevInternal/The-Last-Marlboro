using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        SceneManager.LoadScene(sceneName);
#if DEBUG
        Debug.LogError("Failed to Load Level, Index was invalid");
#endif
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

    public void TransitionToNextLevel(string levelName)
    {
        sceneName = levelName;

        ResetLoader();
        SetImageTransparancy(0.0f);

        if (levelLoaderSettings.transitionExit)
            isActive = true;
        else
            CallNextLevel();
    }
}
