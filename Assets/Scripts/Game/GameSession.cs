using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public static event Action<int> LevelPrepared;
    public static event Action<string> CountdownChanged;
    public static event Action RunStarted;
    public static event Action<int, int, Vector3> ShardCollected;
    public static event Action<bool> PauseChanged;
    public static event Action<Vector3> RunWon;
    public static event Action<Vector3> RunLost;

    public bool HasWorld => world != null;
    public bool Running { get; private set; }
    public bool Paused { get; private set; }
    public int Shards { get; private set; }
    public int Target { get; private set; }
    public float Elapsed { get; private set; }
    public Rect Arena { get; private set; }

    public bool LastWin { get; private set; }
    public int LastStars { get; private set; }
    public int LastGems { get; private set; }
    public bool LastNewBest { get; private set; }

    GameObject world;
    Camera arenaCamera;
    LevelDef level;
    int activeShards;
    int levelIndex;
    float nextPickupTime;
    bool ending;
    Vector2 playerStart = new Vector2(0f, -4.5f);
    readonly List<Vector2> reserved = new List<Vector2>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        LevelPrepared = null;
        CountdownChanged = null;
        RunStarted = null;
        ShardCollected = null;
        PauseChanged = null;
        RunWon = null;
        RunLost = null;
    }

    void Awake()
    {
        Instance = this;
        arenaCamera = GetComponent<Camera>();
        Physics2D.gravity = Vector2.zero;
    }

    void OnEnable()
    {
        OrbPickup.Collected += onPickupCollected;
        OrbVitals.Damaged += onOrbDamaged;
        OrbVitals.Died += onOrbDied;
    }

    void OnDisable()
    {
        OrbPickup.Collected -= onPickupCollected;
        OrbVitals.Damaged -= onOrbDamaged;
        OrbVitals.Died -= onOrbDied;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void BuildLevel(int index)
    {
        Cleanup();
        levelIndex = index;
        level = Catalog.Levels[index];
        Shards = 0;
        Target = level.shardTarget;
        Elapsed = 0f;
        activeShards = 0;
        ending = false;
        reserved.Clear();

        arenaCamera.orthographic = true;
        arenaCamera.orthographicSize = 8f;
        arenaCamera.clearFlags = CameraClearFlags.SolidColor;
        arenaCamera.backgroundColor = UiTheme.BgDeep;
        arenaCamera.transform.position = new Vector3(0f, 0f, -10f);

        float halfWidth = Mathf.Min(8f * arenaCamera.aspect, 6f) - 0.35f;
        Arena = Rect.MinMaxRect(-halfWidth, -7.4f, halfWidth, 5.4f);

        world = new GameObject("arenaWorld");
        BuildFloor();
        SpawnPlayer();

        reserved.Add(playerStart);
        for (int i = 0; i < level.spikes; i++) SpawnSpikePlate(i);
        for (int i = 0; i < level.patrollers; i++) SpawnRailSentry(i);
        for (int i = 0; i < level.turrets; i++) SpawnWallCannon(i);
        for (int i = 0; i < level.chasers; i++) SpawnHunterDrone(i);
        for (int i = 0; i < Mathf.Min(4, Target); i++) SpawnShard();
        nextPickupTime = Time.time + 7f;
        LevelPrepared?.Invoke(index);
    }

    void BuildFloor()
    {
        Vector2 size = Arena.size;
        Vector2 center = Arena.center;
        Transform root = world.transform;

        ArenaDraw.Rect(root, "arenaFloor", UiTheme.Bg, size, -20, center);

        Color grid = new Color(1f, 1f, 1f, 0.045f);
        for (float x = Mathf.Ceil(Arena.xMin); x < Arena.xMax; x += 1f)
            ArenaDraw.Rect(root, "gridLineVertical", grid, new Vector2(0.025f, size.y), -19, new Vector2(x, center.y));
        for (float y = Mathf.Ceil(Arena.yMin); y < Arena.yMax; y += 1f)
            ArenaDraw.Rect(root, "gridLineHorizontal", grid, new Vector2(size.x, 0.025f), -19, new Vector2(center.x, y));

        float thickness = 0.14f;
        Color edge = UiTheme.Outline;
        BuildWall(root, edge, new Vector2(size.x + thickness, thickness), new Vector2(center.x, Arena.yMax));
        BuildWall(root, edge, new Vector2(size.x + thickness, thickness), new Vector2(center.x, Arena.yMin));
        BuildWall(root, edge, new Vector2(thickness, size.y + thickness), new Vector2(Arena.xMin, center.y));
        BuildWall(root, edge, new Vector2(thickness, size.y + thickness), new Vector2(Arena.xMax, center.y));
    }

    void BuildWall(Transform root, Color color, Vector2 size, Vector2 pos)
    {
        ArenaDraw.Rect(root, "arenaWall", color, size, -10, pos);

        GameObject colliderObject = new GameObject("wallCollider");
        colliderObject.transform.SetParent(root, false);
        Vector2 outward = size.x > size.y
            ? new Vector2(0f, Mathf.Sign(pos.y - Arena.center.y))
            : new Vector2(Mathf.Sign(pos.x - Arena.center.x), 0f);
        colliderObject.transform.position = pos + outward * 0.5f;
        BoxCollider2D box = colliderObject.AddComponent<BoxCollider2D>();
        box.size = size.x > size.y ? new Vector2(size.x + 2f, 1f) : new Vector2(1f, size.y + 2f);
    }

    void SpawnPlayer()
    {
        GameObject go = new GameObject("playerOrb");
        go.transform.SetParent(world.transform, false);
        go.transform.position = playerStart;
        PlayerOrb orb = go.AddComponent<PlayerOrb>();

        int hearts = 3;
        bool shield = false;
        if (GameData.BoostEnabled(0)) { shield = true; GameData.AddBoost(0, -1); }
        if (GameData.BoostEnabled(1)) { hearts = 4; GameData.AddBoost(1, -1); }
        orb.Init(GameData.SelectedOrb, hearts, shield);
    }

    void SpawnShard()
    {
        OrbPickup.Create(PickupKind.Shard, RandomPoint(2.5f, 0.9f), world.transform);
        activeShards++;
    }

    void SpawnSpikePlate(int i)
    {
        GameObject go = new GameObject("spikePlate");
        go.transform.SetParent(world.transform, false);
        Vector2 p = RandomPoint(3.5f, 1.2f, 2.4f);
        reserved.Add(p);
        go.transform.position = p;
        go.AddComponent<SpikePlate>().Setup(i * 0.7f);
    }

    void SpawnRailSentry(int i)
    {
        GameObject go = new GameObject("railSentry");
        go.transform.SetParent(world.transform, false);
        float y = Mathf.Lerp(Arena.yMin + 4.5f, Arena.yMax - 1.2f, level.patrollers == 1 ? 0.55f : (float)i / (level.patrollers - 1));
        Vector2 from = new Vector2(Arena.xMin + 0.8f, y);
        Vector2 to = new Vector2(Arena.xMax - 0.8f, y);
        go.AddComponent<RailSentry>().Setup(from, to, 3.6f - i * 0.4f, i * 0.5f);
    }

    void SpawnWallCannon(int i)
    {
        GameObject go = new GameObject("wallCannon");
        go.transform.SetParent(world.transform, false);
        bool left = i % 2 == 0;
        float y = UnityEngine.Random.Range(Arena.yMin + 4f, Arena.yMax - 0.8f);
        go.transform.position = new Vector2(left ? Arena.xMin + 0.35f : Arena.xMax - 0.35f, y);
        go.AddComponent<WallCannon>().Setup(2.4f - level.turrets * 0.1f, i * 0.9f);
    }

    void SpawnHunterDrone(int i)
    {
        GameObject go = new GameObject("hunterDrone");
        go.transform.SetParent(world.transform, false);
        float x = (i % 2 == 0 ? -1f : 1f) * (Arena.width * 0.35f);
        go.transform.position = new Vector2(x, Arena.yMax - 1.2f - (i / 2) * 1.5f);
        go.AddComponent<HunterDrone>().Setup(level.chaserSpeed);
    }

    public Vector2 RandomPoint(float minFromPlayer, float inset, float minFromReserved = 1.5f)
    {
        Vector2 playerPos = PlayerOrb.Instance != null ? (Vector2)PlayerOrb.Instance.transform.position : playerStart;
        Vector2 best = Arena.center;
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector2 p = new Vector2(UnityEngine.Random.Range(Arena.xMin + inset, Arena.xMax - inset), UnityEngine.Random.Range(Arena.yMin + inset, Arena.yMax - inset));
            best = p;
            if (Vector2.Distance(p, playerPos) < minFromPlayer) continue;
            bool clear = true;
            foreach (Vector2 r in reserved) if (Vector2.Distance(p, r) < minFromReserved) { clear = false; break; }
            if (clear) return p;
        }
        return best;
    }

    public void StartCountdown() => StartCoroutine(CountdownRoutine());

    IEnumerator CountdownRoutine()
    {
        Running = false;
        string[] steps = { "3", "2", "1" };
        foreach (string step in steps)
        {
            CountdownChanged?.Invoke(step);
            yield return new WaitForSecondsRealtime(0.65f);
        }
        Running = true;
        CountdownChanged?.Invoke("GO!");
        RunStarted?.Invoke();
        yield return new WaitForSecondsRealtime(0.6f);
        CountdownChanged?.Invoke("");
    }

    void Update()
    {
        if (!Running || Paused) return;
        Elapsed += Time.deltaTime;

        if (Time.time >= nextPickupTime && PlayerOrb.Instance != null)
        {
            nextPickupTime = Time.time + UnityEngine.Random.Range(8f, 12f);
            OrbVitals vitals = PlayerOrb.Instance.Vitals;
            PickupKind kind = vitals.Hearts < vitals.MaxHearts && UnityEngine.Random.value < 0.5f
                ? PickupKind.Heart
                : (UnityEngine.Random.value < 0.5f ? PickupKind.Shield : PickupKind.Boost);
            OrbPickup.Create(kind, RandomPoint(2f, 0.9f), world.transform);
        }
    }

    void onPickupCollected(PickupKind kind, Vector3 at)
    {
        if (kind != PickupKind.Shard || !Running) return;
        Shards++;
        activeShards--;
        ShardCollected?.Invoke(Shards, Target, at);
        if (Shards >= Target) EndLevel(true);
        else if (Shards + activeShards < Target) SpawnShard();
    }

    void onOrbDamaged(OrbHitReport report)
    {
        if (!Paused && report.heartsLeft > 0) StartCoroutine(HitStopRoutine(0.07f));
    }

    void onOrbDied(Vector3 at) => EndLevel(false);

    public void SetPaused(bool paused)
    {
        if (Paused == paused) return;
        Paused = paused;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
        PauseChanged?.Invoke(paused);
    }

    IEnumerator HitStopRoutine(float seconds)
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(seconds);
        Time.timeScale = Paused ? 0f : 1f;
    }

    public void EndLevel(bool win)
    {
        if (!Running || ending) return;
        ending = true;
        Running = false;
        LastWin = win;
        PlayerOrb player = PlayerOrb.Instance;
        Vector3 at = player != null ? player.transform.position : Vector3.zero;
        int heartsLeft = player != null ? player.Vitals.Hearts : 0;
        if (win) RunWon?.Invoke(at);
        else RunLost?.Invoke(at);
        StartCoroutine(EndRoutine(win, heartsLeft));
    }

    IEnumerator EndRoutine(bool win, int heartsLeft)
    {
        if (win) Time.timeScale = 0.35f;
        yield return new WaitForSecondsRealtime(win ? 0.9f : 1.0f);
        Time.timeScale = 1f;

        LastStars = win ? Mathf.Clamp(heartsLeft, 1, 3) : 0;
        LastGems = win ? Shards * 3 + LastStars * 10 : Shards;
        LastNewBest = win && LastStars > GameData.GetStars(GameData.SelectedLevel);
        if (win) GameData.SetStarsIfBetter(GameData.SelectedLevel, LastStars);
        GameData.AddGems(LastGems);
        PlayerPrefs.Save();

        UIManager.Instance.ShowResults();
    }

    public void Cleanup()
    {
        StopAllCoroutines();
        Running = false;
        Paused = false;
        ending = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (world != null) Destroy(world);
        world = null;
    }
}
