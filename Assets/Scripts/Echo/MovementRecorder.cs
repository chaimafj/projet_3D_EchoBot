using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Une image de l'enregistrement : position/rotation du joueur à un instant donné.</summary>
[Serializable]
public struct RecordFrame
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
    public bool interact;   // le joueur a appuyé sur E à ce moment
}

/// <summary>
/// Enregistre les positions du joueur à intervalle régulier.
/// R = démarrer, F = arrêter, G = EchoBot rejoue la séquence.
/// </summary>
public class MovementRecorder : MonoBehaviour
{
    public EchoBot echoBot;
    public float sampleInterval = 0.05f;
    public float maxDuration = 20f;
    public LineRenderer pathLine;

    public KeyCode recordKey = KeyCode.R;
    public KeyCode stopKey = KeyCode.F;
    public KeyCode replayKey = KeyCode.G;

    readonly List<RecordFrame> frames = new List<RecordFrame>();
    public IReadOnlyList<RecordFrame> Frames => frames;
    public bool IsRecording { get; private set; }
    public float RecordingTime { get; private set; }
    public float RecordedDuration => frames.Count > 0 ? frames[frames.Count - 1].time : 0f;
    public bool HasRecording => !IsRecording && frames.Count >= 2;

    float sampleTimer;
    bool pendingInteract;

    void Start()
    {
        if (echoBot == null) echoBot = FindObjectOfType<EchoBot>();
        UpdatePathLine();
    }

    void Update()
    {
        bool canAct = GameManager.Instance == null || GameManager.Instance.CanPlay;
        if (canAct)
        {
            if (Input.GetKeyDown(recordKey)) StartRecording();
            if (Input.GetKeyDown(stopKey)) StopRecording();
            if (Input.GetKeyDown(replayKey)) Replay();
        }

        if (!IsRecording) return;

        RecordingTime += Time.deltaTime;
        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleInterval || pendingInteract)
        {
            sampleTimer = 0f;
            AddFrame();
        }
        if (RecordingTime >= maxDuration)
        {
            StopRecording();
            Message("Durée maximale atteinte : enregistrement arrêté.");
        }
    }

    public void StartRecording()
    {
        if (GameManager.Instance != null && !GameManager.Instance.RecordingUnlocked)
        {
            Message("R est verrouillé : allez d'abord à la console (écran orange, en bas à gauche) et appuyez sur E.", 4f);
            return;
        }
        if (IsRecording) return;
        if (echoBot != null && echoBot.IsReplaying) echoBot.StopReplay();

        frames.Clear();
        RecordingTime = 0f;
        sampleTimer = 0f;
        pendingInteract = false;
        IsRecording = true;
        AddFrame();
        Message("● Enregistrement en cours… (F pour arrêter)", 2f);
    }

    public void StopRecording()
    {
        if (!IsRecording) return;
        AddFrame();
        IsRecording = false;
        Message($"Séquence enregistrée ({RecordedDuration:0.0} s). Appuyez sur G pour qu'EchoBot la rejoue.", 3.5f);
    }

    public void Replay()
    {
        if (IsRecording) StopRecording();
        if (frames.Count < 2)
        {
            Message("Aucune séquence enregistrée. Appuyez sur R pour enregistrer.");
            return;
        }
        if (echoBot == null) return;
        echoBot.StartReplay(frames);
    }

    /// <summary>Appelé par le joueur quand il appuie sur E : l'action sera rejouée par EchoBot.</summary>
    public void MarkInteract()
    {
        if (IsRecording) pendingInteract = true;
    }

    void AddFrame()
    {
        frames.Add(new RecordFrame
        {
            time = RecordingTime,
            position = transform.position,
            rotation = transform.rotation,
            interact = pendingInteract
        });
        pendingInteract = false;
        UpdatePathLine();
    }

    void UpdatePathLine()
    {
        if (pathLine == null) return;
        pathLine.positionCount = frames.Count;
        for (int i = 0; i < frames.Count; i++)
            pathLine.SetPosition(i, frames[i].position + Vector3.up * 0.06f);
    }

    static void Message(string text, float duration = 3f)
    {
        if (GameManager.Instance != null) GameManager.Instance.ShowMessage(text, duration);
    }
}
