using System;
using UnityEngine;

[RequireComponent(typeof(OrbVitals))]
public class PlayerOrb : MonoBehaviour
{
    public static PlayerOrb Instance { get; private set; }

    public static event Action<Vector3, Color> Dashed;

    const float BaseSpeed = 6.2f;
    const float Acceleration = 55f;
    const float DashSpeed = 17f;
    const float DashTime = 0.18f;
    public const float DashCooldown = 1.6f;

    public OrbVitals Vitals { get; private set; }
    public float DashReady01 => Mathf.Clamp01(1f - (dashReadyAt - Time.time) / DashCooldown);
    public bool IsDashing => Time.time < dashUntil;

    Rigidbody2D rb;
    SpriteRenderer body;
    SpriteRenderer glow;
    SpriteRenderer shieldRing;
    TrailRenderer trail;
    Color orbColor;
    float speedMultiplier = 1f;
    float boostUntil;
    float dashUntil;
    float dashReadyAt;
    Vector2 dashDirection = Vector2.up;
    Vector2 lastDirection = Vector2.up;
    bool dead;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Dashed = null;

    void Awake()
    {
        Instance = this;
        Vitals = GetComponent<OrbVitals>();
    }

    void OnEnable()
    {
        OrbVitals.Damaged += onOrbDamaged;
        OrbVitals.Died += onOrbDied;
        OrbVitals.ShieldRaised += onShieldRaised;
        OrbVitals.ShieldBlocked += onShieldBlocked;
    }

    void OnDisable()
    {
        OrbVitals.Damaged -= onOrbDamaged;
        OrbVitals.Died -= onOrbDied;
        OrbVitals.ShieldRaised -= onShieldRaised;
        OrbVitals.ShieldBlocked -= onShieldBlocked;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Init(int orbIndex, int hearts, bool shield)
    {
        OrbDef def = Catalog.Orbs[orbIndex];
        orbColor = def.color;
        speedMultiplier = def.speedMultiplier;
        Vitals.Configure(hearts, shield);

        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = 0.36f;

        glow = ArenaDraw.Shape(transform, "orbGlow", "glow", new Color(orbColor.r, orbColor.g, orbColor.b, 0.4f), 2.8f, 2, Vector2.zero);
        body = ArenaDraw.Shape(transform, "orbBody", "circle", orbColor, 0.85f, 4, Vector2.zero);
        ArenaDraw.Shape(body.transform, "orbShine", "circle", new Color(1f, 1f, 1f, 0.55f), 0.3f, 5, new Vector2(-0.18f, 0.2f));
        shieldRing = ArenaDraw.Shape(transform, "orbShieldRing", "ring", OrbPalette.ShieldIce, 1.5f, 6, Vector2.zero);
        shieldRing.gameObject.SetActive(shield);

        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.28f;
        trail.startWidth = 0.55f;
        trail.endWidth = 0f;
        trail.sharedMaterial = OrbBurstFx.ParticleMaterial;
        trail.sortingOrder = 3;
        trail.startColor = new Color(orbColor.r, orbColor.g, orbColor.b, 0.55f);
        trail.endColor = new Color(orbColor.r, orbColor.g, orbColor.b, 0f);

        StartCoroutine(UiAnim.Scale(transform, Vector3.zero, Vector3.one, 0.45f, EaseType.OutBack));
    }

    void Update()
    {
        if (dead || rb == null) return;
        if (Input.GetKeyDown(KeyCode.Space)) TryDash();
        if (boostUntil > 0f && Time.time > boostUntil)
        {
            boostUntil = 0f;
            speedMultiplier = Catalog.Orbs[GameData.SelectedOrb].speedMultiplier;
        }

        Vector2 v = rb.linearVelocity;
        float speed = v.magnitude;
        if (speed > 0.3f)
        {
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            body.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            float stretch = Mathf.Clamp(speed * 0.022f, 0f, IsDashing ? 0.5f : 0.22f);
            body.transform.localScale = new Vector3(0.85f * (1f + stretch), 0.85f * (1f - stretch * 0.6f), 1f);
        }
        else body.transform.localScale = Vector3.Lerp(body.transform.localScale, Vector3.one * 0.85f, 1f - Mathf.Exp(-12f * Time.deltaTime));

        glow.transform.localScale = Vector3.one * (2.8f + 0.2f * Mathf.Sin(Time.time * 4f));
        if (shieldRing.gameObject.activeSelf) shieldRing.transform.Rotate(0f, 0f, 90f * Time.deltaTime);

        Color c = orbColor;
        c.a = Vitals.IsInvulnerable && !IsDashing ? (Mathf.FloorToInt(Time.time * 14f) % 2 == 0 ? 0.35f : 1f) : 1f;
        body.color = IsDashing ? Color.white : c;
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        GameSession session = GameSession.Instance;
        if (dead || session == null || !session.Running) { rb.linearVelocity = Vector2.zero; return; }

        Vector2 input = ThumbStick.Value;
        if (input.sqrMagnitude > 0.04f) lastDirection = input.normalized;

        if (IsDashing) { rb.linearVelocity = dashDirection * DashSpeed; return; }

        float top = BaseSpeed * speedMultiplier;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, input * top, Acceleration * Time.fixedDeltaTime);
    }

    public bool TryDash()
    {
        GameSession session = GameSession.Instance;
        if (dead || session == null || !session.Running || session.Paused || Time.time < dashReadyAt) return false;
        Vector2 input = ThumbStick.Value;
        dashDirection = input.sqrMagnitude > 0.1f ? input.normalized : lastDirection;
        dashUntil = Time.time + DashTime;
        dashReadyAt = Time.time + DashCooldown;
        Vitals.GrantInvulnerability(DashTime + 0.05f);
        Dashed?.Invoke(transform.position, orbColor);
        return true;
    }

    public bool TakeDamage(Vector3 from) => Vitals.TryHurt(from);

    public void Boost(float seconds)
    {
        speedMultiplier = Catalog.Orbs[GameData.SelectedOrb].speedMultiplier * 1.6f;
        boostUntil = Time.time + seconds;
    }

    void onOrbDamaged(OrbHitReport report)
    {
        if (rb == null) return;
        Vector2 away = ((Vector2)transform.position - (Vector2)report.source).normalized;
        rb.linearVelocity = away * 9f;
    }

    void onOrbDied(Vector3 at)
    {
        dead = true;
        if (rb == null) return;
        rb.linearVelocity = Vector2.zero;
        body.enabled = false;
        glow.enabled = false;
        shieldRing.gameObject.SetActive(false);
        trail.emitting = false;
    }

    void onShieldRaised(Vector3 at)
    {
        if (shieldRing == null) return;
        shieldRing.gameObject.SetActive(true);
        shieldRing.transform.localScale = Vector3.one * 1.5f;
    }

    void onShieldBlocked(Vector3 at)
    {
        if (shieldRing != null) shieldRing.gameObject.SetActive(false);
    }
}
