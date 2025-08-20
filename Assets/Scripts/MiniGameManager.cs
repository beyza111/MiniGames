using UnityEngine;
using System.Collections.Generic;

public class MiniGameManager : MonoBehaviour
{
    public KeyCode key = KeyCode.E;
    public List<GameObject> panels = new();

    int index = 0;
    GameObject active;

    void Start()
    {
        foreach (var p in panels)
        {
            if (!p) continue;
            p.SetActive(false);

            var mb = p.GetComponent<MiniGameBase>();
            if (mb)
            {
                mb.OnPlayerWon += () => { if (active == p) Win(); };
                mb.OnPlayerLost += () => { if (active == p) Lose(); };
            }
            else
            {
                Debug.LogWarning($"[MG] No MiniGameBase found at the root of '{p.name}'.");
            }
        }
    }

    void Update()
    {
       
        if (Input.GetKeyDown(key))
            OpenCurrent();
    }

    void OpenCurrent()
    {
        if (panels.Count == 0) return;

      
        CloseActive();

        active = panels[index];
        active.SetActive(true);

      
        var mb = active.GetComponent<MiniGameBase>();
        if (mb) mb.StartGame();
        else Debug.LogWarning($"[MG] '{active.name}' has no MiniGameBase.");
    }

    void CloseActive()
    {
        if (!active) return;
        active.SetActive(false);
        active = null;
    }

    public void Win()
    {
        CloseActive();
        if (panels.Count == 0) return;
        index = Mathf.Min(index + 1, panels.Count - 1);
    }

    public void Lose()
    {
        CloseActive();
        index = 0;
    }
}
