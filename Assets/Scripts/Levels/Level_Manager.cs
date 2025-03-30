using UnityEngine;
using UnityEngine.SceneManagement;

public class Level_Manager : MonoBehaviour
{
    protected Player player = null;

    protected QuestSystem questSystem = null;
    protected int questsCount = 0;

    protected bool hasPlayer = false;

    private void Start()
    {
        questSystem = GetComponent<QuestSystem>();

        OnStartLevelManager();

        BindPlayer(FindFirstObjectByType<Player>(FindObjectsInactive.Include));
    }

    private void OnDestroy()
    {
        UnbindPlayer();

        OnDestroyLevelManager();
    }

    private void OnPlayerDestroyed(Player _this, bool byScene)
    {
        UnbindPlayer();

        OnPlayerDestroyedImplementation(_this, byScene);
    }

    private void OnPlayerLiveStateChanged(Player _this, bool alive)
    {
        UnbindPlayer();

        OnPlayerLiveStateChangedImplementation(_this, alive);
    }

    protected void SaveData()
    {

    }

    protected void LoadData()
    {

    }

    protected virtual void OnPlayerDestroyedImplementation(Player _this, bool byScene)
    {

    }

    protected virtual void OnPlayerLiveStateChangedImplementation(Player _this, bool alive)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    protected virtual void OnStartLevelManager()
    {

    }

    protected virtual void OnDestroyLevelManager()
    {

    }

    public void BindPlayer(Player player)
    {
        this.player = player;

        hasPlayer = (player != null);

        if(hasPlayer)
        {
            player.BindOnChangeLivingState(OnPlayerLiveStateChanged);
            player.BindOnDestroy(OnPlayerDestroyed);
        }
    }

    public void UnbindPlayer()
    {
        if(hasPlayer)
        {
            player.UnbindOnChangeLivingState(OnPlayerLiveStateChanged);
            player.UnbindOnDestroy(OnPlayerDestroyed);
        }

        hasPlayer = false;
        player = null;
    }
}