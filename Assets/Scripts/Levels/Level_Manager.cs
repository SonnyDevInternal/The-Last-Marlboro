using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(300)]
public class Level_Manager : MonoBehaviour
{
    static public Level_Manager instance = null;
    static public bool hasInstance = false;

    protected LevelLoader loader = null;

    protected SaveSystem saveSystem = null;

    protected Player player = null;

    protected QuestSystem questSystem = null;
    protected int questsCount = 0;

    protected bool hasPlayer = false;

#if DEBUG
    [SerializeField]
    protected bool DEBUG_TriggerSave = false;

    [SerializeField]
    protected bool DEBUG_TriggerLoad = false;
#endif

    [SerializeField]
    private bool canLoadSaveFiles = true;

    [SerializeField]
    private bool savePlayerOnStart = true;

    private void Start()
    {
        if(!hasInstance)
        {
            hasInstance = true;

            instance = this;
        }

        loader = GetComponent<LevelLoader>();

        saveSystem = GetComponent<SaveSystem>();

        questSystem = GetComponent<QuestSystem>();

        BindPlayer(FindFirstObjectByType<Player>(FindObjectsInactive.Include));

        if(canLoadSaveFiles)
            LoadData();

        SaveData();

        OnStartLevelManager();
    }

    private void OnDestroy()
    {
        hasInstance = false;

        instance = null;

        UnbindPlayer();

        OnDestroyLevelManager();
    }

#if DEBUG
    protected void Update()
    {
        if(DEBUG_TriggerSave)
        {
            DEBUG_TriggerSave = false;

            SaveData();
        }

        if(DEBUG_TriggerLoad)
        {
            DEBUG_TriggerLoad = false;

            LoadData();
        }
    }

#endif
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
        player.SavePlayerData(false);

        SaveSystem.saveFileData.playerSaveData = Player.saveData;
        SaveSystem.saveFileData.inventorySaveData = player.GetInventory().GetSaveData();
        SaveSystem.saveFileData.questSaveDatas = questSystem.GetQuestSaveDatas();

        saveSystem.SetSaveFileData();
    }

    protected void LoadData()
    {
        var saveData = saveSystem.GetSaveFileData();

        if (saveData.HasValue)
        {
            var save_ = saveData.Value;

            player.LoadPlayerData(save_.playerSaveData);

            player.GetInventory().LoadSaveData(save_.inventorySaveData);

            questSystem.LoadSaveDatas(save_.questSaveDatas);
        }
    }

    protected void SwitchLevel(EGameLevel level)
    {
        SaveData();

        loader.TransitionToNextLevel(level);
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
        if(player.HasBeenIntialized())
            player.Initialize();

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