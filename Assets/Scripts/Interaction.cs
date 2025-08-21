using System;
using UnityEngine;

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
