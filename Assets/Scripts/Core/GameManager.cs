using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gère l'état du niveau : déblocage de l'enregistrement, pause, victoire et chargement des scènes.
/// Un seul GameManager par scène de niveau.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Niveau")]
    public string levelTitle = "Niveau 1 — La salle de sécurité";
    [TextArea] public string objective = "Ouvrez la porte de sécurité.";
    [TextArea] public string introMessage = "";
    [Tooltip("Scène chargée après la victoire. Vide = dernier niveau.")]
    public string nextScene = "Level2";
    public string mainMenuScene = "MainMenu";
    public bool recordingUnlockedAtStart = false;

    [Tooltip("Sous cette hauteur, le joueur est replacé à son point de départ.")]
    public float fallLimitY = -10f;

    public bool RecordingUnlocked { get; private set; }
    public bool IsLevelComplete { get; private set; }
    public bool IsPaused { get; private set; }
    public float ElapsedTime { get; private set; }
    public bool IsFinalLevel => string.IsNullOrEmpty(nextScene);
    public bool CanPlay => !IsPaused && !IsLevelComplete;

    public event Action<string, float> MessageRequested;
    public event Action LevelCompleted;
    public event Action<bool> PauseChanged;

    void Awake()
    {
        Instance = this;
        RecordingUnlocked = recordingUnlockedAtStart;
        Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (CanPlay) ElapsedTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
    }

    public void UnlockRecording()
    {
        if (RecordingUnlocked) return;
        RecordingUnlocked = true;
        ShowMessage("Console activée !  R = Enregistrer   F = Stop   G = EchoBot rejoue", 5f);
    }

    public void ShowMessage(string message, float duration = 3f)
    {
        MessageRequested?.Invoke(message, duration);
    }

    public void CompleteLevel()
    {
        if (IsLevelComplete) return;
        IsLevelComplete = true;
        LevelCompleted?.Invoke();
    }

    public void TogglePause()
    {
        if (IsLevelComplete) return;
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        PauseChanged?.Invoke(IsPaused);
    }

    public void LoadNextLevel() => LoadScene(IsFinalLevel ? mainMenuScene : nextScene);
    public void RestartLevel() => LoadScene(SceneManager.GetActiveScene().name);
    public void LoadMainMenu() => LoadScene(mainMenuScene);

    static void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
