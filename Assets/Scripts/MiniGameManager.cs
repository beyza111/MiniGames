using UnityEngine;
using System.Collections.Generic;

public class MiniGameManager : MonoBehaviour
{
    public KeyCode key = KeyCode.E;
    public List<GameObject> panels = new();

    int index = 0;
    GameObject active;

    void OnEnable()
    {
       
        MiniGameBase.OnAnyGameWon += HandleAnyGameWon;
        MiniGameBase.OnAnyGameLost += HandleAnyGameLost;
    }

    void OnDisable()
    {
   
        MiniGameBase.OnAnyGameWon -= HandleAnyGameWon;
        MiniGameBase.OnAnyGameLost -= HandleAnyGameLost;
    }

    void Start()
    {
        foreach (var p in panels)
        {
            if (!p) continue;
            p.SetActive(false);

         
            if (!p.GetComponent<MiniGameBase>())
                Debug.LogWarning($"[MG] No MiniGameBase found at the root of '{p.name}'.");
        }
    }

    void Update()
    {
   
        if (Input.GetKeyDown(key) && active == null)
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

 
    void HandleAnyGameWon(MiniGameBase game)
    {
      
        if (active != null && game.gameObject == active)
            Win();
    }

    void HandleAnyGameLost(MiniGameBase game)
    {
        if (active != null && game.gameObject == active)
            Lose();
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
