using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject savePrefab = null;

    [SerializeField]
    private Button startGameBtn = null;

    [SerializeField]
    private Button optionsBtn = null;

    [SerializeField]
    private Button CreditsBtn = null;

    [SerializeField]
    private Button quitBtn = null;

    private void HideMainButtons(bool value)
    {
        bool value_ = !value;

        startGameBtn.gameObject.SetActive(value_);
        optionsBtn.gameObject.SetActive(value_);
        CreditsBtn.gameObject.SetActive(value_);
        quitBtn.gameObject.SetActive(value_);
    }

    void Start()
    {
        startGameBtn.onClick.AddListener(OnPressedStartGame);
    }

    private void OnDestroy()
    {
        startGameBtn.onClick.RemoveAllListeners();
    }

    void OnPressedStartGame()
    {
        HideMainButtons(true);
    }
}
