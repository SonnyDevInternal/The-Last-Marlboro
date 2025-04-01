using UnityEngine;
using UnityEngine.SceneManagement;

enum EGameLevel
{
    MainMenu,
    Level1,
    Level2
}

public class Level_Manager : MonoBehaviour
{
    static public Level_Manager instance = null;
    static public bool hasInstance = false;

    protected Player player = null;

    protected QuestSystem questSystem = null;
    protected int questsCount = 0;

    protected bool hasPlayer = false;

    private void Start()
    {
        if(!hasInstance)
        {
            instance = this;
        }

        questSystem = GetComponent<QuestSystem>();

        BindPlayer(FindFirstObjectByType<Player>(FindObjectsInactive.Include));

        OnStartLevelManager();
    }

    private void OnDestroy()
    {
        hasInstance = false;

        instance = null;

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

    protected void SwitchLevel()
    {

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

#if DEBUG 
            Debug.Log("Bounded Player to LevelManager!");
#endif
            player.BindOnChangeLivingState(OnPlayerLiveStateChanged);
            player.BindOnDestroy(OnPlayerDestroyed);
        }
    }

    public void UnbindPlayer()
    {
        if(hasPlayer)
        {
#if DEBUG
            Debug.Log("Unbounded Player to LevelManager!");
#endif
            player.UnbindOnChangeLivingState(OnPlayerLiveStateChanged);
            player.UnbindOnDestroy(OnPlayerDestroyed);
        }

        hasPlayer = false;
        player = null;
    }

    public bool HasPlayer()
    {
        return hasPlayer;
    }

    public Player GetPlayer()
    {
        return player;
    }
}