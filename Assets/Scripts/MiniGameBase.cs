using UnityEngine;
using System;

public enum MiniGameState { Running, Completed, Failed }

public abstract class MiniGameBase : MonoBehaviour
{
   
    public static event Action<MiniGameBase> OnAnyGameStarted;
    public static event Action<MiniGameBase> OnAnyGameWon;
    public static event Action<MiniGameBase> OnAnyGameLost;

    public string id;
    public string title;
    public MiniGameState state = MiniGameState.Running;

    [Header("Attempts (global)")]
    public int maxAttempts = 3;
    protected int attemptsUsed = 0;

    public bool HasAttemptsLeft => attemptsUsed < maxAttempts;

    public virtual void StartGame()
    {
        state = MiniGameState.Running;
        ResetAttempts();
        OnAnyGameStarted?.Invoke(this);
    }
    protected virtual void PlayerWin()
    {
        state = MiniGameState.Completed;
        OnAnyGameWon?.Invoke(this);
    }

    protected virtual void PlayerLose()
    {
        state = MiniGameState.Failed;
        OnAnyGameLost?.Invoke(this);
    }

    public virtual void UseAttempt()
    {
        attemptsUsed++;
        Debug.Log($"[MiniGame] Attempt used {attemptsUsed}/{maxAttempts}");
        if (!HasAttemptsLeft)
            PlayerLose();
    }

    public virtual void ResetAttempts() => attemptsUsed = 0;

    [ContextMenu("Start test")]
    void _TestStartInEditor() => StartGame();
}
