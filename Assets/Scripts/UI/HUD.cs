using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interface du niveau : objectif, état de l'enregistrement, boutons Record / Stop / Replay,
/// messages, minuteur d'alimentation, menu pause et écran de victoire.
/// </summary>
public class HUD : MonoBehaviour
{
    MovementRecorder recorder;
    EchoBot echoBot;
    PlayerController player;
    PowerSwitch power;

    Text statusText, promptText, messageText, powerText;
    Image statusPanel, promptPanel, messagePanel;
    Button recordButton, stopButton, replayButton;
    GameObject pausePanel, winPanel;
    Text winTitle, winDetails;
    float messageTimer;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        recorder = FindObjectOfType<MovementRecorder>();
        echoBot = FindObjectOfType<EchoBot>();
        power = FindObjectOfType<PowerSwitch>();
        Build();

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.MessageRequested += ShowMessage;
            gm.LevelCompleted += OnLevelCompleted;
            gm.PauseChanged += OnPauseChanged;
            if (!string.IsNullOrEmpty(gm.introMessage)) ShowMessage(gm.introMessage, 10f);
        }
    }

    void OnDestroy()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;
        gm.MessageRequested -= ShowMessage;
        gm.LevelCompleted -= OnLevelCompleted;
        gm.PauseChanged -= OnPauseChanged;
    }

    void Build()
    {
        Transform canvas = UIFactory.CreateCanvas("HUD Canvas", 10).transform;
        GameManager gm = GameManager.Instance;
        Vector2 topLeft = new Vector2(0f, 1f), topRight = new Vector2(1f, 1f), top = new Vector2(0.5f, 1f);
        Vector2 bottom = new Vector2(0.5f, 0f), bottomLeft = new Vector2(0f, 0f), center = new Vector2(0.5f, 0.5f);

        // Objectif
        UIFactory.CreatePanel(canvas, "Objective Panel", new Color(0.02f, 0.08f, 0.11f, 0.96f),
            topLeft, new Vector2(30f, -30f), new Vector2(620f, 200f));
        UIFactory.CreateText(canvas, "Level Title", gm != null ? gm.levelTitle : "EchoBot", 28, TextAnchor.UpperLeft, UIFactory.Cyan,
            topLeft, new Vector2(52f, -42f), new Vector2(580f, 38f), FontStyle.Bold);
        Text objectiveText = UIFactory.CreateText(canvas, "Objective", gm != null ? gm.objective : "", 20, TextAnchor.UpperLeft, Color.white,
            topLeft, new Vector2(52f, -84f), new Vector2(580f, 138f));
        objectiveText.verticalOverflow = VerticalWrapMode.Truncate;

        // État de l'enregistrement
        statusPanel = UIFactory.CreatePanel(canvas, "Status Panel", UIFactory.PanelColor, topRight, new Vector2(-30f, -30f), new Vector2(460f, 90f));
        statusText = UIFactory.CreateText(statusPanel.transform, "Status", "", 26, TextAnchor.MiddleCenter, Color.white,
            center, Vector2.zero, new Vector2(440f, 80f), FontStyle.Bold);

        // Minuteur d'alimentation
        powerText = UIFactory.CreateText(canvas, "Power Timer", "", 40, TextAnchor.MiddleCenter, new Color(0.4f, 1f, 0.4f),
            top, new Vector2(0f, -170f), new Vector2(700f, 60f), FontStyle.Bold, true);

        // Boutons Record / Stop / Replay
        recordButton = UIFactory.CreateButton(canvas, "● ENREGISTRER (R)", bottom, new Vector2(-330f, 40f), new Vector2(300f, 70f),
            () => recorder.StartRecording(), new Color(0.35f, 0.06f, 0.06f, 0.95f), 24);
        stopButton = UIFactory.CreateButton(canvas, "■ STOP (F)", bottom, new Vector2(0f, 40f), new Vector2(300f, 70f),
            () => recorder.StopRecording(), null, 24);
        replayButton = UIFactory.CreateButton(canvas, "► REJOUER (G)", bottom, new Vector2(330f, 40f), new Vector2(300f, 70f),
            () => recorder.Replay(), new Color(0.05f, 0.28f, 0.2f, 0.95f), 24);

        promptPanel = UIFactory.CreatePanel(canvas, "Prompt Panel", new Color(0.02f, 0.04f, 0.06f, 0.96f),
            bottom, new Vector2(0f, 135f), new Vector2(940f, 58f), false);
        promptPanel.raycastTarget = false;
        promptText = UIFactory.CreateText(canvas, "Prompt", "", 26, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.55f),
            bottom, new Vector2(0f, 135f), new Vector2(900f, 50f), FontStyle.Bold);
        promptPanel.gameObject.SetActive(false);

        messagePanel = UIFactory.CreatePanel(canvas, "Message Panel", new Color(0.02f, 0.04f, 0.06f, 0.96f),
            center, new Vector2(0f, -75f), new Vector2(1260f, 130f), false);
        messagePanel.raycastTarget = false;
        messageText = UIFactory.CreateText(canvas, "Message", "", 30, TextAnchor.MiddleCenter, Color.white,
            center, new Vector2(0f, -75f), new Vector2(1200f, 112f), FontStyle.Bold);
        messagePanel.gameObject.SetActive(false);

        UIFactory.CreateText(canvas, "Controls", "ZQSD / WASD : bouger   •   Clic droit : caméra   •   Molette : zoom   •   E : utiliser   •   Échap : pause",
            18, TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.75f), bottomLeft, new Vector2(20f, 10f), new Vector2(1100f, 30f), FontStyle.Normal, true);

        BuildPausePanel(canvas);
        BuildWinPanel(canvas);
    }

    void BuildPausePanel(Transform canvas)
    {
        Vector2 c = new Vector2(0.5f, 0.5f);
        pausePanel = UIFactory.CreateFullscreen(canvas, "Pause", new Color(0f, 0f, 0f, 0.7f)).gameObject;
        UIFactory.CreatePanel(pausePanel.transform, "Box", UIFactory.PanelColor, c, Vector2.zero, new Vector2(560f, 460f));
        UIFactory.CreateText(pausePanel.transform, "Title", "PAUSE", 54, TextAnchor.MiddleCenter, UIFactory.Cyan, c, new Vector2(0f, 160f), new Vector2(500f, 80f), FontStyle.Bold);
        UIFactory.CreateButton(pausePanel.transform, "Reprendre", c, new Vector2(0f, 50f), new Vector2(380f, 70f), () => GameManager.Instance.TogglePause());
        UIFactory.CreateButton(pausePanel.transform, "Recommencer le niveau", c, new Vector2(0f, -40f), new Vector2(380f, 70f), () => GameManager.Instance.RestartLevel());
        UIFactory.CreateButton(pausePanel.transform, "Menu principal", c, new Vector2(0f, -130f), new Vector2(380f, 70f), () => GameManager.Instance.LoadMainMenu());
        pausePanel.SetActive(false);
    }

    void BuildWinPanel(Transform canvas)
    {
        Vector2 c = new Vector2(0.5f, 0.5f);
        bool final = GameManager.Instance != null && GameManager.Instance.IsFinalLevel;
        winPanel = UIFactory.CreateFullscreen(canvas, "Victory", new Color(0f, 0.05f, 0.06f, 0.75f)).gameObject;
        UIFactory.CreatePanel(winPanel.transform, "Box", UIFactory.PanelColor, c, Vector2.zero, new Vector2(820f, 480f));
        winTitle = UIFactory.CreateText(winPanel.transform, "Title", final ? "VOUS ÊTES LIBRE !" : "NIVEAU TERMINÉ !", 60, TextAnchor.MiddleCenter,
            new Color(0.4f, 1f, 0.6f), c, new Vector2(0f, 150f), new Vector2(780f, 90f), FontStyle.Bold);
        winDetails = UIFactory.CreateText(winPanel.transform, "Details", "", 28, TextAnchor.MiddleCenter, Color.white, c, new Vector2(0f, 40f), new Vector2(760f, 120f));
        UIFactory.CreateButton(winPanel.transform, final ? "Retour au menu" : "Niveau suivant ►", c, new Vector2(0f, -80f), new Vector2(420f, 70f),
            () => GameManager.Instance.LoadNextLevel(), new Color(0.05f, 0.3f, 0.2f, 0.95f));
        UIFactory.CreateButton(winPanel.transform, "Rejouer ce niveau", c, new Vector2(0f, -170f), new Vector2(420f, 70f), () => GameManager.Instance.RestartLevel());
        winPanel.SetActive(false);
    }

    void Update()
    {
        UpdateStatus();

        promptText.text = player != null && player.CurrentInteractable != null ? player.CurrentInteractable.Prompt : "";
        promptPanel.gameObject.SetActive(!string.IsNullOrEmpty(promptText.text));

        if (power != null && power.IsPowered)
            powerText.text = $"COURANT : {power.Remaining:0.0} s";
        else
            powerText.text = "";

        if (messageTimer > 0f)
        {
            messageTimer -= Time.unscaledDeltaTime;
            float alpha = messageTimer < 0.8f ? Mathf.Clamp01(messageTimer / 0.8f) : 1f;
            Color textColor = messageText.color;
            textColor.a = alpha;
            messageText.color = textColor;
            Color panelColor = messagePanel.color;
            panelColor.a = 0.96f * alpha;
            messagePanel.color = panelColor;
            messagePanel.gameObject.SetActive(true);
        }
        else
        {
            messagePanel.gameObject.SetActive(false);
        }
    }

    void UpdateStatus()
    {
        if (recorder == null) return;
        bool unlocked = GameManager.Instance == null || GameManager.Instance.RecordingUnlocked;
        recordButton.interactable = unlocked && !recorder.IsRecording;
        stopButton.interactable = recorder.IsRecording;
        replayButton.interactable = unlocked && (recorder.IsRecording || recorder.Frames.Count >= 2);

        if (!unlocked)
        {
            statusText.text = "ENREGISTREMENT VERROUILLÉ";
            statusText.color = new Color(1f, 0.55f, 0.2f);
        }
        else if (recorder.IsRecording)
        {
            bool blink = Mathf.Repeat(Time.time * 2f, 1f) < 0.5f;
            statusText.text = $"{(blink ? "●" : "○")} REC  {recorder.RecordingTime:0.0} / {recorder.maxDuration:0} s";
            statusText.color = new Color(1f, 0.3f, 0.3f);
        }
        else if (echoBot != null && echoBot.IsReplaying)
        {
            statusText.text = $"► ECHOBOT REJOUE  {echoBot.Progress * 100f:0}%";
            statusText.color = new Color(0.4f, 1f, 0.8f);
        }
        else if (recorder.Frames.Count >= 2)
        {
            statusText.text = $"SÉQUENCE PRÊTE  ({recorder.RecordedDuration:0.0} s)";
            statusText.color = UIFactory.Cyan;
        }
        else
        {
            statusText.text = "PRÊT À ENREGISTRER";
            statusText.color = Color.white;
        }
    }

    void ShowMessage(string message, float duration)
    {
        messageText.text = message;
        int wordCount = message.Split(new[] { ' ', '\n', '\t' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
        float readingTime = Mathf.Clamp(2f + wordCount / 3f, 5f, 12f);
        messageTimer = Mathf.Max(duration, readingTime);
        Color c = messageText.color;
        c.a = 1f;
        messageText.color = c;
        c = messagePanel.color;
        c.a = 0.96f;
        messagePanel.color = c;
        messagePanel.gameObject.SetActive(true);
    }

    void OnPauseChanged(bool paused) => pausePanel.SetActive(paused);

    void OnLevelCompleted()
    {
        GameManager gm = GameManager.Instance;
        winDetails.text = gm.IsFinalLevel
            ? $"Vous et EchoBot avez quitté l'usine robotique.\nTemps : {gm.ElapsedTime:0.0} s"
            : $"Temps : {gm.ElapsedTime:0.0} s\nEchoBot se souvient de tout…";
        winPanel.SetActive(true);
    }
}
