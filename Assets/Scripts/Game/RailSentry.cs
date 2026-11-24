using UnityEngine;

public class RailSentry : MonoBehaviour
{
    Vector2 railStart;
    Vector2 railEnd;
    float tripSeconds;
    float clock;
    Transform visual;

    public void Setup(Vector2 from, Vector2 to, float secondsPerTrip, float startOffset)
    {
        railStart = from;
        railEnd = to;
        tripSeconds = secondsPerTrip;
        clock = startOffset;
        transform.position = railStart;

        Vector2 mid = (railStart + railEnd) * 0.5f;
        SpriteRenderer rail = ArenaDraw.Rect(transform.parent, "sentryRail", new Color(1f, 0.36f, 0.36f, 0.14f), new Vector2(Mathf.Abs(railEnd.x - railStart.x), 0.06f), -5, mid);
        rail.transform.position = mid;

        visual = new GameObject("sentryVisual").transform;
        visual.SetParent(transform, false);
        Color c = OrbPalette.SentryOrange;
        ArenaDraw.Shape(visual, "sentryGlow", "glow", new Color(c.r, c.g, c.b, 0.35f), 2.4f, 1, Vector2.zero);
        ArenaDraw.Shape(visual, "sentryBody", "hex", c, 1.15f, 3, Vector2.zero);
        ArenaDraw.Shape(visual, "sentryCore", "circle", UiTheme.BgDeep, 0.45f, 4, Vector2.zero);

        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.55f;
    }

    void Update()
    {
        GameSession session = GameSession.Instance;
        if (session == null || !session.Running) return;
        clock += Time.deltaTime;
        float t = Ease.Evaluate(EaseType.InOutSine, Mathf.PingPong(clock / tripSeconds, 1f));
        transform.position = Vector2.Lerp(railStart, railEnd, t);
        visual.Rotate(0f, 0f, -120f * Time.deltaTime);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerOrb player = other.GetComponent<PlayerOrb>();
        if (player != null) player.TakeDamage(transform.position);
    }
}
