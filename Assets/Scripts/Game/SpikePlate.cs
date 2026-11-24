using UnityEngine;

public class SpikePlate : MonoBehaviour
{
    enum Phase { Idle, Warn, Active }

    Phase phase = Phase.Idle;
    float phaseStart;
    float phaseLength;
    SpriteRenderer baseRing;
    Transform spikes;
    readonly Color danger = OrbPalette.DroneRed;

    public void Setup(float startDelay)
    {
        baseRing = ArenaDraw.Shape(transform, "plateBase", "ring", new Color(1f, 1f, 1f, 0.2f), 1.9f, -4, Vector2.zero);
        SpriteRenderer spikeSprite = ArenaDraw.Shape(transform, "plateSpikes", "star", danger, 1.8f, 0, Vector2.zero);
        spikes = spikeSprite.transform;
        spikes.localScale = Vector3.zero;

        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.75f;

        phaseStart = Time.time + startDelay;
        phaseLength = 1.8f;
    }

    void Update()
    {
        GameSession session = GameSession.Instance;
        if (session == null || !session.Running) return;
        float elapsed = Time.time - phaseStart;
        if (elapsed < 0f) return;

        switch (phase)
        {
            case Phase.Idle:
                baseRing.color = new Color(1f, 1f, 1f, 0.2f);
                spikes.localScale = Vector3.Lerp(spikes.localScale, Vector3.zero, 1f - Mathf.Exp(-14f * Time.deltaTime));
                if (elapsed >= phaseLength) Next(Phase.Warn, 0.8f);
                break;
            case Phase.Warn:
                baseRing.color = Mathf.FloorToInt(Time.time * 10f) % 2 == 0 ? danger : new Color(danger.r, danger.g, danger.b, 0.3f);
                spikes.localScale = Vector3.one * 0.25f;
                if (elapsed >= phaseLength)
                {
                    Next(Phase.Active, 1.0f);
                    OrbSfxPlayer.Play(ArenaCue.SpikeSurge, 1.2f, 0.8f);
                }
                break;
            case Phase.Active:
                baseRing.color = danger;
                spikes.localScale = Vector3.one * Mathf.Max(0.25f, Ease.Evaluate(EaseType.OutBack, elapsed / 0.18f));
                if (elapsed >= phaseLength) Next(Phase.Idle, Random.Range(1.6f, 2.4f));
                break;
        }
    }

    void Next(Phase next, float length)
    {
        phase = next;
        phaseStart = Time.time;
        phaseLength = length;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (phase != Phase.Active) return;
        PlayerOrb player = other.GetComponent<PlayerOrb>();
        if (player != null) player.TakeDamage(transform.position);
    }
}
