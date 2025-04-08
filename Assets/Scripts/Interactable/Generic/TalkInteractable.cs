using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
internal struct TextShower
{
    public string text;

    [Tooltip("How fast the Text is shown between steps")]
    public float textStepTime;

    [Tooltip("How Long the Text Pauses after the Text was shown")]
    public float textPauseTime;
}


public enum ETalkEvent
{
    Start,
    TalkStep,
    End
}

public class TalkInteractable : Interactable
{
    private static bool isTalkActive = false;

    [SerializeField]
    private Camera owningTalkCamera = null;

    private Camera playerCamera = null;

    private Player player = null;

    [SerializeField]
    private Canvas canvas = null;

    private Canvas playerCanvas = null;

    [SerializeField]
    private Image talkUIBar = null;

    [SerializeField]
    private TextMeshProUGUI talkText = null;

    [SerializeField]
    private TextShower[] textShowers = new TextShower[0];

    private int textStep = 0;

    private int textOffset = 0;

    private float textStepTimeCurrent = 0f;
    private float textStopTimeCurrent = 0f;

    private bool isBusy = false;
    private bool canTalkTo = true;

    private bool conversationStarted = false;

    public delegate void OnTalkEvent(TalkInteractable _this, ETalkEvent Event);

    private OnTalkEvent onTalkEvent = delegate { };

    public void BindOnTalkEvent(OnTalkEvent onTalkEvent) { this.onTalkEvent += onTalkEvent; }
    public void UnbindOnTalkEvent(OnTalkEvent onTalkEvent) { this.onTalkEvent -= onTalkEvent; }

    private void Start()
    {
        ActivateTalkUI(false);
    }

    private void Update()
    {
        if(conversationStarted)
        {
            if(textStep >= textShowers.Length)
            {
                EndConversation();
            }
            else
            {
                var cachedTextShower = textShowers[textStep];

                if (textStepTimeCurrent >= cachedTextShower.textStepTime)
                {
                    var nextText = GetNextText(cachedTextShower);

                    if (nextText != null)
                    {
                        talkText.text = talkText.text + nextText;

                        textStepTimeCurrent = 0.0f;

                        OnTextStep();
                    }
                    else
                    {
                        if (textStopTimeCurrent >= cachedTextShower.textPauseTime)
                        {
                            textStopTimeCurrent = 0f;
                            textStepTimeCurrent = 0.0f;

                            textOffset = 0;

                            talkText.text = "";

                            textStep++;
                        }
                        else
                            textStopTimeCurrent += Time.deltaTime;
                    }
                }
                else
                    textStepTimeCurrent += Time.deltaTime;
            }
        }
    }

    private string GetNextText(TextShower textShower)
    {
        if (textOffset >= textShower.text.Length)
            return null;

        int nextOffset = textShower.text.IndexOf(' ', textOffset);

        string nextText;

        if (nextOffset == -1)
        {
            nextText = textShower.text.Substring(textOffset);
            textOffset = textShower.text.Length;
        }
        else
        {
            nextText = textShower.text.Substring(textOffset, nextOffset - textOffset + 1);
            textOffset = nextOffset + 1;
        }


        return nextText;
    }


    private void ActivateTalkUI(bool value)
    {
        talkUIBar.enabled = value;
        talkText.enabled = value;
        canvas.enabled = value;

        if (player)
            player.SetUICanvas(!value);
    }

    private void StartConversation()
    {
        if (this.playerCamera)
            this.playerCamera.gameObject.SetActive(false);

        this.owningTalkCamera.gameObject.SetActive(true);

        textStep = 0;

        isBusy = true;
        conversationStarted = true;

        ActivateTalkUI(true);

        EnemyBase.SetEnemiesIgnorePlayer(true);

        onTalkEvent(this, ETalkEvent.Start);
    }

    private void EndConversation()
    {
        if(this.playerCamera)
            this.playerCamera.gameObject.SetActive(true);

        this.owningTalkCamera.gameObject.SetActive(false);

        isBusy = false;
        conversationStarted = false;

        ActivateTalkUI(false);

        EnemyBase.SetEnemiesIgnorePlayer(false);

        onTalkEvent(this, ETalkEvent.End);
    }

    private void OnTextStep()
    {
        onTalkEvent(this, ETalkEvent.TalkStep);
    }

    public override bool CanInteract(Vector3 playerPos)
    {
        return base.CanInteract(playerPos) && !isBusy && canTalkTo;
    }

    protected override void OnInteract(Player player)
    {
        this.player = player;

        this.playerCamera = player.GetComponent<LocalPlayer>().GetPlayerCamera();

        StartConversation();
    }

    public void SetCanTalkTo(bool canTalkTo)
    {
        this.canTalkTo = canTalkTo;
    }

    public void SkipTextShower()
    {
        if (textStep >= textShowers.Length)
            return;

        textOffset = 0;
        talkText.text = "";
        textStepTimeCurrent = 0f;
        textStopTimeCurrent = 0f;

        textStep++;
    }
}