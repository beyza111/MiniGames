using UnityEngine;
using UnityEngine.UI;

public class NoteHitMiniGame : MiniGameBase
{
    [Header("Setup")]
    public RectTransform spawnPoint;
    public RectTransform hitZone;
    public GameObject notePrefab;
    public Image progressFill;

    [Header("Gameplay")]
    public int notesToWin = 5;
    public int maxMisses = 3;
    public float noteSpeed = 250f;
    public float spawnInterval = 1.2f;
    public KeyCode hitKey = KeyCode.Space;

    int hits;
    int misses;
    float timer;

    void OnEnable() => StartGame();

    public override void StartGame()
    {
        base.StartGame();
        hits = 0;
        misses = 0;
        timer = 0f;
        if (progressFill) progressFill.fillAmount = 0f;
    }

    void Update()
    {
        if (state != MiniGameState.Running) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnNote();
            timer = 0f;
        }
    }

    void SpawnNote()
    {
        var note = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity, spawnPoint.parent);
        note.AddComponent<NoteObject>().Init(this, noteSpeed, hitZone, hitKey);
    }

    public void RegisterHit()
    {
        hits++;
        if (progressFill) progressFill.fillAmount = (float)hits / notesToWin;
        if (hits >= notesToWin) PlayerWin();
    }

    public void RegisterMiss()
    {
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

    public void Init(NoteHitMiniGame owner, float speed, RectTransform hitZone, KeyCode hitKey)
    {
        this.owner = owner;
        this.speed = speed;
        this.hitZone = hitZone;
        this.hitKey = hitKey;
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (resolved) return;

        rt.anchoredPosition += Vector2.right * speed * Time.deltaTime;

        if (Input.GetKeyDown(hitKey) && RectTransformUtility.RectangleContainsScreenPoint(hitZone, rt.position))
        {
            resolved = true;
            owner.RegisterHit();
            Destroy(gameObject);
        }

        if (rt.position.x > hitZone.position.x + 100f) 
        {
            resolved = true;
            owner.RegisterMiss();
            Destroy(gameObject);
        }
    }
}
