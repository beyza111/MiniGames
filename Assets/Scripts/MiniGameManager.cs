using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MiniGameManager : MonoBehaviour
{
    public KeyCode key = KeyCode.E;
    public bool isNearMachine = false;

    public List<GameObject> panels = new();

    public Image fadeImage;
    public float fadeInTime = 0.4f;
    public float holdBlack = 2.0f;
    public float fadeOutTime = 0.4f;

    int index = 0;
    GameObject active;
    bool isTransitioning = false;

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

        if (fadeImage)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
            fadeImage.raycastTarget = false;
        }
    }

    void Update()
    {
        if (!isTransitioning && active == null && isNearMachine && Input.GetKeyDown(key))
        {
            StartCoroutine(OpenWithFade());
        }
    }

    IEnumerator OpenWithFade()
    {
        if (panels.Count == 0) yield break;
        isTransitioning = true;

        yield return StartCoroutine(FadeAlpha(0f, 1f, fadeInTime));

        if (holdBlack > 0f)
            yield return new WaitForSeconds(holdBlack);

        OpenCurrentInternal();

        yield return StartCoroutine(FadeAlpha(1f, 0f, fadeOutTime));

        isTransitioning = false;
    }

    IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (!fadeImage || duration <= 0f)
        {
            if (fadeImage)
            {
                var c = fadeImage.color; c.a = to; fadeImage.color = c;
            }
            yield break;
        }

        float t = 0f;
        var col = fadeImage.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            col.a = a;
            fadeImage.color = col;
            yield return null;
        }
        col.a = to;
        fadeImage.color = col;
    }

    void OpenCurrentInternal()
    {
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

    public void SetMachineProximity(bool isNear) => isNearMachine = isNear;
}
