using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NoteHitMiniGame : MiniGameBase
{
    [Header("Setup")]
    public RectTransform spawnPoint;
    public RectTransform hitZone;
    public GameObject notePrefab;
    public Image progressFill;

    [Header("Hit Zone Feedback")]
    public Image hitZoneImage;
    public Color okColor = Color.green;
    public Color missColor = Color.red;
    public float flashTime = 0.12f;

    [Header("Gameplay")]
    public int notesToWin = 5;
    public int maxMisses = 3;
    public float noteSpeed = 250f;
    public float spawnInterval = 1.2f;
    public KeyCode hitKey = KeyCode.Space;

    int hits;
    int misses;
    float timer;

    Color _baseColor;
    Coroutine _flashCR;
    readonly List<NoteObject> _activeNotes = new();

    void OnEnable() => StartGame();

    public override void StartGame()
    {
        base.StartGame();
        hits = 0;
        misses = 0;
        timer = 0f;
        if (progressFill) progressFill.fillAmount = 0f;

        if (hitZoneImage)
        {
            _baseColor = hitZoneImage.color;
            if (_flashCR != null) StopCoroutine(_flashCR);
            hitZoneImage.color = _baseColor;
        }

        _activeNotes.Clear();
    }

    void Update()
    {
        if (state != MiniGameState.Running) return;

        if (Input.GetKeyDown(hitKey) && !AnyNoteInZone())
            FlashZone(missColor);

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnNote();
            timer = 0f;
        }
    }

    void SpawnNote()
    {
        var go = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity, spawnPoint.parent);
        var note = go.AddComponent<NoteObject>();
        note.Init(this, noteSpeed, hitZone, hitKey);
        note.onResolved += OnNoteResolved;
        _activeNotes.Add(note);
    }

    void OnNoteResolved(NoteObject n)
    {
        n.onResolved -= OnNoteResolved;
        _activeNotes.Remove(n);
    }

    bool AnyNoteInZone()
    {
        for (int i = 0; i < _activeNotes.Count; i++)
            if (_activeNotes[i] && _activeNotes[i].IsInHitZone()) return true;
        return false;
    }

    void FlashZone(Color c)
    {
        if (!hitZoneImage) return;
        if (_flashCR != null) StopCoroutine(_flashCR);
        _flashCR = StartCoroutine(_Flash(c));
    }

    IEnumerator _Flash(Color c)
    {
        hitZoneImage.color = c;
        yield return new WaitForSeconds(flashTime);
        hitZoneImage.color = _baseColor;
        _flashCR = null;
    }

    public void RegisterHit()
    {
        FlashZone(okColor);
        hits++;
        if (progressFill) progressFill.fillAmount = (float)hits / notesToWin;
        if (hits >= notesToWin) PlayerWin();
    }

    public void RegisterMiss()
    {
        FlashZone(missColor);
        misses++;
        if (misses >= maxMisses) PlayerLose();
    }
}

public class NoteObject : MonoBehaviour
{
    float speed;
    RectTransform rt;
    RectTransform hitZone;
    NoteHitMiniGame owner;
    KeyCode hitKey;
    bool resolved;

    public System.Action<NoteObject> onResolved;

    public void Init(NoteHitMiniGame owner, float speed, RectTransform hitZone, KeyCode hitKey)
    {
        this.owner = owner;
        this.speed = speed;
        this.hitZone = hitZone;
        this.hitKey = hitKey;
        rt = GetComponent<RectTransform>();
    }

    public bool IsInHitZone()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(hitZone, rt.position, null);
    }

    void Update()
    {
        if (resolved || owner.state != MiniGameState.Running) return;

        rt.anchoredPosition += Vector2.right * speed * Time.deltaTime;

        if (Input.GetKeyDown(hitKey) && IsInHitZone())
        {
            Resolve(true);
            return;
        }

        if (rt.position.x > hitZone.position.x + 100f)
            Resolve(false);
    }

    void Resolve(bool hit)
    {
        if (resolved) return;
        resolved = true;

        if (hit) owner.RegisterHit();
        else owner.RegisterMiss();

        onResolved?.Invoke(this);
        Destroy(gameObject);
    }
}
