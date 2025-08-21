using UnityEngine;

public class MiniGameMachine : MonoBehaviour
{
    public MiniGameManager manager;
    Renderer rend;
    Color baseColor;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend) baseColor = rend.material.color;
    }

    public void StartMiniGame()
    {
        if (manager != null)
            manager.StartMiniGameWithFade(true); 
    }

    public void SetHighlight(bool on)
    {
        if (!rend) return;
        rend.material.color = on ? Color.red : baseColor;
    }
}
