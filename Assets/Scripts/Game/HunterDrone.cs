using UnityEngine;

public class HunterDrone : MonoBehaviour
{
    enum Phase { Chase, Warn, Lunge, Recover }

    Phase phase = Phase.Chase;
    float chaseSpeed;
    float phaseEnd;
    float nextAttackTime;
    Vector2 lungeDirection;
    SpriteRenderer body;
    SpriteRenderer eye;
    Transform visual;
    readonly Color baseColor = OrbPalette.DroneRed;

    public void Setup(float speed)
    {
        chaseSpeed = speed;
        nextAttackTime = Time.time + 2f;

        visual = new GameObject("droneVisual").transform;
        visual.SetParent(transform, false);
        ArenaDraw.Shape(visual, "droneGlow", "glow", new Color(baseColor.r, baseColor.g, baseColor.b, 0.35f), 2.4f, 1, Vector2.zero);
        body = ArenaDraw.Shape(visual, "droneBody", "hex", baseColor, 1.1f, 3, Vector2.zero);
        eye = ArenaDraw.Shape(visual, "droneEye", "circle", Color.white, 0.38f, 4, Vector2.zero);

        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;
        StartCoroutine(UiAnim.Scale(transform, Vector3.zero, Vector3.one, 0.5f, EaseType.OutBack));
    }

    void Update()
    {
        GameSession session = GameSession.Instance;
        PlayerOrb player = PlayerOrb.Instance;
        if (session == null || !session.Running || player == null) return;

        Vector2 pos = transform.position;
        Vector2 toPlayer = (Vector2)player.transform.position - pos;
        float distance = toPlayer.magnitude;
        Vector2 dir = distance > 0.001f ? toPlayer / distance : Vector2.up;
        eye.transform.localPosition = dir * 0.18f;
        visual.Rotate(0f, 0f, 40f * Time.deltaTime);
        body.color = baseColor;

        switch (phase)
        {
            case Phase.Chase:
                transform.position += (Vector3)(dir * (chaseSpeed + session.Elapsed * 0.01f) * Time.deltaTime);
                if (distance < 3.4f && Time.time >= nextAttackTime)
                {
                    phase = Phase.Warn;
                    phaseEnd = Time.time + 0.55f;
                    OrbBurstFx.FloatingText(transform.position + Vector3.up * 0.9f, "!", OrbPalette.DamageCoral, 8f);
                    OrbSfxPlayer.Play(ArenaCue.SpikeSurge, 0.7f, 0.6f);
                }
                break;

            case Phase.Warn:
                visual.localPosition = Random.insideUnitCircle * 0.05f;
                body.color = Mathf.FloorToInt(Time.time * 16f) % 2 == 0 ? Color.white : baseColor;
                transform.localScale = Vector3.one * (1f + 0.12f * Mathf.Sin(Time.time * 30f));
                if (Time.time >= phaseEnd)
                {
                    phase = Phase.Lunge;
                    phaseEnd = Time.time + 0.4f;
                    lungeDirection = dir;
                    visual.localPosition = Vector3.zero;
                    transform.localScale = Vector3.one;
                }
                break;

            case Phase.Lunge:
                transform.position += (Vector3)(lungeDirection * 11f * Time.deltaTime);
                if (Time.time >= phaseEnd) { phase = Phase.Recover; phaseEnd = Time.time + 0.9f; }
                break;

            case Phase.Recover:
                body.color = Color.Lerp(baseColor, UiTheme.Outline, 0.6f);
                transform.position += (Vector3)(dir * chaseSpeed * 0.25f * Time.deltaTime);
                if (Time.time >= phaseEnd) { phase = Phase.Chase; nextAttackTime = Time.time + 1.2f; }
                break;
        }
        KeepInArena(session);
    }

    void KeepInArena(GameSession session)
    {
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, session.Arena.xMin + 0.4f, session.Arena.xMax - 0.4f);
        p.y = Mathf.Clamp(p.y, session.Arena.yMin + 0.4f, session.Arena.yMax - 0.4f);
        transform.position = p;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerOrb player = other.GetComponent<PlayerOrb>();
        if (player != null) player.TakeDamage(transform.position);
    }
}
