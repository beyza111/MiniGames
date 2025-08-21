using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;

public class WireConnectMiniGame : MiniGameBase
{
    [Header("Plugs")]
    public Plug[] leftPlugs;
    public Plug[] rightPlugs;

    [Header("Line")]
    public Image linePrefab;
    public RectTransform lineLayer;

    [Header("FX")]
    public Color okColor = Color.white;
    public Color wrongColor = Color.red;
    public float wrongFlashTime = 0.15f;

    [Header("Shuffle")]
    public bool shuffleOnStart = true;

    [Header("Line Snap")]
    [SerializeField] float startPad = 24f;
    [SerializeField] float endPad = 24f;
    [SerializeField] bool snapHover = true;

    Plug activeLeft;
    Image tempLine;
    int connectedCount;

    Vector2[] leftSlots;
    Vector2[] rightSlots;
    bool anchorsNormalized;
    bool slotsCached;

    GraphicRaycaster _gr;
    readonly List<RaycastResult> _rayBuf = new List<RaycastResult>(8);

    public MiniGameState State => state;

    void Awake()
    {
        _gr = GetComponentInParent<GraphicRaycaster>();
    }

    public override void StartGame()
    {
        base.StartGame();

        foreach (var p in leftPlugs) p.Unlock();
        foreach (var p in rightPlugs) p.Unlock();

        foreach (Transform c in lineLayer) Destroy(c.gameObject);

        connectedCount = 0;
        activeLeft = null;
        if (tempLine) { Destroy(tempLine.gameObject); tempLine = null; }

        if (!anchorsNormalized)
        {
            NormalizeAnchors(leftPlugs, new Vector2(0f, 0.5f));
            NormalizeAnchors(rightPlugs, new Vector2(1f, 0.5f));
            anchorsNormalized = true;
        }

        CacheSlotsOnce();

        if (shuffleOnStart)
        {
            ShuffleColumn(leftPlugs, leftSlots);
            ShuffleColumn(rightPlugs, rightSlots);
        }
    }

 
    void Update()
    {
        if (state != MiniGameState.Running) return;

        if (Input.GetMouseButtonDown(0) && activeLeft == null)
        {
            var lp = GetLeftUnder(Input.mousePosition);
            if (lp != null)
                BeginDragFrom(lp, (Vector2)Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && activeLeft != null)
        {
            DragTo((Vector2)Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0) && activeLeft != null)
        {
            EndDrag((Vector2)Input.mousePosition);
        }
    }
  

    public void BeginDragFrom(Plug left, Vector2 screenPos)
    {
        if (state != MiniGameState.Running) return;

        activeLeft = left;
        tempLine = Instantiate(linePrefab, lineLayer);
        tempLine.raycastTarget = false;
        tempLine.color = okColor;
        tempLine.rectTransform.pivot = new Vector2(0, 0.5f);
        tempLine.transform.SetAsFirstSibling();

        UpdateLine(left.RT.position, screenPos);
    }

    public void DragTo(Vector2 screenPos)
    {
        if (!activeLeft || !tempLine) return;

        Plug overRight = snapHover ? GetRightUnder(screenPos) : null;
        Vector3 endPos = overRight ? overRight.RT.position : (Vector3)screenPos;

        if (overRight)
            tempLine.color = (overRight.id == activeLeft.id) ? okColor : wrongColor;
        else
            tempLine.color = okColor;

        UpdateLine(activeLeft.RT.position, endPos);
    }

    public void EndDrag(Vector2 screenPos)
    {
        if (!activeLeft || !tempLine) return;

        var hitRight = GetRightUnder(screenPos);

        if (hitRight && hitRight.id == activeLeft.id)
        {
            activeLeft.Lock();
            hitRight.Lock();

            FreezeLine(tempLine);

            connectedCount++;
            tempLine = null;
            activeLeft = null;

            if (connectedCount >= Mathf.Min(leftPlugs.Length, rightPlugs.Length))
                PlayerWin();
        }
        else
        {
            UseAttempt();
            if (!HasAttemptsLeft)
            {
                if (tempLine) Destroy(tempLine.gameObject);
                tempLine = null;
                activeLeft = null;
                return;
            }

            StartCoroutine(FlashWrongThenCancel());
        }
    }

    void UpdateLine(Vector3 a, Vector3 b)
    {
        var rt = tempLine.rectTransform;
        Vector3 dir = b - a;
        float len = dir.magnitude;
        if (len < 1f) len = 1f;

        Vector3 n = dir / len;
        a += n * startPad;
        b -= n * endPad;

        Vector3 d2 = b - a;

        rt.position = a;
        rt.sizeDelta = new Vector2(d2.magnitude, 40f);
        rt.rotation = Quaternion.FromToRotation(Vector3.right, d2.normalized);
    }

    void FreezeLine(Image line)
    {
        if (line) line.raycastTarget = false;
    }

    // -------- UI Raycast yardımcıları (sadece okumak için) --------
    Plug GetLeftUnder(Vector2 screenPos)
    {
        _rayBuf.Clear();
        var ped = new PointerEventData(EventSystem.current) { position = screenPos };
        _gr.Raycast(ped, _rayBuf);
        return _rayBuf
            .Select(r => r.gameObject.GetComponentInParent<Plug>())
            .FirstOrDefault(p => p && p.isLeft && !p.IsLocked);
    }

    Plug GetRightUnder(Vector2 screenPos)
    {
        _rayBuf.Clear();
        var ped = new PointerEventData(EventSystem.current) { position = screenPos };
        _gr.Raycast(ped, _rayBuf);
        return _rayBuf
            .Select(r => r.gameObject.GetComponentInParent<Plug>())
            .FirstOrDefault(p => p && !p.isLeft && !p.IsLocked);
    }
  

    System.Collections.IEnumerator FlashWrongThenCancel()
    {
        if (tempLine) tempLine.color = wrongColor;
        yield return new WaitForSeconds(wrongFlashTime);
        if (tempLine) Destroy(tempLine.gameObject);
        tempLine = null;
        activeLeft = null;
    }

    void NormalizeAnchors(Plug[] plugs, Vector2 anchor)
    {
        foreach (var p in plugs)
        {
            var rt = p.RT;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    void CacheSlotsOnce()
    {
        if (slotsCached) return;
        leftSlots = leftPlugs.Select(p => Snap(p.RT.anchoredPosition)).ToArray();
        rightSlots = rightPlugs.Select(p => Snap(p.RT.anchoredPosition)).ToArray();
        slotsCached = true;
    }

    void ShuffleColumn(Plug[] plugs, Vector2[] slots)
    {
        for (int i = plugs.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (plugs[i], plugs[j]) = (plugs[j], plugs[i]);
        }
        int n = Mathf.Min(plugs.Length, slots.Length);
        for (int i = 0; i < n; i++)
            plugs[i].RT.anchoredPosition = slots[i];
    }

    Vector2 Snap(Vector2 v) => new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
}
