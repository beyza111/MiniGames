using UnityEngine;
using System;
using UnityEngine.EventSystems;

public enum MiniGameState { Running, Completed, Failed }

public abstract class MiniGameBase : MonoBehaviour
{
    public string id;
    public string title;
    public MiniGameState state = MiniGameState.Running;

    [Header("Attempts (global)")]
    public int maxAttempts = 3;
    protected int attemptsUsed = 0;

    public Action OnGameStarted;
    public Action OnPlayerWon;
    public Action OnPlayerLost;

    public bool HasAttemptsLeft => attemptsUsed < maxAttempts;

    public virtual void StartGame()
    {
        state = MiniGameState.Running;
        ResetAttempts();
        OnGameStarted?.Invoke();
    }

    protected virtual void PlayerWin()
    {
        state = MiniGameState.Completed;
        OnPlayerWon?.Invoke();
    }

    protected virtual void PlayerLose()
    {
        state = MiniGameState.Failed;
        OnPlayerLost?.Invoke();
    }

    public virtual void UseAttempt()
    {
        attemptsUsed++;
        Debug.Log($"[MiniGame] Attempt used {attemptsUsed}/{maxAttempts}");
        if (!HasAttemptsLeft)
            PlayerLose();
    }

    public virtual void ResetAttempts()
    {
        attemptsUsed = 0;
    }

    [ContextMenu("Start test")]
    void _TestStartInEditor() => StartGame();
}

public class Player
{
    public bool isWin;

    public void Subscribe(MiniGameBase game)
    {
        game.OnGameStarted += () => Debug.Log("MiniGame started.");
        game.OnPlayerWon += () => OnMiniGameWon(game);
        game.OnPlayerLost += () => OnMiniGameLost(game);
    }

    public void OnMiniGameWon(MiniGameBase game)
    {
        isWin = true;
        Debug.Log($"Player won {game.title}");
    }

    public void OnMiniGameLost(MiniGameBase game)
    {
        isWin = false;
        Debug.Log($"Player lost {game.title}");
    }
}

public class Interaction
{
    public float inputWindowSec = 1f;
    public KeyCode requiredKey = KeyCode.None;

    float timer;
    bool windowOpen;
    Action<bool> onResolved;

    public void StartInteraction(KeyCode key, Action<bool> callback)
    {
        requiredKey = key;
        timer = inputWindowSec;
        windowOpen = true;
        onResolved = callback;
    }

    public void HandleInput()
    {
        if (!windowOpen) return;

        if (Input.GetKeyDown(requiredKey))
        {
            windowOpen = false;
            onResolved?.Invoke(true);
        }
        else if (Input.anyKeyDown)
        {
            windowOpen = false;
            onResolved?.Invoke(false);
        }
    }

    public void Update()
    {
        if (!windowOpen) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            windowOpen = false;
            onResolved?.Invoke(false);
        }
    }

    public void ResetRound()
    {
        windowOpen = false;
        timer = 0f;
        onResolved = null;
        requiredKey = KeyCode.None;
    }
}
