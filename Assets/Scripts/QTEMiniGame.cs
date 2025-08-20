using UnityEngine;
using UnityEngine.UI;

public class QTEMiniGame : MiniGameBase
{
    [Header("Input")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public bool useArrowsInstead = false;

    [Header("Win/Lose")]
    public int tapsToWin = 10;
    public float perTapWindow = 0.85f;
    public bool randomStart = true;
    public bool loseOnWrongKey = true; 

    [Header("Logs")]
    public bool logDebug = true;

    [Header("UI")]
    public Image leftHint;
    public Image rightHint;
    public Image progressFill;

    [Header("UI Vurgu")]
    public float activeAlpha = 1f;
    public float inactiveAlpha = 0.35f;

    private Interaction _inter = new Interaction();
    private int _done;
    private bool _expectLeft;
    private bool _running;

    private Color _cL, _cR;

    void Awake()
    {
        if (useArrowsInstead)
        {
            leftKey = KeyCode.LeftArrow;
            rightKey = KeyCode.RightArrow;
        }
    }

    void OnEnable()
    {
        StartGame();
    }

    public override void StartGame()
    {
        base.StartGame(); 

        _running = true;
        _done = 0;
        if (progressFill) progressFill.fillAmount = 0f;

        _expectLeft = randomStart ? (Random.value > 0.5f) : true;

        CacheInitialColors();
        UpdateHints();

        StartRound();
    }

    void Update()
    {
        if (!_running || state != MiniGameState.Running) return;

        _inter.HandleInput();
        _inter.Update();
    }

    void StartRound()
    {
        var required = _expectLeft ? leftKey : rightKey;
        _inter.inputWindowSec = perTapWindow;
        _inter.StartInteraction(required, OnRoundResolved);
    }

    void OnRoundResolved(bool success)
    {
        if (!success)
        {
            if (loseOnWrongKey)
            {
                if (logDebug) Debug.Log("QTE: FAIL (instant wrong)");
                ImmediateFail();
                return;
            }
            else
            {
                UseAttempt(); 
                if (!HasAttemptsLeft)
                {
                    if (logDebug) Debug.Log("QTE: FAIL (no attempts left)");
                    StopRunningAfterLose();
                    return;
                }
            }
        }
        else
        {
            _done++;
            if (logDebug) Debug.Log($"QTE: hit {_done}/{tapsToWin}");
            if (progressFill) progressFill.fillAmount = (float)_done / tapsToWin;

            if (_done >= tapsToWin)
            {
                if (logDebug) Debug.Log("QTE: WIN");
                Win();
                return;
            }
        }

        _expectLeft = !_expectLeft;
        UpdateHints();
        StartRound();
    }

    void Win()
    {
        _running = false;
        if (progressFill) progressFill.fillAmount = 1f;
        _inter.ResetRound();
        PlayerWin();
        gameObject.SetActive(false);
    }

 
    void ImmediateFail()
    {
        _running = false;
        _inter.ResetRound();
        PlayerLose();
        gameObject.SetActive(false);
    }

    void StopRunningAfterLose()
    {
        _running = false;
        _inter.ResetRound();
        gameObject.SetActive(false);
    }

    void CacheInitialColors()
    {
        if (leftHint) _cL = leftHint.color;
        if (rightHint) _cR = rightHint.color;
    }

    void UpdateHints()
    {
        if (leftHint)
        {
            _cL.a = _expectLeft ? activeAlpha : inactiveAlpha;
            leftHint.color = _cL;
        }
        if (rightHint)
        {
            _cR.a = _expectLeft ? inactiveAlpha : activeAlpha;
            rightHint.color = _cR;
        }
    }
}
