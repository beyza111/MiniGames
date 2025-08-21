using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MiniGameManager : MonoBehaviour
{
    public MonoBehaviour playerController;

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

    bool suppressUnlockOnce = false; // prevents unlock during panel switching

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

        if (panels.Count > 0 && active == panels[0])
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) OpenGameAt(1);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) OpenGameAt(2);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) OpenGameAt(3);
        }
    }

    IEnumerator OpenWithFade()
    {
        if (panels.Count == 0) yield break;
        isTransitioning = true;
        if (playerController) playerController.enabled = false; // lock player when UI opens

        // unlock cursor so UI is clickable
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return StartCoroutine(FadeAlpha(0f, 1f, fadeInTime));

        if (holdBlack > 0f)
            yield return new WaitForSeconds(holdBlack);

        OpenCurrentInternal();

        yield return StartCoroutine(FadeAlpha(1f, 0f, fadeOutTime));

        isTransitioning = false;
        UnlockIfSafe();
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
        suppressUnlockOnce = true;
        CloseActive();

        active = panels[index];
        active.SetActive(true);

        var mb = active.GetComponent<MiniGameBase>();
        if (mb) mb.StartGame();
    }

    void CloseActive()
    {
        if (!active) return;
        active.SetActive(false);
        active = null;

        if (suppressUnlockOnce)
        {
            suppressUnlockOnce = false;
            return;
        }

        UnlockIfSafe();
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

    public void StartMiniGameWithFade(bool force = false)
    {
        if (isTransitioning || active != null) return;
        if (!force && !isNearMachine) return;

        StartCoroutine(OpenWithFade());
    }

    public void OpenGameAt(int i)
    {
        if (i < 0 || i >= panels.Count) return;
        index = i;
        OpenCurrentInternal();
    }

    void UnlockIfSafe()
    {
        if (!isTransitioning && active == null)
        {
            if (playerController) playerController.enabled = true; // unlock only when UI fully closed

            // lock cursor again when UI is closed
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
